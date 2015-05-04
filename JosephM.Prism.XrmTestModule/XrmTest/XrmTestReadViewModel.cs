#region

using JosephM.Record.Application.RecordEntry.Form;

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