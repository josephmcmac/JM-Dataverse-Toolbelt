#region

using JosephM.Record.Application.Controller;
using JosephM.Record.Application.RecordEntry;
using JosephM.Record.Xrm.XrmRecord;

#endregion

namespace JosephM.Prism.XrmModule.Xrm
{
    public abstract class XrmFormController : FormController
    {
        protected XrmFormController(XrmRecordService recordService, XrmFormService formService,
            IApplicationController appplicationController)
            : base(recordService, formService, appplicationController)
        {
        }
    }
}