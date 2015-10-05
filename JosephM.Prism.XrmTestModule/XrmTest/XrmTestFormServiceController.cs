using JosephM.Application.Application;
using JosephM.Application.ViewModel.RecordEntry;
using JosephM.Record.Xrm.XrmRecord;

namespace JosephM.Prism.XrmTestModule.XrmTest
{
    public class XrmTestFormServiceController : FormController
    {
        public XrmTestFormServiceController(XrmRecordService recordService, XrmTestFormService formService,
            IApplicationController appplicationController)
            : base(recordService, formService, appplicationController)
        {
        }
    }
}