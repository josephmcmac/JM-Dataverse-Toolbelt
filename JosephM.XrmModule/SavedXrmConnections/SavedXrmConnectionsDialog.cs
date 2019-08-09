using JosephM.Application.Application;
using JosephM.Application.Desktop.Module.Settings;
using JosephM.Application.ViewModel.Dialog;
using JosephM.Core.AppConfig;
using JosephM.Record.Xrm.XrmRecord;
using System.Linq;

namespace JosephM.XrmModule.SavedXrmConnections
{
    public class SavedXrmConnectionsDialog : AppSettingsDialog<ISavedXrmConnections, SavedXrmConnections>
    {
        public SavedXrmConnectionsDialog(IDialogController dialogController)
            : base(dialogController)
        {
        }

        protected override void CompleteDialogExtention()
        {
            base.CompleteDialogExtention();
            var connections = ApplicationController.ResolveType<ISavedXrmConnections>();
            if (connections.Connections.Any(c => c.Active))
            {
                var savedConfig = connections.Connections.First(c => c.Active);
                var recordconfig =
                    new ObjectMapping.ClassMapperFor<SavedXrmRecordConfiguration, XrmRecordConfiguration>().Map(savedConfig);
                ApplicationController.ResolveType<ISettingsManager>().SaveSettingsObject(recordconfig);
                SavedXrmConnectionsModule.RefreshXrmServices(recordconfig, ApplicationController);
            }
        }
    }
}