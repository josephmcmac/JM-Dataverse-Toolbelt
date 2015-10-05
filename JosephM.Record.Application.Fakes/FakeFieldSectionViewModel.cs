#region

using JosephM.Application.ViewModel.RecordEntry.Section;

#endregion

namespace JosephM.Application.ViewModel.Fakes
{
    public class FakeFieldSectionViewModel : FieldSectionViewModel
    {
        public FakeFieldSectionViewModel()
            : base(
                FakeFormService.FakeSection1,
                new FakeRecordEntryViewModel()
                )
        {
        }
    }
}