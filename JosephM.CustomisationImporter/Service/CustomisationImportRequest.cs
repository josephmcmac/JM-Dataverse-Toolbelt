using JosephM.Core.Attributes;
using JosephM.Core.Constants;
using JosephM.Core.Extentions;
using JosephM.Core.FieldType;
using JosephM.Core.Service;
using JosephM.Record.Attributes;
using JosephM.Record.Query;
using System.Linq;

namespace JosephM.CustomisationImporter.Service
{
    [Instruction("To download the import template click the button above. Customisations in the imported excel file will be upserted into the instance and published.\n\nIf a match exists for the schema name of each customisation it will be updated, otherwise a new customisation item will be created")]
    [AllowSaveAndLoad]
    [DisplayName("Customisation Import")]
    [Group(Sections.ImportFile, Group.DisplayLayoutEnum.VerticalCentered, order: 10, displayLabel: false)]
    [Group(Sections.ItemsToInclude, Group.DisplayLayoutEnum.HorizontalLabelAbove, order: 20, selectAll: true)]
    [Group(Sections.Solution, Group.DisplayLayoutEnum.VerticalCentered, order: 30, displayLabel: false)]
    public class CustomisationImportRequest : ServiceRequestBase, IValidatableObject
    {
        [GridWidth(400)]
        [DisplayOrder(10)]
        [Group(Sections.ImportFile)]
        [RequiredProperty]
        [FileMask(FileMasks.ExcelFile)]
        [PropertyInContextByPropertyValue("HideExcelFile", false)]
        public FileReference ExcelFile { get; set; }

        [Hidden]
        public bool HideExcelFile { get; set; }

        [Hidden]
        public bool HideSolutionOptions { get; set; }
        [GridWidth(120)]
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

        [GridWidth(110)]
        [DisplayOrder(200)]
        [Group(Sections.ItemsToInclude)]
        public bool Entities { get; set; }

        [GridWidth(110)]
        [DisplayOrder(210)]
        [Group(Sections.ItemsToInclude)]
        public bool Fields { get; set; }

        [GridWidth(110)]
        [DisplayOrder(220)]
        [Group(Sections.ItemsToInclude)]
        public bool Relationships { get; set; }

        [GridWidth(110)]
        [DisplayOrder(230)]
        [Group(Sections.ItemsToInclude)]
        public bool FieldOptionSets { get; set; }

        [GridWidth(110)]
        [DisplayOrder(240)]
        [Group(Sections.ItemsToInclude)]
        public bool SharedOptionSets { get; set; }

        [GridWidth(110)]
        [DisplayOrder(250)]
        [Group(Sections.ItemsToInclude)]
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
            public const string ImportFile = "Import File";
            public const string ItemsToInclude = "Items To Include In Import";
            public const string Solution = "Solution";
        }
    }
}