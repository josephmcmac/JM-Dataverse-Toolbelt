using System;
using System.IO;
using System.Linq;
using JosephM.Application.ViewModel.Dialog;
using JosephM.Record.Extentions;
using JosephM.Record.Xrm.XrmRecord;
using JosephM.Xrm.Schema;
using JosephM.XRM.VSIX.Utilities;

namespace JosephM.XRM.VSIX.Commands.UpdateAssembly
{
    public class UpdateAssemblyDialog : DialogViewModel
    {
        public string AssemblyFile { get; set; }
        public XrmRecordService Service { get; set; }
        public XrmPackageSettings PackageSettings { get; set; }

        public UpdateAssemblyDialog(IDialogController dialogController, string assemblyFile, XrmRecordService xrmRecordService, XrmPackageSettings packageSettings)
            : base(dialogController)
        {
            AssemblyFile = assemblyFile;
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

            var assemblyFile = AssemblyFile;
            var fileInfo = new FileInfo(AssemblyFile);
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

            CompletionMessage = "Assembly Updated";

            LoadingViewModel.IsLoading = false;
        }
    }
}