using JosephM.Core.Attributes;
using JosephM.Core.FieldType;
using System.Collections.Generic;

namespace JosephM.Xrm.Autonumber
{
    [Instruction("Open the Microsoft article above for numbering options")]
    [Group(Sections.Main, Group.DisplayLayoutEnum.VerticalCentered, displayLabel: false)]
    public class AutonumberNavigator
    {
        [Group(Sections.Main)]
        [DisplayName("Select a record type to display its autonumber fields")]
        public RecordType RecordType { get; set; }

        [DoNotAllowDelete]
        [DoNotAllowGridOpen]
        [DoNotAllowAdd]
        [PropertyInContextByPropertyNotNull(nameof(RecordType))]
        public IEnumerable<AutonumberField> AutonumberFields { get; set; }

        [DoNotAllowGridEdit]
        public class AutonumberField
        {
            public string SchemaName { get; set; }
            public string Format { get; set; }
        }

        private static class Sections
        {
            public const string Main = "Select a record type to display its autonumber fields";
        }
    }
}
