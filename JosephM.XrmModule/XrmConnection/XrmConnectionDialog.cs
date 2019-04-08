using JosephM.Application.Desktop.Module.Settings;
using JosephM.Application.ViewModel.Dialog;
using JosephM.Record.Xrm.XrmRecord;

namespace JosephM.XrmModule.XrmConnection
{
    public class XrmConnectionDialog : AppSettingsDialog<IXrmRecordConfiguration, XrmRecordConfiguration>
    {
        public XrmConnectionDialog(IDialogController dialogController)
            : base(dialogController)
        {
        }
    }
}