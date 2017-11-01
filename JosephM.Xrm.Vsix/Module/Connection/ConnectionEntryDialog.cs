using JosephM.Application.ViewModel.Dialog;
using JosephM.Record.Xrm.XrmRecord;
using JosephM.Xrm.Vsix.Utilities;
using System;

namespace JosephM.XRM.VSIX.Commands.RefreshConnection
{
    public class ConnectionEntryDialog : DialogViewModel
    {
        private XrmRecordConfiguration ObjectToEnter { get; set; }
        private bool AddToSolution { get; set; }
        public IVisualStudioService VisualStudioService { get; set; }
        public Action DoPostEntry { get; private set; }

        public ConnectionEntryDialog(IDialogController dialogController, XrmRecordConfiguration objectToEnter, IVisualStudioService visualStudioService, bool addtoSolution)
            : base(dialogController)
        {
            VisualStudioService = visualStudioService;
            AddToSolution = addtoSolution;
            ObjectToEnter = objectToEnter;
            var configEntryDialog = new ObjectEntryDialog(ObjectToEnter, this, ApplicationController);
            SubDialogs = new DialogViewModel[] { configEntryDialog };
        }

        public ConnectionEntryDialog(DialogViewModel parentDialog, XrmRecordConfiguration objectToEnter, IVisualStudioService visualStudioService, bool addtoSolution, Action doPostEntry = null)
            : base(parentDialog)
        {
            VisualStudioService = visualStudioService;
            AddToSolution = addtoSolution;
            ObjectToEnter = objectToEnter;
            var configEntryDialog = new ObjectEntryDialog(ObjectToEnter, this, ApplicationController);
            SubDialogs = new DialogViewModel[] { configEntryDialog };
            DoPostEntry = doPostEntry;
        }

        protected override void LoadDialogExtention()
        {
            StartNextAction();
        }

        protected override void CompleteDialogExtention()
        {
            if(AddToSolution)
            {
                VisualStudioService.AddSolutionItem("solution.xrmconnection", ObjectToEnter);
            }
            CompletionMessage = "Connection Refreshed";
            if (DoPostEntry != null)
                DoPostEntry();
        }
    }
}