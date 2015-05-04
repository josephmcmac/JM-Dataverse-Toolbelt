#region

using JosephM.Record.Application.RecordEntry.Section;

#endregion

namespace JosephM.Record.Application.Fakes
{
    public class FakeFieldSectionViewModel : FieldSectionViewModel
    {
        public FakeFieldSectionViewModel()
            : base(
                new FakeFormController(),
                FakeFormService.FakeSection1,
                new FakeRecordEntryViewModel()
                )
        {
        }
    }
}