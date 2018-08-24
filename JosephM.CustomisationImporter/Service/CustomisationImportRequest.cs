#region

using JosephM.Core.Attributes;
using JosephM.Core.Constants;
using JosephM.Core.Extentions;
using JosephM.Core.FieldType;
using JosephM.Core.Service;
using JosephM.Record.Attributes;
using JosephM.Record.Query;
using System.Linq;

#endregion

namespace JosephM.CustomisationImporter.Service
{
    [Instruction("Customisations Defined In The Excel File Will Be Imported Into The Dynamics Instance And Published. For Each Item If A Match Exists For The Schema Name It Will Be Updated, Otherwise A New Customisation Item Will Be Created")]
    [AllowSaveAndLoad]
    [DisplayName("Import Customisations")]
    [Group(Sections.File, true, 10)]
    [Group(Sections.Solution, true, 20)]
    [Group(Sections.Include, true, order: 30, selectAll: true)]
    public class CustomisationImportRequest : ServiceRequestBase, IValidatableObject
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

        public IsValidResponse Validate()
        {
            //lets just ensure at leats one valid oiton is selected
            var validProperties = new[] { nameof(Entities), nameof(Fields), nameof(Relationships), nameof(FieldOptionSets), nameof(SharedOptionSets), nameof(Views) };
            var isOneSelected = validProperties.Any(p => (bool)this.GetPropertyValue(p));
            var isValidResponse = new IsValidResponse();
            if (!isOneSelected)
            {
                var thisType = GetType();
                isValidResponse.AddInvalidReason($"At Least One Of {validProperties.Select(p => thisType.GetProperty(p).GetDisplayName()).JoinGrammarAnd()} Is Required To Be Selected");
            }
            return isValidResponse;
        }

        private static class Sections
        {
            public const string File = "File";
            public const string Include = "Include These Items In Import";
            public const string Solution = "Solution";
        }
    }
}