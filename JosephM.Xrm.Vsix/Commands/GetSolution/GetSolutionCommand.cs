using JosephM.Application.ViewModel.Dialog;
using JosephM.Core.FieldType;
using JosephM.ObjectMapping;
using JosephM.Prism.XrmModule.SavedXrmConnections;
using JosephM.Xrm.ImportExporter.Service;
using JosephM.XRM.VSIX.Dialogs;
using JosephM.XRM.VSIX.Utilities;
using System;

namespace JosephM.XRM.VSIX.Commands.GetSolution
{
    internal sealed class GetSolutionCommand : CommandBase<GetSolutionCommand>
    {
        public override int CommandId
        {
            get { return 0x0107; }
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

            var mapper = new ClassSelfMapper();
            var savedConnection = new SavedXrmRecordConfiguration();
            mapper.Map(xrmRecordService.XrmRecordConfiguration, savedConnection);

            var folderPath = visualStudioService.SolutionDirectory + "/Customisations";

            var request = new XrmSolutionImporterExporterRequest()
            {
                ImportExportTask = SolutionImportExportTask.ExportSolutions,
                FolderPath = new Folder(folderPath),
                SolutionExports = new[]
                   {
                       new SolutionExport()
                       {
                            Connection = savedConnection,
                            ExportAsManaged = false,
                            Solution = settings.Solution
                       }
                   }
            };

            var controller = new DialogController(new VsixApplicationController("VSIX", null));

            var service = new XrmSolutionImporterExporterService();

            var dialog = new GetSolutionDialog(service, request, controller, xrmRecordService, settings, visualStudioService);

            DialogUtility.LoadDialog(dialog);
        }
    }
}
