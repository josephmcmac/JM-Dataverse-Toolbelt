using JosephM.Application.Desktop.Module.ServiceRequest;
using JosephM.Application.ViewModel.Attributes;
using JosephM.Application.ViewModel.Dialog;
using JosephM.Core.Service;
using JosephM.Record.Xrm.XrmRecord;

namespace JosephM.Deployment.CreatePackage
{
    [RequiresConnection]
    public class CreatePackageDialog :
        ServiceRequestDialog
            <CreatePackageService, CreatePackageRequest,
                ServiceResponseBase<DataImportResponseItem>, DataImportResponseItem>
    {
        public CreatePackageDialog(CreatePackageService service, IDialogController dialogController, XrmRecordService lookupService)
            : base(service, dialogController, lookupService)
        {
        }
    }
}