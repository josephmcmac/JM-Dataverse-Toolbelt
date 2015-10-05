#region

using JosephM.Application.ViewModel.RecordEntry.Form;

#endregion

namespace JosephM.Application.ViewModel.Fakes
{
    public class FakeObjectEntryViewModel : ObjectEntryViewModel
    {
        public FakeObjectEntryViewModel()
            : base(() => { }, () => { }, FakeObjectFormController.GetTheObject(), new FakeObjectFormController())
        {
        }
    }
}