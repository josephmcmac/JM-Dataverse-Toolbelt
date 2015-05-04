using JosephM.Core.Attributes;
using JosephM.Core.Service;

namespace JosephM.Prism.TestModule.Prism.TestDialog
{
    [DisplayName("Test Dialog")]
    public class TestDialogRequest : ServiceRequestBase
    {
        public bool ThrowResponseErrors { get; set; }
    }
}