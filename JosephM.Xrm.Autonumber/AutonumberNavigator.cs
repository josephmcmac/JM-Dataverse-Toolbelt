using JosephM.Core.Attributes;
using JosephM.Core.FieldType;
using System.Collections.Generic;

namespace JosephM.Xrm.Autonumber
{
    [Instruction("If your dynamics instance is version 9+ this feature allows navigating, editing & creating new autonumbers using the dynamics platforms autonumber feature\n\nNote earlier versions of dynamics do not support native autonumbering so it will not work for them\n\nFor more information and formatting options see the Microsoft article")]
    [Group(Sections.Main, true, 10)]
    public class AutonumberNavigator
    {
        [Group(Sections.Main)]
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
            public const string Main = "Select A Record Type To Display It's Autonumber Fields";
        }
    }
}
