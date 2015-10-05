namespace JosephM.Application.ViewModel.RecordEntry.Form
{
    public class MaintainViewModel : OpenViewModel
    {
        public MaintainViewModel(FormController formController)
            : base(formController)
        {
            OnSave = () => RecordService.Update(GetRecord(), ChangedPersistentFields);
        }

        public override string TabLabel
        {
            get { return "Maintain"; }
        }

        public override string SaveButtonLabel
        {
            get { return "Save"; }
        }
    }
}