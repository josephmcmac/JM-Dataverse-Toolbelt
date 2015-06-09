#region

using JosephM.Record.Application.RecordEntry.Form;

#endregion

namespace JosephM.Record.Application.Fakes
{
    public class FakeObjectEntryViewModel : ObjectEntryViewModel
    {
        public FakeObjectEntryViewModel()
            : base(() => { }, () => { }, FakeObjectFormController.GetTheObject(), new FakeObjectFormController())
        {
        }
    }
}