namespace JosephM.Application.ViewModel.Dialog
{
    public class ObjectInstanceEntryDialog
    {
        private readonly object _objectToEnter;

        public ObjectInstanceEntryDialog()
        {
        }

        public ObjectInstanceEntryDialog(object objectToEnter)
        {
            _objectToEnter = objectToEnter;
        }

        protected object ObjectToEnter
        {
            get { return _objectToEnter; }
        }
    }
}