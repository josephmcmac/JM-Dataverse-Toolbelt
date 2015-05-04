using System.Collections.Generic;
using JosephM.Core.Attributes;

namespace JosephM.Record.Application.Test
{
    public class TestViewModelValidationObject
    {
        [RequiredProperty]
        public string RequiredString { get; set; }

        public string NotRequiredString { get; set; }

        [RequiredProperty]
        public IEnumerable<TestEnumerablePropertyObject> RequiredIEnumerableProperty{ get; set; }

        public IEnumerable<TestEnumerablePropertyObject> NotRequiredIEnumerableProperty { get; set; }

        public class TestEnumerablePropertyObject
        {
            [RequiredProperty]
            public string RequiredString { get; set; }

            public string NotRequiredString { get; set; }
        }
    }


}
