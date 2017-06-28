using System.Collections.Generic;
using JosephM.Core.Attributes;
using JosephM.Core.FieldType;
using JosephM.Prism.XrmModule.SavedXrmConnections;
using JosephM.Record.Attributes;
using JosephM.Xrm.Schema;

namespace JosephM.Xrm.ImportExporter.Service
{
    [Group(Sections.Solution, true, 10)]
    [Group(Sections.IncludeWithExportedRecords, true, 30)]
    public class SolutionExport
    {
        [Group(Sections.Solution)]
        [DisplayName("Saved Connection For The CRM Instance")]
        [DisplayOrder(20)]
        [RequiredProperty]
        [GridWidth(400)]
        [SettingsLookup(typeof(ISavedXrmConnections), "Connections")]
        [ConnectionFor("Solution")]
        [ConnectionFor("DataToExport")]
        [ReadOnlyWhenSet]
        public SavedXrmRecordConfiguration Connection { get; set; }

        [Group(Sections.Solution)]
        [DisplayName("Solution To Export")]
        [DisplayOrder(30)]
        [RequiredProperty]
        [GridWidth(400)]
        [ReferencedType("solution")]
        [LookupCondition("ismanaged", false)]
        [LookupCondition("isvisible", true)]
        [PropertyInContextByPropertyNotNull("Connection")]
        [UsePicklist(Fields.solution_.uniquename)]
        public Lookup Solution { get; set; }

        [Group(Sections.Solution)]
        [PropertyInContextByPropertyNotNull("Solution")]
        [DisplayOrder(40)]
        public bool ExportAsManaged { get; set; }

        [Group(Sections.IncludeWithExportedRecords)]
        [DisplayOrder(1050)]
        [PropertyInContextByPropertyNotNull("Connection")]
        public bool IncludeNotes { get; set; }

        [Group(Sections.IncludeWithExportedRecords)]
        [DisplayOrder(1060)]
        [RequiredProperty]
        [PropertyInContextByPropertyNotNull("Connection")]
        public bool IncludeNNRelationshipsBetweenEntities { get; set; }

        [DisplayOrder(200)]
        [GridWidth(400)]
        [PropertyInContextByPropertyNotNull("Connection")]
        public IEnumerable<ImportExportRecordType> DataToExport { get; set; }

        public override string ToString()
        {
            return Solution != null ? Solution.Name : base.ToString();
        }

        private static class Sections
        {
            public const string Solution = "Solution";
            public const string IncludeWithExportedRecords = "Options To Include With Exported Records";
        }
    }
}
