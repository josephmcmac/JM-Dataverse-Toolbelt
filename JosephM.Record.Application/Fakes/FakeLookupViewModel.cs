#region

using System.Threading;
using JosephM.Record.Application.RecordEntry.Field;

#endregion

namespace JosephM.Record.Application.Fakes
{
    /// <summary>
    ///     Not actually showing yet - was attempting to show a sample form in vs
    /// </summary>
    public class FakeLookupViewModel : LookupFieldViewModel
    {
        public FakeLookupViewModel()
            : base(
                FakeConstants.LookupField, "Fake Lookup", new FakeRecordEntryViewModel(),
                FakeConstants.RecordType, FakeRecordService.Get())
        {
            new Thread(() =>
            {
                Thread.Sleep(2000);
                EnteredText = "Main Record";
                Search();
            }).Start();
        }
    }
}