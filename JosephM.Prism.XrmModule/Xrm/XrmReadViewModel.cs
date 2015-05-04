#region

using JosephM.Record.Application.RecordEntry.Form;

#endregion

namespace JosephM.Prism.XrmModule.Xrm
{
    public class XrmReadViewModel : ReadViewModel
    {
        public XrmReadViewModel(XrmFormController formController)
            : base(formController)
        {
        }
    }
}