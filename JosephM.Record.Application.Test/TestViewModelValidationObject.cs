using System.Collections.Generic;
using JosephM.Core.Attributes;

namespace JosephM.Application.ViewModel.Test
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

            public bool RequireRecordsInTheGrid { get; set; }

            [PropertyInContextByPropertyValue(nameof(RequireRecordsInTheGrid), true)]
            [RequiredProperty]
            public IEnumerable<TestEnumerablePropertyObject> RequiredEnumerableInTheGrid { get; set; }
        }
    }


}
