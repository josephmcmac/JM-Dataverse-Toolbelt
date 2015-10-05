using JosephM.Core.Attributes;
using JosephM.Core.FieldType;

namespace JosephM.Application.ViewModel.SettingTypes
{
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