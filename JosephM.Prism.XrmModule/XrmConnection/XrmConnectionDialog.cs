#region

using JosephM.Core.Log;
using JosephM.Prism.Infrastructure.Dialog;
using JosephM.Prism.Infrastructure.Prism;
using JosephM.Record.Application.Dialog;
using JosephM.Record.Xrm.XrmRecord;

#endregion

namespace JosephM.Prism.XrmModule.XrmConnection
{
    public class XrmConnectionDialog : AppSettingsDialog<IXrmRecordConfiguration, XrmRecordConfiguration>
    {
        public XrmConnectionDialog(IDialogController dialogController, PrismContainer container)
            : base(dialogController, container)
        {
        }

        private XrmRecordConfiguration XrmConfiguration
        {
            get { return SettingsObject; }
        }

        protected override bool Validate()
        {
            var isValid = new XrmRecordService(XrmConfiguration, new LogController()).VerifyConnection();
            if (isValid.IsValid)
            {
                XrmConnectionModule.RefreshXrmServices(XrmConfiguration, Container);
                return true;
            }
            else
            {
                CompletionMessage = "Error Verifying The Crm Instance\n" + string.Join("\n", isValid.InvalidReasons);
                return false;
            }
        }
    }
}