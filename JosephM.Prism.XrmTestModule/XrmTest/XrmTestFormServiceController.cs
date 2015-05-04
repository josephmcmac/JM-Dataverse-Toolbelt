using JosephM.Record.Application.Controller;
using JosephM.Record.Application.RecordEntry;
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