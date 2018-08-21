using JosephM.Application.ViewModel.Attributes;
using JosephM.Application.ViewModel.Dialog;
using JosephM.Core.FieldType;
using JosephM.Core.Utility;
using JosephM.Deployment.CreatePackage;
using JosephM.Record.Xrm.XrmRecord;
using JosephM.Xrm.Vsix.Application;
using JosephM.Xrm.Vsix.Module.PackageSettings;
using System.IO;

namespace JosephM.Xrm.Vsix.Module.CreatePackage
{
    [RequiresConnection(nameof(ProcessEnteredSettings))]
    public class VsixCreatePackageDialog : CreatePackageDialog
    {
        public IVisualStudioService VisualStudioService { get; set; }
        public XrmPackageSettings XrmPackageSettings { get; }

        public VsixCreatePackageDialog(CreatePackageService service, IDialogController dialogController, XrmRecordService xrmRecordService, IVisualStudioService visualStudioService, XrmPackageSettings xrmPackageSettings)
            : base(service, dialogController, xrmRecordService)
        {
            VisualStudioService = visualStudioService;
            XrmPackageSettings = xrmPackageSettings;
        }

        public void ProcessEnteredSettings(XrmPackageSettings packageSettings)
        {
            if (packageSettings != null)
            {
                Request.Solution = packageSettings.Solution;
            }
        }

        protected override void LoadDialogExtention()
        {
            Request.HideTypeAndFolder = true;
            Request.FolderPath = new Folder(VisualStudioService.SolutionDirectory + "/TempSolutionFolder");
            Request.Solution = XrmPackageSettings.Solution;
            //WARNING THIS FOLDER IS CLEARED BEFORE PROCESSING SO CAREFUL IF CHANGE DIRECTORY
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