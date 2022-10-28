using JosephM.Application.Application;
using JosephM.Application.ViewModel.Dialog;
using JosephM.Core.AppConfig;
using JosephM.Core.Attributes;
using JosephM.Record.Xrm.XrmRecord;
using System;
using System.Linq;
using static JosephM.Application.Desktop.Module.AboutModule.About;

namespace JosephM.XrmModule.SavedXrmConnections
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
            var configEntryDialog = new ObjectEntryDialog(ObjectToEnter, this, ApplicationController, saveButtonLabel: "Next");
            SubDialogs = new DialogViewModel[] { configEntryDialog };
            XrmRecordService = xrmRecordService;
        }

        public AppXrmConnectionEntryDialog(IDialogController applicationController)
    :       base(applicationController)
        {
            ObjectToEnter = new SavedXrmRecordConfiguration();
            var configEntryDialog = new ObjectEntryDialog(ObjectToEnter, this, ApplicationController, saveButtonLabel: "Next");
            SubDialogs = new DialogViewModel[] { configEntryDialog };
        }

        protected override void LoadDialogExtention()
        {
            ObjectToEnter.HideActive = true;
            StartNextAction();
        }

        protected override void CompleteDialogExtention()
        {
            ObjectToEnter.HideActive = false;
            //uh huh - okay now
            ObjectToEnter.Active = true;
            //lets set the connection in the service our parent dialog is using
            if (XrmRecordService != null)
                XrmRecordService.XrmRecordConfiguration = ObjectToEnter;
            //lets also refresh it in the applications containers
            SavedXrmConnectionsModule.RefreshXrmServices(ObjectToEnter, ApplicationController, xrmRecordService: XrmRecordService);
            //lets also refresh it in the saved settings
            var appSettingsManager = ApplicationController.ResolveType(typeof(ISettingsManager)) as ISettingsManager;
            var savedConnectionsObject = ApplicationController.ResolveType<ISavedXrmConnections>();
            if(savedConnectionsObject.Connections != null)
            {
                foreach (var item in savedConnectionsObject.Connections)
                    item.Active = false;
            }
            savedConnectionsObject.Connections
                = savedConnectionsObject.Connections == null
                ? new [] { ObjectToEnter }
                : savedConnectionsObject.Connections.Union(new [] { ObjectToEnter }).ToArray();
            appSettingsManager.SaveSettingsObject(savedConnectionsObject);
            var recordconfig =
                new ObjectMapping.ClassMapperFor<SavedXrmRecordConfiguration, XrmRecordConfiguration>().Map(ObjectToEnter);
            appSettingsManager.SaveSettingsObject(recordconfig);

            if (!HasParentDialog)
                CompletionItem = new CompletedMessage();
        }

        [Group(Sections.Detail, Group.DisplayLayoutEnum.HorizontalCenteredInputOnly, 30)]
        public class CompletedMessage
        {
            [Group(Sections.Detail)]
            public string Message { get { return "New Connection Created And Activated"; } }
        }
    }
}