using JosephM.Application.ViewModel.Dialog;
using JosephM.Core.FieldType;
using JosephM.Core.Utility;
using JosephM.Record.Xrm.XrmRecord;
using JosephM.Xrm.DataImportExport.XmlImport;
using JosephM.Xrm.Vsix.Application;

namespace JosephM.Xrm.Vsix.Module.AddReleaseData
{
    public class AddReleaseDataDialog : ExportXmlDialog
    {
        public AddReleaseDataDialog(ExportXmlService service, IDialogController dialogController, XrmRecordService xrmRecordService, IVisualStudioService visualStudioService)
            : base(service, dialogController, xrmRecordService)
        {
            VisualStudioService = visualStudioService;
        }

        private string _solutionFolder;

        public IVisualStudioService VisualStudioService { get; }

        protected override void LoadDialogExtention()
        {
            _solutionFolder = Request.Folder.FolderPath;
            Request.HideFolder = true;
            Request.Folder = new Folder(VisualStudioService.SolutionDirectory + "/TempSolutionFolder");
            //WARNING THIS FOLDER IS CLEARED BEFORE PROCESSING SO CAREFUL IF CHANGE DIRECTORY
            FileUtility.DeleteSubFolders(Request.Folder.FolderPath);
            FileUtility.DeleteFiles(Request.Folder.FolderPath);

            base.LoadDialogExtention();
        }

        protected override void CompleteDialogExtention()
        {
            base.CompleteDialogExtention();

            LogController.UpdateProgress(7, 8, "Moving Package Into Solution Folder");

            var folder = Request.Folder.FolderPath;

            FileUtility.MoveWithReplace(folder, _solutionFolder);

            LogController.UpdateProgress(3, 3, "Adding Package To Solution");

            VisualStudioService.AddFolder(_solutionFolder);

            Response.Message = "All data found has been added to the solution";
        }
    }
}
