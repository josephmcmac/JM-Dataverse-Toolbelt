using JosephM.Application.Application;
using JosephM.Application.ViewModel.RecordEntry;
using JosephM.Application.ViewModel.RecordEntry.Metadata;
using JosephM.Record.Service;

namespace JosephM.Application.ViewModel.Fakes
{
    public class FakeObjectFormController : FormController
    {
        public FakeObjectFormController()
            : base(new ObjectRecordService(GetTheObject(), GetTheApplicationController()), new ObjectFormService(GetTheObject(), GetTheService()), GetTheApplicationController())
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
                _theService = new ObjectRecordService(GetTheObject(), GetTheApplicationController());
            return _theService;
        }

        private static IApplicationController _theApplicationController;
        public static IApplicationController GetTheApplicationController()
        {
            if (_theApplicationController == null)
                _theApplicationController = new FakeApplicationController();
            return _theApplicationController;
        }
    }
}