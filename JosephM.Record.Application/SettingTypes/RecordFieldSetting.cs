using JosephM.Core.Attributes;
using JosephM.Core.FieldType;

namespace JosephM.Application.ViewModel.SettingTypes
{
    [DoNotAllowGridOpen]
    public class RecordFieldSetting
    {
        [DisplayOrder(10)]
        [RequiredProperty]
        [RecordTypeFor(nameof(RecordField))]
        public RecordType RecordType { get; set; }

        [DisplayOrder(20)]
        [RequiredProperty]
        public RecordField RecordField { get; set; }

        public override string ToString()
        {
            return string.Format("{0}.{1}", RecordType, RecordField);
        }
    }
}