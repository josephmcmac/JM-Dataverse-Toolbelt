using JosephM.Record.Application.Controller;
using JosephM.Record.Application.RecordEntry;

namespace JosephM.Record.Application.Fakes
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