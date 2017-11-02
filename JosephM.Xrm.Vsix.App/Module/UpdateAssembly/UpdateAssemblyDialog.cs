using JosephM.Application.ViewModel.Dialog;
using JosephM.Record.Extentions;
using JosephM.Record.Xrm.XrmRecord;
using JosephM.Xrm.Schema;
using JosephM.Xrm.Vsix.Utilities;
using JosephM.XRM.VSIX;
using JosephM.XRM.VSIX.Utilities;
using System;
using System.IO;
using System.Linq;

namespace JosephM.Xrm.Vsix.Module.UpdateAssembly
{
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
            LoadingViewModel.IsLoading = true;

            var assemblyFile = VisualStudioService.BuildSelectedProjectAndGetAssemblyName();
            if (string.IsNullOrWhiteSpace(assemblyFile))
            {
                CompletionMessage = "Could Not Find Built Assembly. Check The Build Results";
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
                    throw new NullReferenceException("Assembly Not Deployed. Try Deploy Assembly");
                }

                //okay first create/update the plugin assembly
                var assemblyRecord = Service.NewRecord(Entities.pluginassembly);
                assemblyRecord.Id = preAssembly.Id;
                if (preAssembly.Id != null)
                    assemblyRecord.SetField(Fields.pluginassembly_.pluginassemblyid, preAssembly.Id, Service);
                assemblyRecord.SetField(Fields.pluginassembly_.content, assemblyContent, Service);
                var matchField = Fields.pluginassembly_.pluginassemblyid;

                var assemblyLoadResponse = VsixUtility.LoadIntoCrm(Service, new[] { assemblyRecord }, matchField);
                if (assemblyLoadResponse.Errors.Any())
                {
                    throw new Exception("Error Updating Assembly", assemblyLoadResponse.Errors.Values.First());
                }
                //add plugin assembly to the solution
                var componentType = OptionSets.SolutionComponent.ObjectTypeCode.PluginAssembly;
                var itemsToAdd = assemblyLoadResponse.Created.Union(assemblyLoadResponse.Updated);
                VsixUtility.AddSolutionComponents(Service, PackageSettings, componentType, itemsToAdd);
            }
            CompletionMessage = "Assembly Updated";
            LoadingViewModel.IsLoading = false;
        }
    }
}