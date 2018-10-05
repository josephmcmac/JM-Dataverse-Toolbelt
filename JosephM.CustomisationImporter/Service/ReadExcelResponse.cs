using JosephM.Core.Attributes;
using JosephM.CustomisationImporter.ImportMetadata;
using JosephM.Record.Metadata;
using System.Collections.Generic;

namespace JosephM.CustomisationImporter.Service
{
    [Instruction("The Errors Below Were Encountered Loading The Spreadsheet. These Errors Will Need To Be Fixed Before Importing The Customisations")]
    public class ReadExcelResponse : CustomisationImportResponse
    {
        private IEnumerable<PicklistOptionSet> optionSets;

        public IEnumerable<PicklistOptionSet> GetOptionSets()
        {
            return optionSets;
        }

        internal void SetOptionSets(IEnumerable<PicklistOptionSet> value)
        {
            optionSets = value;
        }

        private IDictionary<int, FieldMetadata> fieldMetadataToImport;

        public IDictionary<int, FieldMetadata> GetFieldMetadataToImport()
        {
            return fieldMetadataToImport;
        }

        internal void SetFieldMetadataToImport(IDictionary<int, FieldMetadata> value)
        {
            fieldMetadataToImport = value;
        }

        private IDictionary<int, Many2ManyRelationshipMetadata> relationshipMetadataToImport;

        public IDictionary<int, Many2ManyRelationshipMetadata> GetRelationshipMetadataToImport()
        {
            return relationshipMetadataToImport;
        }

        internal void SetRelationshipMetadataToImport(IDictionary<int, Many2ManyRelationshipMetadata> value)
        {
            relationshipMetadataToImport = value;
        }

        private IDictionary<int, ImportRecordMetadata> recordMetadataToImport;

        public IDictionary<int, ImportRecordMetadata> GetRecordMetadataToImport()
        {
            return recordMetadataToImport;
        }

        internal void SetRecordMetadataToImport(IDictionary<int, ImportRecordMetadata> value)
        {
            recordMetadataToImport = value;
        }
    }
}