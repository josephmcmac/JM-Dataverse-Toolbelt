using JosephM.Application.ViewModel.Dialog;
using JosephM.Core.Service;
using JosephM.Core.Utility;
using JosephM.Deployment;
using JosephM.Deployment.CreateDeploymentPackage;
using JosephM.Record.Xrm.XrmRecord;
using JosephM.XRM.VSIX.Dialogs;
using JosephM.XRM.VSIX.Utilities;
using System.IO;

namespace JosephM.XRM.VSIX.Commands.CreateDeploymentPackage
{
    public class CreateDeploymentPackageDialog : VsixServiceDialog<CreateDeploymentPackageService, CreateDeploymentPackageRequest, ServiceResponseBase<DataImportResponseItem>, DataImportResponseItem>
    {
        public XrmRecordService XrmRecordService { get { return Service.XrmRecordService; } }
        public XrmPackageSettings PackageSettings { get; set; }
        public IVisualStudioService VisualStudioService { get; set; }

        public CreateDeploymentPackageDialog(CreateDeploymentPackageService service, CreateDeploymentPackageRequest request, IDialogController dialogController, XrmPackageSettings packageSettings, IVisualStudioService visualStudioService)
            : base(service, request, dialogController, lookupService: service.XrmRecordService, showRequestEntryForm: true)
        {
            PackageSettings = packageSettings;
            VisualStudioService = visualStudioService;
        }

        protected override void LoadDialogExtention()
        {
            FileUtility.DeleteSubFolders(Request.FolderPath.FolderPath);
            FileUtility.DeleteFiles(Request.FolderPath.FolderPath);
            StartNextAction();
        }

        public override void PostExecute()
        {
            var folder = Request.FolderPath.FolderPath;

            //!! NOTE THE Releases Folder name is repeated in the DeployPackageCommand visibility criteria + test scripts !!
            var solutionFolder = Path.Combine(VisualStudioService.SolutionDirectory,"Releases", Request.ThisReleaseVersion);

            FileUtility.MoveWithReplace(folder, solutionFolder);

            VisualStudioService.AddFolder(solutionFolder);
        }
    }
}