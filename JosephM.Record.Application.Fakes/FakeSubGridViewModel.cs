#region

using JosephM.Application.ViewModel.RecordEntry.Section;

#endregion

namespace JosephM.Application.ViewModel.Fakes
{
    public class FakeSubGridViewModel : GridSectionViewModel
    {
        public FakeSubGridViewModel()
            : base(FakeFormService.FakeSubGridSection, new FakeRecordEntryViewModel())
        {
        }
    }
}