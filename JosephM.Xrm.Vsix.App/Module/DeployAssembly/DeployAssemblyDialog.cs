using JosephM.Application.Desktop.Module.ServiceRequest;
using JosephM.Application.ViewModel.Attributes;
using JosephM.Application.ViewModel.Dialog;
using JosephM.Core.Extentions;
using JosephM.Record.Extentions;
using JosephM.Record.IService;
using JosephM.Record.Query;
using JosephM.Record.Xrm.XrmRecord;
using JosephM.Xrm.Schema;
using JosephM.Xrm.Vsix.App.Extensions;
using JosephM.Xrm.Vsix.Application;
using JosephM.Xrm.Vsix.DeployAssembly.AssemblyReader;
using JosephM.Xrm.Vsix.Module.PackageSettings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace JosephM.Xrm.Vsix.Module.DeployAssembly
{
    [RequiresConnection]
    public class DeployAssemblyDialog : ServiceRequestDialog<DeployAssemblyService, DeployAssemblyRequest, DeployAssemblyResponse, DeployAssemblyResponseItem>
    {
        public XrmRecordService XrmRecordService { get; set; }
        public XrmPackageSettings PackageSettings { get; set; }
        IVisualStudioService VisualStudioService { get; set; }

        public DeployAssemblyDialog(DeployAssemblyService service, IDialogController dialogController, IVisualStudioService visualStudioService, XrmRecordService xrmRecordService, XrmPackageSettings packageSettings)
            : base(service, dialogController, null, nextButtonLabel: "Deploy")
        {
            VisualStudioService = visualStudioService;
            PackageSettings = packageSettings;
            XrmRecordService = xrmRecordService;
        }

        protected override void CompleteDialogExtention()
        {
            if (AssemblyLoadErrorMessage != null)
            {
                Response = new DeployAssemblyResponse
                {
                    Message = AssemblyLoadErrorMessage
                };
                CompletionItem = Response;
            }
            else
            {
                base.CompleteDialogExtention();
            }
        }

        private string AssemblyLoadErrorMessage { get; set; }

        protected override void LoadDialogExtention()
        {
            LoadingViewModel.LoadingMessage = "Please wait while building and loading assembly information. This may take a while";
            //hijack the load method so that we can prepopulate
            //the entered request with various details
            AssemblyLoadErrorMessage = LoadAssemblyDetails();
            if (AssemblyLoadErrorMessage != null)
                SkipObjectEntry = true;
            base.LoadDialogExtention();
        }

        private string LoadAssemblyDetails()
        {
            var selectedProjectName = VisualStudioService.GetSelectedProjectName();
            var addIlMergePath = PackageSettings.AddIlMergePathForProject(selectedProjectName);
            AssemblyFile = VisualStudioService.BuildSelectedProjectAndGetAssemblyName(addIlMergePath);
            if (string.IsNullOrWhiteSpace(AssemblyFile))
                return "Could Not Find Built Assembly. Check The Build Result For Errors";


            var fileInfo = new FileInfo(AssemblyFile);
            var assemblyName = fileInfo.Name.Substring(0,
                fileInfo.Name.LastIndexOf(fileInfo.Extension, StringComparison.Ordinal));

            var bytes = File.ReadAllBytes(AssemblyFile);
            var assemblyContent = Convert.ToBase64String(bytes);

            //okay this crazy stuff is to load the assembly dll in a different app domain
            //that way it may be loaded, inspected, then released (removing any lock on the assembly file)
            //when the appdomain is unloaded
            //the loading and inspection is done in an object PluginAssemblyReader
            //which is loaded in the context of the separate app domain
            var myDomain = AppDomain.CreateDomain("JosephM.XRM.VSIX.DeployAssemblyCommand", null, null);
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
            IEnumerable<PluginAssemblyReader.PluginType> plugins;
            bool isSigned = false;
            try
            {
                var reader =
                    (PluginAssemblyReader)
                    myDomain.CreateInstanceFrom(
                        Assembly.GetExecutingAssembly().Location,
                        typeof(PluginAssemblyReader).FullName).Unwrap();
                var loadResponse = reader.LoadTypes(AssemblyFile, addIlMergePath);
                isSigned = loadResponse.IsSigned;
                var loadedPlugins = loadResponse.PluginTypes;
                plugins =
                    loadedPlugins.Select(p => new PluginAssemblyReader.PluginType(p.Type, p.TypeName)).ToArray();
            }
            finally
            {
                AppDomain.CurrentDomain.AssemblyResolve -= CurrentDomain_AssemblyResolve;
                if (null != myDomain)
                {
                    AppDomain.Unload(myDomain);
                }
            }
            if (!isSigned)
                return "Assembly Cannot By Deployed. You Need To Sign The Assembly With A Strong Named Key File";
            if (!plugins.Any())
                return "Assembly Cannot By Deployed. No Plugin Classes Were Found In The Assembly";

            Request.AssemblyName = assemblyName;
            Request.SetContent(assemblyContent);

            var preAssembly = XrmRecordService.GetFirst(Entities.pluginassembly, Fields.pluginassembly_.name, assemblyName);
            if (preAssembly != null)
            {
                Request.Id = preAssembly.Id;
                Request.IsolationMode = preAssembly.GetOptionKey(Fields.pluginassembly_.isolationmode).ParseEnum<DeployAssemblyRequest.IsolationMode_>();
            }

            var pluginTypes = new List<PluginType>();
            Request.PluginTypes = pluginTypes;
            foreach (var item in plugins)
            {
                var pluginType = new PluginType();
                pluginType.TypeName = item.TypeName;
                //get the type name after "." as the name
                var startIndex = item.TypeName.LastIndexOf(".", StringComparison.Ordinal);
                if (startIndex != -1 && item.TypeName.Length > startIndex + 1)
                    startIndex = startIndex + 1;
                else
                    startIndex = 0;
                pluginType.FriendlyName = item.TypeName.Substring(startIndex);
                pluginType.Name = item.TypeName.Substring(startIndex);
                pluginType.IsWorkflowActivity = item.Type == PluginAssemblyReader.PluginType.XrmPluginType.WorkflowActivity;
                pluginType.InAssembly = true;
                pluginTypes.Add(pluginType);
            }
            Request.SetPreTypeRecords(preAssembly == null
                ? new IRecord[0]
                : XrmRecordService.RetrieveAllAndClauses(Entities.plugintype, new[] { new Condition(Fields.plugintype_.pluginassemblyid, ConditionType.Equal, Request.Id) }));

            foreach (var item in Request.GetPreTypeRecords())
            {
                var matchingItems =
                    pluginTypes.Where(pt => item.GetStringField(Fields.plugintype_.typename) == pt.TypeName);
                var matchingItem = matchingItems.Any()
                    ? matchingItems.First()
                    : null;
                if (matchingItem == null)
                {
                    //if the plugin was not loaded by the types in the assembly
                    //then create a new one
                    matchingItem = new PluginType();
                    pluginTypes.Add(matchingItem);
                    matchingItem.TypeName = item.GetStringField(Fields.plugintype_.typename);
                    matchingItem.IsWorkflowActivity = item.GetBoolField(Fields.plugintype_.isworkflowactivity);
                }
                matchingItem.Id = item.Id;
                matchingItem.FriendlyName = item.GetStringField(Fields.plugintype_.friendlyname);
                matchingItem.Name = item.GetStringField(Fields.plugintype_.name);
                matchingItem.GroupName = item.GetStringField(Fields.plugintype_.workflowactivitygroupname);
            }

            foreach (var item in pluginTypes)
            {
                var matchingItems =
                    Request.GetPreTypeRecords().Where(pt => pt.GetStringField(Fields.plugintype_.typename) == item.TypeName);
                if (matchingItems.Any())
                {
                    var matchingItem = matchingItems.First();
                    item.IsDeployed = true;
                    item.Id = matchingItem.Id;
                    item.Name = matchingItem.GetStringField(Fields.plugintype_.friendlyname);
                    item.GroupName = matchingItem.GetStringField(Fields.plugintype_.workflowactivitygroupname);
                }
            }

            return null;
        }

        public string AssemblyFile { get; private set; }

        //code copied - http://weblog.west-wind.com/posts/2009/Jan/19/Assembly-Loading-across-AppDomains
        private Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            try
            {
                var assembly = Assembly.Load(args.Name);
                if (assembly != null)
                    return assembly;
            }
            catch
            {
                // ignore load error
            }

            // *** Try to load by filename - split out the filename of the full assembly name
            // *** and append the base path of the original assembly (ie. look in the same dir)
            // *** NOTE: this doesn't account for special search paths but then that never
            //           worked before either.
            var parts = args.Name.Split(',');
            var file = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\" + parts[0].Trim() +
                          ".dll";

            return Assembly.LoadFrom(file);
        }

        protected override IDictionary<string, string> GetPropertiesForCompletedLog()
        {
            var dictionary = base.GetPropertiesForCompletedLog();
            void addProperty(string name, string value)
            {
                if (!dictionary.ContainsKey(name))
                    dictionary.Add(name, value);
            }
            if(Request != null)
            {
                addProperty("Isolation Mode", Request.IsolationMode.ToString());
                if(Request.PluginTypes != null)
                {
                    var pluginCount = Request.PluginTypes.Count(pt => !pt.IsWorkflowActivity);
                    var workflowCount = Request.PluginTypes.Count(pt => pt.IsWorkflowActivity);
                    addProperty("Plugin Count", pluginCount.ToString());
                    addProperty("Workflow Count", workflowCount.ToString());
                }

            }
            return dictionary;
        }
    }
}