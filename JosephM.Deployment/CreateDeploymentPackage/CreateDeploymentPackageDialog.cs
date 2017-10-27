#region

using JosephM.Application.ViewModel.Dialog;
using JosephM.Core.Service;
using JosephM.Prism.Infrastructure.Dialog;
using JosephM.Record.Xrm.XrmRecord;

#endregion

namespace JosephM.Deployment.CreateDeploymentPackage
{
    public class CreateDeploymentPackageDialog :
        ServiceRequestDialog
            <CreateDeploymentPackageService, CreateDeploymentPackageRequest,
                ServiceResponseBase<DataImportResponseItem>, DataImportResponseItem>
    {
        public CreateDeploymentPackageDialog(CreateDeploymentPackageService service,
            IDialogController dialogController, XrmRecordService lookupService)
            : base(service, dialogController, lookupService)
        {
        }
    }
}