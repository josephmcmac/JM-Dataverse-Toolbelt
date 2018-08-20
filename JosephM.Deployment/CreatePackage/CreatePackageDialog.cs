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
        public CreatePackageDialog(CreatePackageService service, IDialogController dialogController, XrmRecordService lookupService)
            : this(service, dialogController, lookupService, false)
        {
        }

        protected CreatePackageDialog(CreatePackageService service, IDialogController dialogController, XrmRecordService lookupService, bool dontAddRedirect)
            : base(service, dialogController, lookupService)
        {
            if (!dontAddRedirect)
                this.AddRedirectToConnectionEntryWhenNotConnected(lookupService);
        }

    }
}