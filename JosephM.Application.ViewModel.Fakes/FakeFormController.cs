using JosephM.Application.Application;
using JosephM.Application.ViewModel.RecordEntry;

namespace JosephM.Application.ViewModel.Fakes
{
    public class FakeFormController : FormController
    {
        public FakeFormController()
            : base(FakeRecordService.Get(), new FakeFormService(), new FakeApplicationController())
        {
        }

        public FakeFormController(IApplicationController controller)
            : base(FakeRecordService.Get(), new FakeFormService(), controller)
        {
        }
    }
}