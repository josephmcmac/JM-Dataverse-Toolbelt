using JosephM.Application.ViewModel.Dialog;
using JosephM.Application.ViewModel.RecordEntry;
using JosephM.Application.ViewModel.RecordEntry.Form;

namespace JosephM.Application.Prism.Module.AboutModule
{
    public class AboutDialog : DialogViewModel
    {
        public About About { get; set; }

        public AboutDialog(About about, IDialogController dialogController)
            : base(dialogController)
        {
            About = about;
        }

        protected override void CompleteDialogExtention()
        {
            
        }

        protected override void LoadDialogExtention()
        {
            var objectDisplayViewModel = new ObjectDisplayViewModel(About, FormController.CreateForObject(About, ApplicationController, null));
            Controller.LoadToUi(objectDisplayViewModel);
        }
    }
}
