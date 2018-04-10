using JosephM.Application.Prism.Module.ServiceRequest;
using JosephM.Application.ViewModel.Dialog;
using JosephM.Core.Service;
using JosephM.Record.Xrm.XrmRecord;


namespace JosephM.Deployment.DeployPackage
{
    public class DeployPackageDialog :
        ServiceRequestDialog
            <DeployPackageService, DeployPackageRequest,
                ServiceResponseBase<DataImportResponseItem>, DataImportResponseItem>
    {
        public DeployPackageDialog(DeployPackageService service,
            IDialogController dialogController, XrmRecordService lookupService)
            : base(service, dialogController, lookupService)
        {
        }
    }
}