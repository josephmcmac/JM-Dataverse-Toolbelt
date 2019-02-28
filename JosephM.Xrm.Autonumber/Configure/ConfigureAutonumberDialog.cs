using JosephM.Application.Desktop.Module.ServiceRequest;
using JosephM.Application.ViewModel.Dialog;
using JosephM.Record.Xrm.XrmRecord;
using System;

namespace JosephM.Application.Desktop.Module.Crud.ConfigureAutonumber
{
    public class ConfigureAutonumberDialog :
        ServiceRequestDialog<ConfigureAutonumberService, ConfigureAutonumberRequest, ConfigureAutonumberResponse, ConfigureAutonumberResponseItem>
    {
        public ConfigureAutonumberDialog(XrmRecordService recordService, IDialogController dialogController, ConfigureAutonumberRequest request, Action onClose)
            : base(new ConfigureAutonumberService(recordService), dialogController, recordService, request, onClose)
        {
        }

        protected override void CompleteDialogExtention()
        {
            base.CompleteDialogExtention();
        }
    }
}