using System.Collections.Generic;
using System.Linq;
using JosephM.Application.ViewModel.SettingTypes;
using JosephM.Core.Attributes;
using JosephM.Core.Extentions;
using JosephM.Core.FieldType;
using JosephM.Core.Service;

namespace JosephM.CustomisationExporter.Exporter
{
    [Instruction("CSV Files Will Be Output Into The Folder Listing All The Components Of Each Type Along With Their Details")]
    [AllowSaveAndLoad]
    [DisplayName("Export Customisations")]
    [Group(Sections.Folder, true, 10)]
    [Group(Sections.RecordsFieldsoptions, true, 20, selectAll: true)]
    [Group(Sections.Relationships, true, 30)]
    [Group(Sections.RecordTypes, true, 40)]
    public class CustomisationExporterRequest : ServiceRequestBase, IValidatableObject
    {
        [DisplayOrder(10)]
        [Group(Sections.Folder)]
        [RequiredProperty]
        public Folder SaveToFolder { get; set; }

        [DisplayOrder(100)]
        [Group(Sections.RecordsFieldsoptions)]
        public bool Entities { get; set; }

        [DisplayOrder(110)]
        [Group(Sections.RecordsFieldsoptions)]
        public bool Fields { get; set; }

        [DisplayOrder(120)]
        [Group(Sections.RecordsFieldsoptions)]
        public bool FieldOptionSets { get; set; }

        [DisplayOrder(130)]
        [Group(Sections.RecordsFieldsoptions)]
        public bool SharedOptionSets { get; set; }

        [DisplayOrder(200)]
        [Group(Sections.Relationships)]
        public bool Relationships { get; set; }

        [DisplayOrder(210)]
        [Group(Sections.Relationships)]
        [PropertyInContextByPropertyValue("Relationships", true)]
        public bool DuplicateManyToManyRelationshipSides { get; set; }

        [DisplayOrder(220)]
        [Group(Sections.Relationships)]
        [PropertyInContextByPropertyValue("Relationships", true)]
        public bool IncludeOneToManyRelationships { get; set; }

        [DisplayOrder(300)]
        [RequiredProperty]
        [PropertyInContextForAny(nameof(Entities), nameof(Fields), nameof(FieldOptionSets), nameof(Relationships))]
        [Group(Sections.RecordTypes)]
        public bool IncludeAllRecordTypes { get; set; }

        [DisplayOrder(310)]
        [Group(Sections.RecordTypes)]
        [RequiredProperty]
        [DisplayName("Include These Specific Record Types")]
        [PropertyInContextByPropertyValue(nameof(IncludeAllRecordTypes), false)]
        [PropertyInContextForAny(nameof(Entities), nameof(Fields), nameof(FieldOptionSets), nameof(Relationships))]
        public IEnumerable<RecordTypeSetting> RecordTypes { get; set; }

        public IsValidResponse Validate()
        {
            //lets just ensure at leats one valid oiton is selected
            var validProperties = new[] { nameof(Entities), nameof(Fields), nameof(FieldOptionSets), nameof(SharedOptionSets), nameof(Relationships) };
            var isOneSelected = validProperties.Any(p => (bool)this.GetPropertyValue(p));
            var isValidResponse = new IsValidResponse();
            if(!isOneSelected)
            {
                var thisType = GetType();
                isValidResponse.AddInvalidReason($"At Least One Of {validProperties.Select(p => thisType.GetProperty(p).GetDisplayName()).JoinGrammarAnd()} Is Required To Be Selected");
            }
            return isValidResponse;
        }

        private static class Sections
        {
            public const string Folder = "Select The Folder To Save The Generated CSVs To";
            public const string RecordsFieldsoptions = "Include Records, Fields and Options";
            public const string Relationships = "Include Relationships";
            public const string RecordTypes = "Record Types";
        }
    }
}