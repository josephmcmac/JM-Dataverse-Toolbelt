using JosephM.Record.Application.Controller;
using JosephM.Record.Application.RecordEntry;
using JosephM.Record.Application.RecordEntry.Metadata;
using JosephM.Record.Service;

namespace JosephM.Record.Application.Fakes
{
    public class FakeObjectFormController : FormController
    {
        public FakeObjectFormController()
            : base(new ObjectRecordService(GetTheObject()), new ObjectFormService(GetTheObject(), GetTheService()), new FakeApplicationController())
        {
        }

        private static FakeObjectEntryClass _theObject;
        public static FakeObjectEntryClass GetTheObject()
        {
            if(_theObject == null)
                _theObject = new FakeObjectEntryClass();
            return _theObject;
        }

        private static ObjectRecordService _theService;
        public static ObjectRecordService GetTheService()
        {
            if(_theService == null)
                _theService = new ObjectRecordService(GetTheObject());
            return _theService;
        }
    }
}