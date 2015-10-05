#region

using JosephM.Application.ViewModel.RecordEntry.Form;

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