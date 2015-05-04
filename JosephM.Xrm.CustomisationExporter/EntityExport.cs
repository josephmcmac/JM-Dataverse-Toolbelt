using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JosephM.Core.Attributes;
using JosephM.Record.Metadata;
using JosephM.Xrm.MetadataImportExport;

namespace JosephM.Xrm.CustomisationExporter
{
    public class EntityExport
    {
        public EntityExport(string recordTypeLabel, string recordTypeSchemaName, bool isCustomEntity, string viewName, string objectTypeCode,
            string displayCollectionName, string description, bool audit, bool isActivityType, bool notes, bool activities
            , bool connections, bool mailMerge, bool queues)
        {
            RecordTypeLabel = recordTypeLabel;
            RecordTypeSchemaName = recordTypeSchemaName;
            DisplayCollectionName = displayCollectionName;
            Description = description;
            Audit = audit;
            IsActivityType = isActivityType;
            Notes = notes;
            Activities = activities;
            Connections = connections;
            MailMerge = mailMerge;
            Queues = queues;

            IsCustomEntity = isCustomEntity;
            ViewName = viewName;
            ObjectTypeCode = objectTypeCode;
        }

        [DisplayName(Headings.RecordTypes.DisplayName)]
        public string RecordTypeLabel { get; set; }
        [DisplayName(Headings.RecordTypes.SchemaName)]
        public string RecordTypeSchemaName { get; set; }
        [DisplayName(Headings.RecordTypes.DisplayCollectionName)]
        public string DisplayCollectionName { get; set; }
        [DisplayName(Headings.RecordTypes.Description)]
        public string Description { get; set; }
        [DisplayName(Headings.RecordTypes.Audit)]
        public bool Audit { get; set; }
        [DisplayName(Headings.RecordTypes.IsActivityType)]
        public bool IsActivityType { get; set; }
        [DisplayName(Headings.RecordTypes.Notes)]
        public bool Notes { get; set; }
        [DisplayName(Headings.RecordTypes.Activities)]
        public bool Activities { get; set; }
        [DisplayName(Headings.RecordTypes.Connections)]
        public bool Connections { get; set; }
        [DisplayName(Headings.RecordTypes.MailMerge)]
        public bool MailMerge { get; set; }
        [DisplayName(Headings.RecordTypes.Queues)]
        public bool Queues { get; set; }
        public bool IsCustomEntity { get; set; }
        public string ViewName { get; set; }
        public string ObjectTypeCode { get; set; }
    }
}
