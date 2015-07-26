#region

using JosephM.Record.Application.RecordEntry.Section;

#endregion

namespace JosephM.Record.Application.Fakes
{
    public class FakeSubGridViewModel : GridSectionViewModel
    {
        public FakeSubGridViewModel()
            : base(FakeFormService.FakeSubGridSection, new FakeRecordEntryViewModel())
        {
        }
    }
}