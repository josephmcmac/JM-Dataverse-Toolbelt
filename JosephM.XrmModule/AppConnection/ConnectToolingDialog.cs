using JosephM.Application.ViewModel.Dialog;
using JosephM.Record.Xrm.XrmRecord;
using System;

namespace JosephM.XrmModule.AppConnection
{
    public class ConnectToolingDialog : DialogViewModel
    {
        public ConnectToolingDialog(XrmRecordService xrmRecordService, DialogViewModel parentDialog)
            : base(parentDialog)
        {
            XrmRecordService = xrmRecordService;
        }

        public XrmRecordService XrmRecordService { get; }

        protected override void CompleteDialogExtention()
        {
            var verifyConnection = XrmRecordService.VerifyConnection();
            if (!verifyConnection.IsValid)
            {
                throw new Exception(verifyConnection.GetErrorString());
            }
        }

        protected override void LoadDialogExtention()
        {
            StartNextAction();
        }
    }
}
