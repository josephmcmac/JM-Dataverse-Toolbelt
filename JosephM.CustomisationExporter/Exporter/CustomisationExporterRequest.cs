using System.Collections.Generic;
using JosephM.Application.ViewModel.SettingTypes;
using JosephM.Core.Attributes;
using JosephM.Core.FieldType;
using JosephM.Core.Service;

namespace JosephM.CustomisationExporter.Exporter
{
    [DisplayName("Export Customisations")]
    public class CustomisationExporterRequest : ServiceRequestBase
    {
        [RequiredProperty]
        public Folder SaveToFolder { get; set; }

        public bool ExportEntities { get; set; }

        public bool ExportFields { get; set; }

        public bool ExportRelationships { get; set; }

        [PropertyInContextByPropertyValue("ExportRelationships", true)]
        public bool DuplicateManyToManyRelationshipSides { get; set; }

        [PropertyInContextByPropertyValue("ExportRelationships", true)]
        public bool IncludeOneToManyRelationships { get; set; }

        public bool ExportOptionSets { get; set; }

        [PropertyInContextByPropertyValue("ExportOptionSets", true)]
        public bool ExportSharedOptionSets { get; set; }

        [RequiredProperty]
        public bool AllRecordTypes { get; set; }

        [RequiredProperty]
        [PropertyInContextByPropertyValue("AllRecordTypes", false)]
        public IEnumerable<RecordTypeSetting> RecordTypes { get; set; }
    }
}