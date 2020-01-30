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
        [GridWidth(100)]
        [DisplayOrder(5)]
        [Group(Sections.Folder)]
        [RequiredProperty]
        public FileFormat Format { get; set; }
        [GridWidth(300)]
        [DisplayOrder(10)]
        [Group(Sections.Folder)]
        [RequiredProperty]
        public Folder SaveToFolder { get; set; }
        [GridWidth(110)]
        [DisplayOrder(100)]
        [Group(Sections.RecordsFieldsoptions)]
        public bool Entities { get; set; }
        [GridWidth(110)]
        [DisplayOrder(110)]
        [Group(Sections.RecordsFieldsoptions)]
        public bool Fields { get; set; }
        [GridWidth(110)]
        [DisplayOrder(120)]
        [Group(Sections.RecordsFieldsoptions)]
        public bool FieldOptionSets { get; set; }
        [GridWidth(110)]
        [DisplayOrder(130)]
        [Group(Sections.RecordsFieldsoptions)]
        public bool SharedOptionSets { get; set; }
        [GridWidth(110)]
        [DisplayOrder(140)]
        [Group(Sections.RecordsFieldsoptions)]
        public bool Solutions { get; set; }
        [GridWidth(110)]
        [DisplayOrder(145)]
        [Group(Sections.RecordsFieldsoptions)]
        public bool Workflows { get; set; }
        [GridWidth(110)]
        [DisplayOrder(150)]
        [Group(Sections.RecordsFieldsoptions)]
        public bool PluginAssemblies { get; set; }
        [GridWidth(110)]
        [DisplayOrder(151)]
        [Group(Sections.RecordsFieldsoptions)]
        public bool PluginTriggers { get; set; }
        [GridWidth(110)]
        [DisplayOrder(155)]
        [Group(Sections.RecordsFieldsoptions)]
        public bool SecurityRoles { get; set; }
        [GridWidth(110)]
        [DisplayOrder(160)]
        [Group(Sections.RecordsFieldsoptions)]
        public bool FieldSecurityProfiles { get; set; }
        [GridWidth(110)]
        [DisplayOrder(165)]
        [Group(Sections.RecordsFieldsoptions)]
        public bool Users { get; set; }
        [GridWidth(110)]
        [DisplayOrder(170)]
        [Group(Sections.RecordsFieldsoptions)]
        public bool Teams { get; set; }
        [GridWidth(110)]
        [DisplayOrder(175)]
        [Group(Sections.RecordsFieldsoptions)]
        public bool Reports { get; set; }
        [GridWidth(110)]
        [DisplayOrder(175)]
        [Group(Sections.RecordsFieldsoptions)]
        public bool WebResources { get; set; }
        [GridWidth(110)]
        [DisplayOrder(180)]
        [Group(Sections.RecordsFieldsoptions)]
        public bool FormsAndDashboards { get; set; }

        [GridWidth(110)]
        [DisplayOrder(200)]
        [Group(Sections.Relationships)]
        public bool Relationships { get; set; }
        [DisplayName("Duplicate N:N Relationship Sides")]
        [GridWidth(110)]
        [DisplayOrder(210)]
        [Group(Sections.Relationships)]
        [PropertyInContextByPropertyValue("Relationships", true)]
        public bool DuplicateManyToManyRelationshipSides { get; set; }
        [GridWidth(110)]
        [DisplayOrder(220)]
        [Group(Sections.Relationships)]
        [DisplayName("Include 1:N Relationships")]
        [PropertyInContextByPropertyValue("Relationships", true)]
        public bool IncludeOneToManyRelationships { get; set; }
        [GridWidth(110)]
        [DisplayOrder(300)]
        [RequiredProperty]
        [PropertyInContextForAny(nameof(Entities), nameof(Fields), nameof(FieldOptionSets), nameof(Relationships))]
        [Group(Sections.RecordTypes)]
        public bool IncludeAllRecordTypes { get; set; }

        [DisplayOrder(310)]
        [Group(Sections.RecordTypes)]
        [RequiredProperty]
        [DisplayName("Include These Types")]
        [PropertyInContextByPropertyValue(nameof(IncludeAllRecordTypes), false)]
        [PropertyInContextForAny(nameof(Entities), nameof(Fields), nameof(FieldOptionSets), nameof(Relationships))]
        public IEnumerable<RecordTypeSetting> RecordTypes { get; set; }

        public IsValidResponse Validate()
        {
            //lets just ensure at leats one valid oiton is selected
            var validProperties = new[] { nameof(Entities), nameof(Fields), nameof(FieldOptionSets), nameof(SharedOptionSets), nameof(Relationships), nameof(Solutions), nameof(Workflows), nameof(PluginAssemblies), nameof(SecurityRoles), nameof(FieldSecurityProfiles), nameof(Users), nameof(Teams), nameof(Reports), nameof(WebResources), nameof(PluginTriggers), nameof(FormsAndDashboards) };
            var isOneSelected = validProperties.Any(p => (bool)this.GetPropertyValue(p));
            var isValidResponse = new IsValidResponse();
            if(!isOneSelected)
            {
                var thisType = GetType();
                isValidResponse.AddInvalidReason($"At Least One Of {validProperties.Select(p => thisType.GetProperty(p).GetDisplayName()).JoinGrammarAnd()} Is Required To Be Selected");
            }
            return isValidResponse;
        }

        public enum FileFormat
        {
            Xlsx,
            Csv
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