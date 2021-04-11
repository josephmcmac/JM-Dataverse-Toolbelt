using JosephM.Application.ViewModel.RecordEntry.Form;
using JosephM.Record.Extentions;
using JosephM.Record.Metadata;
using System.Collections.Generic;
using System.Linq;

namespace JosephM.Application.ViewModel.Attributes
{
    public class FieldInContextForPropertyTypes : FieldInContext
    {
        public bool IncludeAssociationReferences { get; set; }

        public FieldInContextForPropertyTypes(string recordFieldpropertyName, params RecordFieldType[] inContextTypes)
        {
            RecordFieldPropertyName = recordFieldpropertyName;
            InContextTypes = inContextTypes;
        }

        public string RecordFieldPropertyName { get; }
        public IEnumerable<RecordFieldType> InContextTypes { get; }

        public override bool IsInContext(RecordEntryViewModelBase recordEntryViewModel)
        {
            var recordFieldViewModel = recordEntryViewModel.GetRecordFieldFieldViewModel(RecordFieldPropertyName);
            var recordTypeOfField = recordEntryViewModel.FormService.GetDependantValue(RecordFieldPropertyName, recordEntryViewModel.GetRecordType(), recordEntryViewModel);
            if (recordFieldViewModel.Value != null
                && recordTypeOfField != null)
            {
                var reference = recordEntryViewModel.ParentFormReference == null
                    ? recordTypeOfField
                    : recordTypeOfField + ":" + recordEntryViewModel.ParentFormReference;
                var lookupService = recordFieldViewModel.GetRecordService().GetLookupService(RecordFieldPropertyName, recordEntryViewModel.GetRecordType(), reference, recordEntryViewModel.GetRecord());
                if(InContextTypes.Contains(lookupService.GetFieldType(recordFieldViewModel.Value.Key, recordTypeOfField)))
                {
                    return true;
                }
                if(IncludeAssociationReferences
                    && lookupService.IsManyToManyRelationship(recordTypeOfField))
                {
                    var metadata = lookupService.GetManyToManyRelationships().First(m => m.IntersectEntityName == recordTypeOfField);
                    if(metadata.Entity1IntersectAttribute == recordFieldViewModel.Value.Key
                        || metadata.Entity2IntersectAttribute == recordFieldViewModel.Value.Key)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
