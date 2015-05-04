namespace JosephM.Record.Application.RecordEntry.Form
{
    public abstract class MaintainViewModel : OpenViewModel
    {
        protected MaintainViewModel(FormController formController)
            : base(formController)
        {
        }

        public override string TabLabel
        {
            get { return "Maintain"; }
        }

        public override string SaveButtonLabel
        {
            get { return "Save"; }
        }

        public override void OnSaveExtention()
        {
            RecordService.Update(GetRecord(), ChangedPersistentFields);
        }
    }
}