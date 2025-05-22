using JosephM.Application.Desktop.Module.ServiceRequest;
using JosephM.Application.ViewModel.Dialog;
using JosephM.Record.Xrm.XrmRecord;
using System;

namespace JosephM.XrmModule.Crud.RemoveRoles
{
    public class RemoveRolesDialog :
        ServiceRequestDialog<RemoveRolesService, RemoveRolesRequest, RemoveRolesResponse, RemoveRolesResponseItem>
    {
        public RemoveRolesDialog(XrmRecordService xrmRecordService, IDialogController dialogController, RemoveRolesRequest request, Action onClose)
            : base(new RemoveRolesService(xrmRecordService), dialogController, xrmRecordService, request, onClose)
        {
            
        }

        public override bool DisplayResponseDuringServiceRequestExecution => true;
    }
}