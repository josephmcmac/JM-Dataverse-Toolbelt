using JosephM.Application.ViewModel.Attributes;
using JosephM.Application.ViewModel.Dialog;
using JosephM.Record.Xrm.XrmRecord;

namespace JosephM.Xrm.Vsix.Module.Web
{
    [RequiresConnection]
    public class OpenSettingsDialog : OpenWebDialog
    {
        public OpenSettingsDialog(XrmRecordService xrmRecordService, IDialogController controller)
            : base(xrmRecordService, controller)
        {
        }

        protected override string GetUrl()
        {
            var url = XrmRecordService.WebUrl;
            return string.Format("{0}/main.aspx?settingsonly=true", url);
        }
    }
}
