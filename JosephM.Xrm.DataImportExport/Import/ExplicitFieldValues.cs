using JosephM.Core.Attributes;
using JosephM.Core.FieldType;

namespace JosephM.Xrm.DataImportExport.Import
{
    [Group(Sections.FieldUpdate, Group.DisplayLayoutEnum.VerticalCentered, 20)]
    [DoNotAllowGridEdit]
    public class ExplicitFieldValues
    {
        public override string ToString()
        {
            return
                FieldToSet == null
                ? base.ToString()
                : FieldToSet.Value + " = " + (ClearValue ? "(null)" : ValueToSet?.ToString());
        }

        [MyDescription("Field to set the explicit value in")]
        [Group(Sections.FieldUpdate)]
        [DisplayOrder(10)]
        [RequiredProperty]
        [RecordFieldFor(nameof(ValueToSet))]
        public RecordField FieldToSet { get; set; }

        [MyDescription("If set the field will be cleared rather than populated with a specific value")]
        [GridWidth(100)]
        [Group(Sections.FieldUpdate)]
        [DisplayOrder(20)]
        [RequiredProperty]
        [PropertyInContextByPropertyNotNull(nameof(FieldToSet))]
        [DisplayName("Set Value Null")]
        public bool ClearValue { get; set; }

        [MyDescription("The value to be set in the field")]
        [GridWidth(300)]
        [Group(Sections.FieldUpdate)]
        [DisplayOrder(30)]
        [RequiredProperty]
        [PropertyInContextByPropertyNotNull(nameof(FieldToSet))]
        [PropertyInContextByPropertyValue(nameof(ClearValue), false)]
        public object ValueToSet { get; set; }

        private static class Sections
        {
            public const string FieldUpdate = "Field Value To Set";
        }
    }
}
