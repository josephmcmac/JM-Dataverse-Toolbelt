using System;
using System.Linq;
using JosephM.Application.ViewModel.Dialog;
using JosephM.Record.Xrm.XrmRecord;
using JosephM.XRM.VSIX.Utilities;
using JosephM.Xrm.ImportExporter.Service;
using JosephM.XRM.VSIX.Dialogs;
using JosephM.ObjectMapping;
using JosephM.Prism.XrmModule.SavedXrmConnections;
using JosephM.Core.FieldType;

namespace JosephM.XRM.VSIX.Commands.GetSolution
{
    public class GetSolutionDialog : DialogViewModel
    {
        public XrmRecordService Service { get; set; }
        public XrmPackageSettings PackageSettings { get; set; }
        public IVisualStudioService VisualStudioService { get; set; }

        public GetSolutionDialog(IDialogController dialogController, XrmRecordService xrmRecordService, XrmPackageSettings packageSettings, IVisualStudioService visualStudioService)
            : base(dialogController)
        {
            Service = xrmRecordService;
            PackageSettings = packageSettings;
            VisualStudioService = visualStudioService;
        }

        protected override void LoadDialogExtention()
        {
            StartNextAction();
        }

        protected override void CompleteDialogExtention()
        {
            LoadingViewModel.IsLoading = true;

            if (PackageSettings.Solution == null)
                throw new NullReferenceException("Solution is not populated in the package settings");

            var mapper = new ClassSelfMapper();
            var savedConnection = new SavedXrmRecordConfiguration();
            mapper.Map(Service.XrmRecordConfiguration, savedConnection);

            var folderPath = VisualStudioService.SolutionDirectory + "/Customisations";

            var request = new XrmSolutionImporterExporterRequest()
            {
                 ImportExportTask = SolutionImportExportTask.ExportSolutions,
                  FolderPath = new Folder(folderPath),
                   SolutionExports = new []
                   {
                       new SolutionExport()
                       {
                            Connection = savedConnection,
                            ExportAsManaged = false,
                            Solution = PackageSettings.Solution  
                       }
                   }
            };

            var service = new XrmSolutionImporterExporterService();

            var dialog = new VsixServiceDialog<XrmSolutionImporterExporterService, XrmSolutionImporterExporterRequest, XrmSolutionImporterExporterResponse, XrmSolutionImporterExporterResponseItem>(
                service,
                request,
                new DialogController(new VsixApplicationController("VSIX", null)));

            DialogUtility.LoadDialog(dialog);

            if (dialog.Response.ExportedSolutions.Any())
            {
                VisualStudioService.AddSolutionItem(dialog.Response.ExportedSolutions.First().SolutionFile.FileName);
                CompletionMessage = "Solution Updated In Visual Studio Solution";
            }
            else
            {
                throw new Exception("Error the response did not contain exported solutions");
            }


            LoadingViewModel.IsLoading = false;
        }
    }
}