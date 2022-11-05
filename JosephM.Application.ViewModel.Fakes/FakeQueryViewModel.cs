using JosephM.Application.ViewModel.Query;

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