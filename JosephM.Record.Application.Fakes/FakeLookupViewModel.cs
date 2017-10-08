#region

using System.Threading;
using JosephM.Application.ViewModel.RecordEntry.Field;

#endregion

namespace JosephM.Application.ViewModel.Fakes
{
    /// <summary>
    ///     Not actually showing yet - was attempting to show a sample form in vs
    /// </summary>
    public class FakeLookupViewModel : LookupFieldViewModel
    {
        public FakeLookupViewModel()
            : base(
                FakeConstants.LookupField, "Fake Lookup", new FakeRecordEntryViewModel(),
                FakeConstants.RecordType, false, true)
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