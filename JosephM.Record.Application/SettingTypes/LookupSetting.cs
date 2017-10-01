using JosephM.Application.ViewModel.Attributes;
using JosephM.Core.Attributes;
using JosephM.Core.FieldType;

namespace JosephM.Application.ViewModel.SettingTypes
{
    [BulkAddLookupFunction]
    public class LookupSetting
    {
        [GridWidth(400)]
        [DisplayOrder(20)]
        [RequiredProperty]
        public Lookup Record { get; set; }

        public override string ToString()
        {
            return Record == null ? "" : Record.Name;
        }
    }
}