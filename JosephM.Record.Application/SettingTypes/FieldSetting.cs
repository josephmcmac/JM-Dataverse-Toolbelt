using JosephM.Application.ViewModel.Attributes;
using JosephM.Core.Attributes;
using JosephM.Core.FieldType;

namespace JosephM.Application.ViewModel.SettingTypes
{
    [DoNotAllowGridOpen]
    [BulkAddFieldFunction]
    public class FieldSetting
    {
        [DisplayOrder(20)]
        [RequiredProperty]
        public RecordField RecordField { get; set; }

        public override string ToString()
        {
            return RecordField == null ? "" : RecordField.Value;
        }
    }
}