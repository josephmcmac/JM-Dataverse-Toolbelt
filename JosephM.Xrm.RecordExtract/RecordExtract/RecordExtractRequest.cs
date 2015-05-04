using JosephM.Core.Attributes;
using JosephM.Core.FieldType;
using JosephM.Core.Service;
using JosephM.Xrm.RecordExtract.DocumentWriter;

namespace JosephM.Xrm.RecordExtract.RecordExtract
{
    [DisplayName("Extract Record")]
    public class RecordExtractRequest : ServiceRequestBase
    {
        [RequiredProperty]
        public Folder SaveToFolder { get; set; }

        [RequiredProperty]
        public DocumentType DocumentFormat { get; set; }

        [RequiredProperty]
        [RecordTypeFor("RecordLookup")]
        //Dont Change The Property Name Without Updating The Dialog LookupAllowedValues
        public RecordType RecordType { get; set; }

        [RequiredProperty]
        [PropertyInContextByPropertyNotNull("RecordType")]
        public Lookup RecordLookup { get; set; }

        [RequiredProperty]
        public DetailLevel DetailOfRelatedRecords { get; set; }
    }
}