using JosephM.Application.ViewModel.Dialog;
using JosephM.Record.IService;

namespace JosephM.XRM.VSIX.Dialogs
{
    public abstract class VsixEntryDialog : DialogViewModel
    {
        protected object EnteredObject { get; set; }

        protected VsixEntryDialog(IDialogController dialogController, object objectToEnter, IRecordService xrmRecordService)
            : base(dialogController)
        {
            EnteredObject = objectToEnter;
            var configEntryDialog = new ObjectEntryDialog(EnteredObject, this, ApplicationController, xrmRecordService, null);
            SubDialogs = new DialogViewModel[] { configEntryDialog };
        }

        protected VsixEntryDialog(IDialogController dialogController, object objectToEnter)
            : this(dialogController, objectToEnter, null)
        {
        }

        protected override void LoadDialogExtention()
        {
            StartNextAction();
        }
    }
}