namespace JosephM.Application.ViewModel.RecordEntry.Form
{
    public abstract class ReadViewModel : OpenViewModel
    {
        protected ReadViewModel(FormController formController)
            : base(formController)
        {
        }

        public override string TabLabel
        {
            get { return "Read"; }
        }
    }
}