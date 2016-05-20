using JosephM.Application.ViewModel.Dialog;

namespace JosephM.XRM.VSIX.Dialogs
{
    public abstract class VsixEntryDialog : DialogViewModel
    {
        protected object EnteredObject { get; set; }

        protected VsixEntryDialog(IDialogController dialogController, object objectToEnter)
            : base(dialogController)
        {
            EnteredObject = objectToEnter;
            var configEntryDialog = new ObjectEntryDialog(EnteredObject, this, ApplicationController);
            SubDialogs = new DialogViewModel[] { configEntryDialog };
        }

        protected override void LoadDialogExtention()
        {
            StartNextAction();
        }
    }
}