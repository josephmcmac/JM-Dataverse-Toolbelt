#region

using JosephM.Core.Attributes;
using JosephM.Core.Constants;
using JosephM.Core.FieldType;
using JosephM.Core.Service;
using JosephM.Record.Attributes;
using JosephM.Record.Query;
using JosephM.Xrm.Schema;

#endregion

namespace JosephM.CustomisationImporter.Service
{
    [DisplayName("Import Customisations")]
    [Group(Sections.File, true)]
    [Group(Sections.Solution, true)]
    [Group(Sections.Include, true)]
    public class CustomisationImportRequest : ServiceRequestBase
    {
        [Group(Sections.File)]
        [RequiredProperty]
        [FileMask(FileMasks.ExcelFile)]
        public FileReference ExcelFile { get; set; }

        [Group(Sections.Solution)]
        [DisplayName("Add Components To Solution")]
        public bool AddToSolution { get; set; }

        [RequiredProperty]
        [Group(Sections.Solution)]
        [ReferencedType(Xrm.Schema.Entities.solution)]
        [UsePicklist]
        [LookupCondition(Xrm.Schema.Fields.solution_.ismanaged, false)]
        [LookupCondition(Xrm.Schema.Fields.solution_.isvisible, true)]
        [LookupCondition(Xrm.Schema.Fields.solution_.uniquename, ConditionType.NotEqual, "default")]
        [PropertyInContextByPropertyValue("AddToSolution", true)]
        public Lookup Solution { get; set; }

        [Group(Sections.Include)]
        public bool Entities { get; set; }
        [Group(Sections.Include)]
        public bool Fields { get; set; }
        [Group(Sections.Include)]
        public bool Relationships { get; set; }
        [Group(Sections.Include)]
        public bool FieldOptionSets { get; set; }
        [Group(Sections.Include)]
        public bool SharedOptionSets { get; set; }
        [Group(Sections.Include)]
        public bool Views { get; set; }

        private static class Sections
        {
            public const string File = "File";
            public const string Include = "Include These Items In Import";
            public const string Solution = "Solution";
        }
    }
}