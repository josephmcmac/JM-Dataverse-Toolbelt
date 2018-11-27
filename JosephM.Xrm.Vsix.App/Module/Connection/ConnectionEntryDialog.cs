using JosephM.Application.Application;
using JosephM.Application.ViewModel.Dialog;
using JosephM.Record.Xrm.XrmRecord;
using JosephM.Xrm.Vsix.Application;
using JosephM.XrmModule.SavedXrmConnections;
using System;

namespace JosephM.Xrm.Vsix.Module.Connection
{
    public class ConnectionEntryDialog : DialogViewModel
    {
        private SavedXrmRecordConfiguration ObjectToEnter { get; set; }
        public bool AddToSolution { get; set; }
        public IVisualStudioService VisualStudioService { get; set; }
        public Action DoPostEntry { get; private set; }

        public ConnectionEntryDialog(IDialogController dialogController, SavedXrmRecordConfiguration objectToEnter, IVisualStudioService visualStudioService, bool addtoSolution)
            : base(dialogController)
        {
            VisualStudioService = visualStudioService;
            AddToSolution = addtoSolution;
            ObjectToEnter = objectToEnter;
            var configEntryDialog = new ObjectEntryDialog(ObjectToEnter, this, ApplicationController);
            SubDialogs = new DialogViewModel[] { configEntryDialog };
        }

        public ConnectionEntryDialog(DialogViewModel parentDialog, SavedXrmRecordConfiguration objectToEnter, IVisualStudioService visualStudioService, bool addtoSolution, Action doPostEntry = null)
            : base(parentDialog)
        {
            VisualStudioService = visualStudioService;
            AddToSolution = addtoSolution;
            ObjectToEnter = objectToEnter;
            var configEntryDialog = new ObjectEntryDialog(ObjectToEnter, this, ApplicationController, saveButtonLabel: "Next");
            SubDialogs = new DialogViewModel[] { configEntryDialog };
            DoPostEntry = doPostEntry;
        }

        protected override void LoadDialogExtention()
        {
            ObjectToEnter.HideActive = true;
            StartNextAction();
        }

        protected override void CompleteDialogExtention()
        {
            ObjectToEnter.HideActive = false;
            if (AddToSolution)
            {
                var settingsManager = ApplicationController.ResolveType(typeof(ISettingsManager)) as ISettingsManager;
                if (settingsManager == null)
                    throw new NullReferenceException("settingsManager");
                settingsManager.SaveSettingsObject(ObjectToEnter);
            }
            CompletionMessage = "Connection Refreshed";
            if (DoPostEntry != null)
                DoPostEntry();
        }
    }
}