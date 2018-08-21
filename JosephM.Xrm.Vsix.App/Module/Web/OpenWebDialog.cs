using JosephM.Application.ViewModel.Attributes;
using JosephM.Application.ViewModel.Dialog;
using JosephM.Record.Xrm.XrmRecord;
using System.Diagnostics;

namespace JosephM.Xrm.Vsix.Module.Web
{
    [RequiresConnection]
    public class OpenWebDialog : DialogViewModel
    {
        public OpenWebDialog(XrmRecordService xrmRecordService, IDialogController controller)
            : base(controller)
        {
            XrmRecordService = xrmRecordService;
        }

        public XrmRecordService XrmRecordService { get; }

        protected override void LoadDialogExtention()
        {
            StartNextAction();
        }

        protected override void CompleteDialogExtention()
        {
            string url = GetUrl();
            Process.Start(url);
            Controller.Close();
        }

        protected virtual string GetUrl()
        {
            return XrmRecordService.WebUrl;
        }
    }
}
