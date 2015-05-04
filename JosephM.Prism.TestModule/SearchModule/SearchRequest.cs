using JosephM.Core.Attributes;
using JosephM.Core.FieldType;
using JosephM.Core.Service;

namespace JosephM.Prism.TestModule.SearchModule
{
    [DisplayName("Search")]
    public class SearchRequest : ServiceRequestBase
    {
        [RequiredProperty]
        public SearchType SearchType { get; set; }
        [RequiredProperty]
        [RecordTypeFor("FieldToSearch")]
        public RecordType RecordType { get; set; }
        [RequiredProperty]
        public RecordField FieldToSearch { get; set; }
        [RequiredProperty]
        public string SearchValue { get; set; }
    }
}