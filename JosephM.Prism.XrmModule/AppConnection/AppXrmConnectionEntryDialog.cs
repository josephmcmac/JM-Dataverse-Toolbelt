using JosephM.Application.Application;
using JosephM.Application.ViewModel.Dialog;
using JosephM.Record.Xrm.XrmRecord;
using JosephM.XrmModule.XrmConnection;
using System;
using JosephM.Core.AppConfig;
using System.Linq;
using JosephM.XrmModule.SavedXrmConnections;

namespace JosephM.XrmModule.AppConnection
{
    /// <summary>
    /// This dialog is for entering a connection to dynamics when the app is not yet connected
    /// </summary>
    public class AppXrmConnectionEntryDialog : DialogViewModel
    {
        private SavedXrmRecordConfiguration ObjectToEnter { get; set; }
        public Action DoPostEntry { get; private set; }
        public XrmRecordService XrmRecordService { get; }

        public AppXrmConnectionEntryDialog(DialogViewModel parentDialog, XrmRecordService xrmRecordService)
            : base(parentDialog)
        {
            ObjectToEnter = new SavedXrmRecordConfiguration();
            var configEntryDialog = new ObjectEntryDialog(ObjectToEnter, this, ApplicationController);
            SubDialogs = new DialogViewModel[] { configEntryDialog };
            XrmRecordService = xrmRecordService;
        }

        protected override void LoadDialogExtention()
        {
            StartNextAction();
        }

        protected override void CompleteDialogExtention()
        {
            //uh huh - okay now
            ObjectToEnter.Active = true;
            //lets set the connection in the service our parent dialog is using
            XrmRecordService.XrmRecordConfiguration = ObjectToEnter;
            //lets also refresh it in the applications containers
            XrmConnectionModule.RefreshXrmServices(ObjectToEnter, ApplicationController, xrmRecordService: XrmRecordService);
            //lets also refresh it in the saved settings
            var appSettingsManager = ApplicationController.ResolveType(typeof(ISettingsManager)) as ISettingsManager;
            var savedConnectionsObject = ApplicationController.ResolveType<ISavedXrmConnections>();
            savedConnectionsObject.Connections = savedConnectionsObject.Connections == null ? new [] { ObjectToEnter } : savedConnectionsObject.Connections.Union(new [] { ObjectToEnter }).ToArray();
            appSettingsManager.SaveSettingsObject(savedConnectionsObject);
            var recordconfig =
                new ObjectMapping.ClassMapperFor<SavedXrmRecordConfiguration, XrmRecordConfiguration>().Map(ObjectToEnter);
            appSettingsManager.SaveSettingsObject(recordconfig);
        }
    }
}