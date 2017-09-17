using JosephM.Application.ViewModel.Dialog;
using JosephM.Core.FieldType;
using JosephM.ObjectMapping;
using JosephM.Prism.XrmModule.SavedXrmConnections;
using JosephM.Xrm.ImportExporter.Service;
using JosephM.XRM.VSIX.Dialogs;
using JosephM.XRM.VSIX.Utilities;
using System;

namespace JosephM.XRM.VSIX.Commands.CreateDeploymentPackage
{
    internal sealed class CreateDeploymentPackageCommand : CommandBase<CreateDeploymentPackageCommand>
    {
        public override int CommandId
        {
            get { return 0x010E; }
        }

        public override void DoDialog()
        {
            var settings = VsixUtility.GetPackageSettings(GetDte2());
            if (settings == null)
                settings = new XrmPackageSettings();

            if (settings.Solution == null)
                throw new NullReferenceException("Solution is not populated in the package settings");

            var xrmRecordService = GetXrmRecordService();
            var visualStudioService = GetVisualStudioService();

            var savedConnection = SavedXrmRecordConfiguration.CreateNew(xrmRecordService.XrmRecordConfiguration);

            //WARNING THIS FOLDER IS CLEARED BEFORE PROCESSING SO CAREFUL IF CHANGE DIRECTORY
            var folderPath = visualStudioService.SolutionDirectory + "/TempSolutionFolder";

            var request = XrmSolutionImporterExporterRequest.CreateForCreatePackage(folderPath, settings.Solution);

            var controller = CreateDialogController();

            var service = new XrmSolutionImporterExporterService(xrmRecordService);

            var dialog = new CreateDeploymentPackageDialog(service, request, controller, settings, visualStudioService);

            DialogUtility.LoadDialog(dialog);
        }
    }
}
