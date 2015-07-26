#region

using JosephM.Record.Application.RecordEntry.Form;

#endregion

namespace JosephM.Record.Application.Fakes
{
    public class FakeRecordEntryViewModel : MaintainViewModel
    {
        public FakeRecordEntryViewModel()
            : base(new FakeFormController())
        {
            var record = RecordService.GetFirst(FakeConstants.RecordType, FakeConstants.PrimaryField,
                FakeConstants.MainRecordName);
            RecordId = record.Id;
            RecordIdName = FakeConstants.Id;
            RecordType = record.Type;
            SetRecord(record);
            Reload();
        }

        public override string SaveButtonLabel
        {
            get { return "Fake"; }
        }
    }
}