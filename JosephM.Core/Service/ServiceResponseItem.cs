using JosephM.Core.Attributes;
using JosephM.Core.Extentions;
using System;

namespace JosephM.Core.Service
{
    public class ServiceResponseItem
    {
        [Hidden]
        [DisplayName("Is Error")]
        [GridWidth(75)]
        [DisplayOrder(999999)]
        [PropertyInContextByPropertyValue(nameof(HasError), true)]
        public bool HasError
        {
            get { return Exception != null; }
        }

        [Hidden]
        public Exception Exception { get; set; }

        [PropertyInContextByPropertyValue(nameof(HasError), true)]
        [DisplayOrder(1000000)]
        [GridWidth(400)]
        [Multiline]
        public string ErrorDetails
        {
            get { return Exception == null ? null : Exception.DisplayString(); }
        }
    }
}