using System.Collections;
using System.Collections.Generic;
using JosephM.Core.Attributes;
using JosephM.Core.Service;

namespace JosephM.Prism.TestModule.Prism.TestDialog
{
    [DisplayName("Test Dialog")]
    public class TestDialogRequest : ServiceRequestBase
    {
        public TestDialogRequest()
        {
            Items = new[] {new TestDialogRequestItem()};
        }
        public bool ThrowResponseErrors { get; set; }

        [DoNotAllowAdd]
        public IEnumerable<TestDialogRequestItem> Items { get; set; }

        public class TestDialogRequestItem
        {
            public TestDialogRequestItem()
            {
                ReadOnlyProp = "I Read Only";
            }

            [ReadOnlyWhenSet]
            public string ReadOnlyProp { get; set; }
        }
    }
} ;