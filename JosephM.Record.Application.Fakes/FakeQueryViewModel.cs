#region

using JosephM.Application.ViewModel.Grid;
using JosephM.Application.ViewModel.RecordEntry.Form;
using JosephM.Record.Extentions;

#endregion

namespace JosephM.Application.ViewModel.Fakes
{
    public class FakeQueryViewModel : QueryViewModel
    {
        public FakeQueryViewModel()
            : base(new[] { FakeConstants.RecordType }, FakeRecordService.Get(), new FakeApplicationController(), allowQuery: true, loadInitially: true)
        {

        }
    }
}