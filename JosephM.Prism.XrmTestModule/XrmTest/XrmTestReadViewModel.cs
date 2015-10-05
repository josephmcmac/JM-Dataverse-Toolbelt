#region

using JosephM.Application.ViewModel.RecordEntry.Form;

#endregion

namespace JosephM.Prism.XrmTestModule.XrmTest
{
    public class XrmTestReadViewModel : ReadViewModel
    {
        public XrmTestReadViewModel(XrmTestFormServiceController formController)
            : base(formController)
        {
        }
    }
}