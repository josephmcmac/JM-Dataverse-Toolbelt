#region

using JosephM.Record.Application.RecordEntry.Form;

#endregion

namespace JosephM.Prism.XrmModule.Xrm
{
    public class XrmCreateViewModel : CreateViewModel
    {
        public XrmCreateViewModel(XrmFormController formController)
            : base(formController)
        {
        }
    }
}