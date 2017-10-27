using JosephM.Application.ViewModel.Dialog;
using JosephM.Core.Service;
using JosephM.Deployment;
using JosephM.Deployment.DeployPackage;
using JosephM.Record.Xrm.XrmRecord;
using JosephM.XRM.VSIX.Dialogs;
using JosephM.XRM.VSIX.Utilities;

namespace JosephM.XRM.VSIX.Commands.DeployPackage
{
    public class DeployPackageDialog : VsixServiceDialog<DeployPackageService, DeployPackageRequest, ServiceResponseBase<DataImportResponseItem>, DataImportResponseItem>
    {
        public XrmRecordService XrmRecordService { get { return Service.XrmRecordService; } }
        public XrmPackageSettings PackageSettings { get; set; }
        public IVisualStudioService VisualStudioService { get; set; }

        public DeployPackageDialog(DeployPackageService service, DeployPackageRequest request, IDialogController dialogController, XrmPackageSettings packageSettings, IVisualStudioService visualStudioService)
            : base(service, request, dialogController, lookupService: service.XrmRecordService, showRequestEntryForm: true)
        {
            PackageSettings = packageSettings;
            VisualStudioService = visualStudioService;
        }

        protected override void LoadDialogExtention()
        {
            StartNextAction();
        }
    }
}