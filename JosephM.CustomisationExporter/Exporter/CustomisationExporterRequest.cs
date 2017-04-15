using System.Collections.Generic;
using JosephM.Application.ViewModel.SettingTypes;
using JosephM.Core.Attributes;
using JosephM.Core.FieldType;
using JosephM.Core.Service;

namespace JosephM.CustomisationExporter.Exporter
{
    [DisplayName("Export Customisations")]
    [Group(Sections.Folder, true)]
    [Group(Sections.RecordsFieldsoptions, true)]
    [Group(Sections.Relationships, true)]
    [Group(Sections.RecordTypes, true)]
    public class CustomisationExporterRequest : ServiceRequestBase
    {
        [Group(Sections.Folder)]
        [RequiredProperty]
        public Folder SaveToFolder { get; set; }

        [Group(Sections.RecordsFieldsoptions)]
        public bool Entities { get; set; }

        [Group(Sections.RecordsFieldsoptions)]
        public bool Fields { get; set; }

        [Group(Sections.RecordsFieldsoptions)]
        public bool FieldOptionSets { get; set; }

        [Group(Sections.RecordsFieldsoptions)]
        public bool SharedOptionSets { get; set; }

        [Group(Sections.Relationships)]
        public bool Relationships { get; set; }

        [Group(Sections.Relationships)]
        [PropertyInContextByPropertyValue("Relationships", true)]
        public bool DuplicateManyToManyRelationshipSides { get; set; }

        [Group(Sections.Relationships)]
        [PropertyInContextByPropertyValue("Relationships", true)]
        public bool IncludeOneToManyRelationships { get; set; }

        [RequiredProperty]
        [Group(Sections.RecordTypes)]
        public bool IncludeAllRecordTypes { get; set; }

        [RequiredProperty]
        [DisplayName("Include These Specific Record Types")]
        [PropertyInContextByPropertyValue("IncludeAllRecordTypes", false)]
        public IEnumerable<RecordTypeSetting> RecordTypes { get; set; }

        private static class Sections
        {
            public const string Folder = "Select The Folder To Save The Generated CSVs To";
            public const string RecordsFieldsoptions = "Include Records, Fields and Options";
            public const string Relationships = "Include Relationships";
            public const string RecordTypes = "Record Types";
        }
    }
}