using System.Collections.Generic;
using JosephM.Application.ViewModel.SettingTypes;
using JosephM.Core.Attributes;
using JosephM.Core.FieldType;
using JosephM.Core.Service;

namespace JosephM.CustomisationExporter.Exporter
{
    [DisplayName("Export Customisations")]
    [Group("Include Records and Fields", true)]
    [Group("Include Relationships", true)]
    [Group("Include Option Sets", true)]
    [Group("Record Types", true)]
    public class CustomisationExporterRequest : ServiceRequestBase
    {
        [RequiredProperty]
        public Folder SaveToFolder { get; set; }

        [Group("Include Records and Fields", true)]
        public bool Entities { get; set; }

        [Group("Include Records and Fields", true)]
        public bool Fields { get; set; }

        [Group("Include Relationships", true)]
        public bool Relationships { get; set; }

        [Group("Include Relationships", true)]
        [PropertyInContextByPropertyValue("Relationships", true)]
        public bool DuplicateManyToManyRelationshipSides { get; set; }

        [Group("Include Relationships", true)]
        [PropertyInContextByPropertyValue("Relationships", true)]
        public bool IncludeOneToManyRelationships { get; set; }

        [Group("Include Option Sets", true)]
        public bool OptionSets { get; set; }

        [Group("Include Option Sets", true)]
        [PropertyInContextByPropertyValue("OptionSets", true)]
        public bool IncludeSharedOptionSets { get; set; }

        [RequiredProperty]
        [Group("Record Types")]
        public bool IncludeAllRecordTypes { get; set; }

        [RequiredProperty]
        [DisplayName("Include These Specific Record Types")]
        [PropertyInContextByPropertyValue("IncludeAllRecordTypes", false)]
        public IEnumerable<RecordTypeSetting> RecordTypes { get; set; }
    }
}