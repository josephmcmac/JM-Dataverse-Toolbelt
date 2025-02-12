using JosephM.Application.Desktop.Module.ServiceRequest;
using JosephM.Application.ViewModel.Dialog;
using JosephM.Record.Xrm.XrmRecord;
using System;

namespace JosephM.XrmModule.Crud.AddRoles
{
    public class AddRolesDialog :
        ServiceRequestDialog<AddRolesService, AddRolesRequest, AddRolesResponse, AddRolesResponseItem>
    {
        public AddRolesDialog(XrmRecordService xrmRecordService, IDialogController dialogController, AddRolesRequest request, Action onClose)
            : base(new AddRolesService(xrmRecordService), dialogController, xrmRecordService, request, onClose)
        {
            
        }

        public override bool DisplayResponseDuringServiceRequestExecution => true;
    }
}