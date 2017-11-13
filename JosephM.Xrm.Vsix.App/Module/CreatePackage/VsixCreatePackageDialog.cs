using JosephM.Application.ViewModel.Dialog;
using JosephM.Core.Utility;
using JosephM.Deployment.CreatePackage;
using JosephM.Record.Xrm.XrmRecord;
using JosephM.Xrm.Vsix.Application;
using System.IO;

namespace JosephM.Xrm.Vsix.Module.CreatePackage
{
    public class VsixCreatePackageDialog : CreatePackageDialog
    {
        public IVisualStudioService VisualStudioService { get; set; }

        public VsixCreatePackageDialog(CreatePackageService service, IDialogController dialogController, XrmRecordService xrmRecordService, IVisualStudioService visualStudioService)
            : base(service, dialogController, xrmRecordService)
        {
            VisualStudioService = visualStudioService;
        }

        protected override void LoadDialogExtention()
        {
            FileUtility.DeleteSubFolders(Request.FolderPath.FolderPath);
            FileUtility.DeleteFiles(Request.FolderPath.FolderPath);
            base.LoadDialogExtention();
        }

        protected override void ProcessCompletionExtention()
        {
            base.ProcessCompletionExtention();

            LogController.UpdateProgress(7, 8, "Moving Package Into Solution Folder");

            var folder = Request.FolderPath.FolderPath;

            //!! NOTE THE Releases Folder name is repeated in the DeployPackageCommand visibility criteria + test scripts !!
            var solutionFolder = Path.Combine(VisualStudioService.SolutionDirectory, "Releases", Request.ThisReleaseVersion);

            FileUtility.MoveWithReplace(folder, solutionFolder);

            LogController.UpdateProgress(3, 3, "Adding Package To Solution");

            VisualStudioService.AddFolder(solutionFolder);
        }
    }
}