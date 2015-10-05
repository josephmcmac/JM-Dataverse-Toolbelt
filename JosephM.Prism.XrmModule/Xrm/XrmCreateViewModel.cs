#region

using JosephM.Application.ViewModel.RecordEntry.Form;

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