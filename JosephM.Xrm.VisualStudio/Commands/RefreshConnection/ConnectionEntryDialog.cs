using System.Collections.Generic;
using System.IO;
using EnvDTE;
using EnvDTE80;
using JosephM.Application.ViewModel.Dialog;
using JosephM.Core.Extentions;
using JosephM.Core.Serialisation;
using JosephM.Core.Utility;
using JosephM.Record.Xrm.XrmRecord;
using JosephM.XRM.VSIX.Dialogs;
using JosephM.XRM.VSIX.Utilities;

namespace JosephM.XRM.VSIX.Commands.RefreshConnection
{
    public class ConnectionEntryDialog : VsixEntryDialog
    {
        private bool AddToSolution { get; set; }
        public IVisualStudioService VisualStudioService { get; set; }
        public ConnectionEntryDialog(IDialogController dialogController, XrmRecordConfiguration objectToEnter, IVisualStudioService visualStudioService, bool addtoSolution)
            : base(dialogController, objectToEnter)
        {
            VisualStudioService = visualStudioService;
            AddToSolution = addtoSolution;
        }

        protected override void LoadDialogExtention()
        {
            StartNextAction();
        }

        protected override void CompleteDialogExtention()
        {
            if(AddToSolution)
            {
                VsixUtility.AddXrmConnectionToSolution(XrmRecordConfiguration, VisualStudioService);
            }
            CompletionMessage = "Connection Refreshed";
        }

        private XrmRecordConfiguration XrmRecordConfiguration
        {
            get
            {
                return EnteredObject as XrmRecordConfiguration;
            }
        }
    }
}