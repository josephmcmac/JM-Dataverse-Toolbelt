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
    [AllowSaveAndLoad]
    [DisplayName("Import Customisations")]
    [Group(Sections.File, true, 10)]
    [Group(Sections.Solution, true, 20)]
    [Group(Sections.Include, true, order: 30, selectAll: true)]
    public class CustomisationImportRequest : ServiceRequestBase
    {
        [DisplayOrder(10)]
        [Group(Sections.File)]
        [RequiredProperty]
        [FileMask(FileMasks.ExcelFile)]
        [PropertyInContextByPropertyValue("HideExcelFile", false)]
        public FileReference ExcelFile { get; set; }

        [Hidden]
        public bool HideExcelFile { get; set; }

        [Hidden]
        public bool HideSolutionOptions { get; set; }

        [DisplayOrder(100)]
        [Group(Sections.Solution)]
        [PropertyInContextByPropertyValue("HideSolutionOptions", false)]
        [DisplayName("Add Components To Solution")]
        public bool AddToSolution { get; set; }

        [DisplayOrder(110)]
        [RequiredProperty]
        [Group(Sections.Solution)]
        [ReferencedType(Xrm.Schema.Entities.solution)]
        [UsePicklist(Xrm.Schema.Fields.solution_.uniquename)]
        [LookupCondition(Xrm.Schema.Fields.solution_.ismanaged, false)]
        [LookupCondition(Xrm.Schema.Fields.solution_.isvisible, true)]
        [LookupCondition(Xrm.Schema.Fields.solution_.uniquename, ConditionType.NotEqual, "default")]
        [PropertyInContextByPropertyValue("AddToSolution", true)]
        [PropertyInContextByPropertyValue("HideSolutionOptions", false)]
        public Lookup Solution { get; set; }

        [DisplayOrder(200)]
        [Group(Sections.Include)]
        public bool Entities { get; set; }
        [DisplayOrder(210)]
        [Group(Sections.Include)]
        public bool Fields { get; set; }
        [DisplayOrder(220)]
        [Group(Sections.Include)]
        public bool Relationships { get; set; }
        [DisplayOrder(230)]
        [Group(Sections.Include)]
        public bool FieldOptionSets { get; set; }
        [DisplayOrder(240)]
        [Group(Sections.Include)]
        public bool SharedOptionSets { get; set; }
        [DisplayOrder(250)]
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