#region

using JosephM.Record.Application.RecordEntry.Form;

#endregion

namespace JosephM.Prism.XrmModule.Xrm
{
    public class XrmMaintainViewModel : MaintainViewModel
    {
        public XrmMaintainViewModel(XrmFormController formController)
            : base(formController)
        {
        }
    }
}