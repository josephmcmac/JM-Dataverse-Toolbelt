using JosephM.Application.ViewModel.Attributes;
using JosephM.Application.ViewModel.Dialog;
using JosephM.Record.Extentions;
using JosephM.Record.Xrm.XrmRecord;
using JosephM.Xrm.Schema;
using JosephM.Xrm.Vsix.Application;
using JosephM.Xrm.Vsix.Module.PackageSettings;
using System;
using System.IO;
using System.Linq;

namespace JosephM.Xrm.Vsix.Module.UpdateAssembly
{
    [RequiresConnection]
    public class UpdateAssemblyDialog : DialogViewModel
    {
        public XrmRecordService Service { get; set; }
        public XrmPackageSettings PackageSettings { get; set; }
        IVisualStudioService VisualStudioService { get; set; }

        public UpdateAssemblyDialog(IDialogController dialogController, IVisualStudioService visualStudioService, XrmRecordService xrmRecordService, XrmPackageSettings packageSettings)
            : base(dialogController)
        {
            VisualStudioService = visualStudioService;
            Service = xrmRecordService;
            PackageSettings = packageSettings;
        }

        protected override void LoadDialogExtention()
        {
            StartNextAction();
        }

        protected override void CompleteDialogExtention()
        {
            var verify = Service.VerifyConnection();
            if (!verify.IsValid)
                throw new Exception($"Update assembly cannot proceed due to a connection error\n\n{string.Join(",", verify.InvalidReasons)}");
            LoadingViewModel.IsLoading = true;
            var response = new UpdateAssemblyResponse();
            CompletionItem = response;
            try
            {
                var assemblyFile = VisualStudioService.BuildSelectedProjectAndGetAssemblyName();
                if (string.IsNullOrWhiteSpace(assemblyFile))
                {
                    response.CompletionMessage = "Could Not Find Built Assembly. Check The Build Results";
                }
                else
                {
                    var fileInfo = new FileInfo(assemblyFile);
                    var assemblyName = fileInfo.Name.Substring(0,
                        fileInfo.Name.LastIndexOf(fileInfo.Extension, StringComparison.Ordinal));

                    var bytes = File.ReadAllBytes(assemblyFile);
                    var assemblyContent = Convert.ToBase64String(bytes);

                    var preAssembly = Service.GetFirst(Entities.pluginassembly, Fields.pluginassembly_.name, assemblyName);
                    if (preAssembly == null)
                    {
                        response.CompletionMessage = "There is no plugin assembly deployed in the dynamics instance with a matching name. Try the deploy assembly option to deploy a new plugin assembly, or rename the assembly to match an existing assembly deployed to the instance";
                    }
                    else
                    {
                        //okay first create/update the plugin assembly
                        var assemblyRecord = Service.NewRecord(Entities.pluginassembly);
                        assemblyRecord.Id = preAssembly.Id;
                        if (preAssembly.Id != null)
                            assemblyRecord.SetField(Fields.pluginassembly_.pluginassemblyid, preAssembly.Id, Service);
                        assemblyRecord.SetField(Fields.pluginassembly_.content, assemblyContent, Service);
                        var matchField = Fields.pluginassembly_.pluginassemblyid;

                        var assemblyLoadResponse = Service.LoadIntoCrm(new[] { assemblyRecord }, matchField);
                        if (assemblyLoadResponse.Errors.Any())
                        {
                            throw new Exception("Error Updating Assembly", assemblyLoadResponse.Errors.Values.First());
                        }
                        //add plugin assembly to the solution
                        var componentType = OptionSets.SolutionComponent.ObjectTypeCode.PluginAssembly;
                        var itemsToAdd = assemblyLoadResponse.Created.Union(assemblyLoadResponse.Updated);
                        if (PackageSettings.AddToSolution)
                            Service.AddSolutionComponents(PackageSettings.Solution.Id, componentType, itemsToAdd.Select(i => i.Id));

                        response.CompletionMessage = "Assembly Updated";
                    }
                }
            }
            catch(Exception ex)
            {
                response.CompletionMessage = ex.XrmDisplayString();
            }
            finally
            {
                LoadingViewModel.IsLoading = false;
            }
        }
    }
}