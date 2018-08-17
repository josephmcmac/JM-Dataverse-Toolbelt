using JosephM.Application.Desktop.Module.ServiceRequest;
using JosephM.Application.ViewModel.Dialog;
using JosephM.Core.Service;
using JosephM.Record.Xrm.XrmRecord;
using JosephM.XrmModule.Extentions;

namespace JosephM.Deployment.CreatePackage
{
    public class CreatePackageDialog :
        ServiceRequestDialog
            <CreatePackageService, CreatePackageRequest,
                ServiceResponseBase<DataImportResponseItem>, DataImportResponseItem>
    {
        public CreatePackageDialog(CreatePackageService service,
            IDialogController dialogController, XrmRecordService lookupService)
            : base(service, dialogController, lookupService)
        {
            this.AddRedirectToConnectionEntryWhenNotConnected(lookupService);
        }
    }
}