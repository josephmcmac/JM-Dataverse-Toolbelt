#region

using JosephM.Application.ViewModel.RecordEntry.Form;

#endregion

namespace JosephM.Prism.XrmTestModule.XrmTest
{
    public class XrmTestCreateViewModel : CreateViewModel
    {
        public XrmTestCreateViewModel(XrmTestFormServiceController formController)
            : base(formController)
        {
        }
    }
}