#region

using System.Linq;
using JosephM.Prism.Infrastructure.Dialog;
using JosephM.Prism.Infrastructure.Prism;
using JosephM.Prism.XrmModule.XrmConnection;
using JosephM.Record.Application.Dialog;
using JosephM.Record.Xrm.XrmRecord;

#endregion

namespace JosephM.Prism.XrmModule.SavedXrmConnections
{
    public class SavedXrmConnectionsDialog : AppSettingsDialog<ISavedXrmConnections, SavedXrmConnections>
    {
        public SavedXrmConnectionsDialog(IDialogController dialogController, PrismContainer container)
            : base(dialogController, container)
        {
        }

        protected override void CompleteDialogExtention()
        {
            base.CompleteDialogExtention();
            var connections = Container.Resolve<ISavedXrmConnections>();
            if (connections.Connections.Any(c => c.Active))
            {
                var savedConfig = connections.Connections.First(c => c.Active);
                var recordconfig =
                    new ObjectMapping.ClassMapperFor<SavedXrmRecordConfiguration, XrmRecordConfiguration>().Map(savedConfig);
                Container.Resolve<PrismSettingsManager>().SaveSettingsObject(recordconfig);
                XrmConnectionModule.RefreshXrmServices(recordconfig, Container);
            }
        }
    }
}