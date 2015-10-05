#region

using JosephM.Application.ViewModel.RecordEntry.Form;
using JosephM.Record.Extentions;

#endregion

namespace JosephM.Application.ViewModel.Fakes
{
    /// <summary>
    ///     Not actually showing yet - was attempting to show a sample form in vs
    /// </summary>
    public class FakeMaintainViewModel : MaintainViewModel
    {
        public FakeMaintainViewModel(FakeFormController formController)
            : base(formController)
        {
            var record = RecordService.GetFirst(FakeConstants.RecordType, FakeConstants.PrimaryField,
                FakeConstants.MainRecordName);
            RecordId = record.Id;
            RecordIdName = FakeConstants.Id;
            RecordType = record.Type;
            SetRecord(record);
        }
    }
}