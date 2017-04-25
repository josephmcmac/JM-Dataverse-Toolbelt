using JosephM.Application.ViewModel.Dialog;
using JosephM.Core.Extentions;
using JosephM.Core.Service;
using JosephM.Record.Extentions;
using JosephM.Record.IService;
using JosephM.Record.Query;
using JosephM.Record.Xrm.XrmRecord;
using JosephM.Xrm.Schema;
using JosephM.XRM.VSIX.Utilities;
using Microsoft.Practices.Prism;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace JosephM.XRM.VSIX.Commands.DeployAssembly
{
    public class DeployAssemblyDialog : DialogViewModel
    {
        public string AssemblyFile { get; set; }
        public XrmRecordService Service { get; set; }
        public XrmPackageSettings PackageSettings { get; set; }

        public DeployAssemblyDialog(IDialogController dialogController, string assemblyFile, XrmRecordService xrmRecordService, XrmPackageSettings packageSettings)
            : base(dialogController)
        {
            PackageSettings = packageSettings;
            AssemblyFile = assemblyFile;
            Service = xrmRecordService;

            var configEntryDialog = new ObjectGetEntryDialog(() => PluginAssembly, this, ApplicationController, Service);
            SubDialogs = new DialogViewModel[] { configEntryDialog };
        }

        protected override void LoadDialogExtention()
        {
            StartNextAction();
        }

        private PluginAssembly _pluginAssembly;
        public PluginAssembly PluginAssembly
        {
            get
            {
                if (_pluginAssembly == null)
                {
                    Load();
                }
                return _pluginAssembly;
            }
        }

        private void Load()
        {
            var assemblyFile = AssemblyFile;
            var fileInfo = new FileInfo(AssemblyFile);
            var assemblyName = fileInfo.Name.Substring(0,
                fileInfo.Name.LastIndexOf(fileInfo.Extension, StringComparison.Ordinal));

            var bytes = File.ReadAllBytes(assemblyFile);
            var assemblyContent = Convert.ToBase64String(bytes);

            //okay this crazy stuff is to load the assembly dll in a different app domain
            //that way it may be loaded, inspected, then released (removing any lock on the assembly file)
            //when the appdomain is unloaded
            //the loading and inspection is done in an object PluginAssemblyReader
            //which is loaded in the context of the separate app domain
            var myDomain = AppDomain.CreateDomain("JosephM.XRM.VSIX.DeployAssemblyCommand", null, null);
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
            IEnumerable<PluginAssemblyReader.PluginType> plugins;
            try
            {

                var reader =
                    (PluginAssemblyReader)
                    myDomain.CreateInstanceFrom(
                        Assembly.GetExecutingAssembly().Location,
                        typeof(PluginAssemblyReader).FullName).Unwrap();
                var loadedPlugins = reader.LoadTypes(assemblyFile);
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
            if(!plugins.Any())
                throw new Exception("There are no plugin classes in the assembly");

            _pluginAssembly = new PluginAssembly();
            PluginAssembly.Name = assemblyName;
            PluginAssembly.Content = assemblyContent;

            var preAssembly = Service.GetFirst(Entities.pluginassembly, Fields.pluginassembly_.name, assemblyName);
            if (preAssembly != null)
            {
                PluginAssembly.Id = preAssembly.Id;
                PluginAssembly.IsolationMode = preAssembly.GetOptionKey(Fields.pluginassembly_.isolationmode).ParseEnum<PluginAssembly.IsolationMode_>();
            }

            var pluginTypes = new List<PluginType>();
            PluginAssembly.PluginTypes = pluginTypes;
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
            PreTypeRecords = preAssembly == null 
                ? new IRecord[0]
                : Service.RetrieveAllAndClauses(Entities.plugintype, new[] { new Condition(Fields.plugintype_.pluginassemblyid, ConditionType.Equal, PluginAssembly.Id) });

            foreach (var item in PreTypeRecords)
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
                    PreTypeRecords.Where(pt => pt.GetStringField(Fields.plugintype_.typename) == item.TypeName);
                if (matchingItems.Any())
                {
                    var matchingItem = matchingItems.First();
                    item.Id = matchingItem.Id;
                    item.Name = matchingItem.GetStringField(Fields.plugintype_.friendlyname);
                    item.GroupName = matchingItem.GetStringField(Fields.plugintype_.workflowactivitygroupname);
                }
            }
        }

        public IEnumerable<IRecord> PreTypeRecords { get; set; }

        protected override void CompleteDialogExtention()
        {
            var responses = new List<PluginTypeErrors>();

            var service = Service;

            var removedPlugins = PreTypeRecords.Where(ptr => PluginAssembly.PluginTypes.All(pt => pt.Id != ptr.Id)).ToArray();
            foreach (var record in removedPlugins)
            {
                try
                {
                    service.Delete(record);
                }
                catch (Exception ex)
                {
                    throw new Exception(string.Format("Error deleting plugin {0}", record.GetStringField(Fields.plugintype_.name)), ex);
                }
            }

            //okay first create/update the plugin assembly
            var assemblyRecord = service.NewRecord(Entities.pluginassembly);
            assemblyRecord.Id = PluginAssembly.Id;
            if (PluginAssembly.Id != null)
                assemblyRecord.SetField(Fields.pluginassembly_.pluginassemblyid, PluginAssembly.Id, service);
            assemblyRecord.SetField(Fields.pluginassembly_.name, PluginAssembly.Name, service);
            assemblyRecord.SetField(Fields.pluginassembly_.content, PluginAssembly.Content, service);
            assemblyRecord.SetField(Fields.pluginassembly_.isolationmode, (int)PluginAssembly.IsolationMode, service);
            var matchField = Fields.pluginassembly_.pluginassemblyid;

            var assemblyLoadResponse = VsixUtility.LoadIntoCrm(service, new[] { assemblyRecord }, matchField);
            if (assemblyLoadResponse.Errors.Any())
            {
                throw new Exception("Error Updating Assembly", assemblyLoadResponse.Errors.Values.First());
            }
            //okay create/update the plugin types
            var pluginTypes = new List<IRecord>();
            foreach (var pluginType in PluginAssembly.PluginTypes)
            {
                var pluginTypeRecord = service.NewRecord(Entities.plugintype);
                pluginTypeRecord.Id = pluginType.Id;
                if (pluginType.Id != null)
                    pluginTypeRecord.SetField(Fields.plugintype_.plugintypeid, pluginType.Id, service);
                pluginTypeRecord.SetField(Fields.plugintype_.typename, pluginType.TypeName, service);
                pluginTypeRecord.SetField(Fields.plugintype_.name, pluginType.Name, service);
                pluginTypeRecord.SetField(Fields.plugintype_.friendlyname, pluginType.FriendlyName, service);
                pluginTypeRecord.SetField(Fields.plugintype_.assemblyname, PluginAssembly.Name, service);
                pluginTypeRecord.SetLookup(Fields.plugintype_.pluginassemblyid, assemblyRecord.Id, assemblyRecord.Type);
                pluginTypeRecord.SetField(Fields.plugintype_.isworkflowactivity, pluginType.IsWorkflowActivity, service);
                pluginTypeRecord.SetField(Fields.plugintype_.workflowactivitygroupname, pluginType.GroupName, service);
                pluginTypes.Add(pluginTypeRecord);
            }

            var pluginTypeLoadResponse = VsixUtility.LoadIntoCrm(service, pluginTypes, Fields.plugintype_.plugintypeid);

            foreach (var item in pluginTypeLoadResponse.Errors)
            {
                var responseItem = new PluginTypeErrors();
                responseItem.Name = item.Key.GetStringField(Fields.plugintype_.typename);
                responseItem.Exception = item.Value;
                responses.Add(responseItem);
            }
            CompletionItems.AddRange(responses);

            //add plugin assembly to the solution
            var componentType = OptionSets.SolutionComponent.ObjectTypeCode.PluginAssembly;
            var itemsToAdd = assemblyLoadResponse.Created.Union(assemblyLoadResponse.Updated);
            VsixUtility.AddSolutionComponents(service, PackageSettings, componentType, itemsToAdd);

            if (responses.Any())
                CompletionMessage = "There Were Errors Thrown Updating The Plugins";
            else
                CompletionMessage = "Plugins Updated";
        }

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

        public class PluginTypeErrors : ServiceResponseItem
        {
            public string Name { get; set; }
        }
    }
}