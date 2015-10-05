#region

using JosephM.Application.ViewModel.Dialog;
using JosephM.Core.Log;
using JosephM.Prism.Infrastructure.Dialog;
using JosephM.Prism.Infrastructure.Prism;
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