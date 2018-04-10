#region

using JosephM.Application.Prism.Module.Settings;
using JosephM.Application.ViewModel.Dialog;
using JosephM.Record.Xrm.XrmRecord;

#endregion

namespace JosephM.Prism.XrmModule.XrmConnection
{
    public class XrmConnectionDialog : AppSettingsDialog<IXrmRecordConfiguration, XrmRecordConfiguration>
    {
        public XrmConnectionDialog(IDialogController dialogController)
            : base(dialogController)
        {
        }
    }
}