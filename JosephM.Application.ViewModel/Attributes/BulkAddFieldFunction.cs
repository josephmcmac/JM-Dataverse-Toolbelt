using JosephM.Application.ViewModel.RecordEntry.Form;
using JosephM.Core.Extentions;
using JosephM.Core.FieldType;
using System;
using System.Collections.Generic;
using System.Linq;

namespace JosephM.Application.ViewModel.Attributes
{
    [AttributeUsage(
        AttributeTargets.Class,
        AllowMultiple = false)]
    public class BulkAddFieldFunction : BulkAddMultiSelectFunction
    {
        public override Type TargetPropertyType
        {
            get { return typeof(RecordField); }
        }

        public override IEnumerable<PicklistOption> GetSelectionOptions(RecordEntryViewModelBase recordForm, string subGridReference)
        {
            var targetPropertyInfo = GetTargetProperty(recordForm, subGridReference);

            var gridField = GetEntryViewModel(recordForm).GetEnumerableFieldViewModel(subGridReference);

            var alreadySelected = gridField.DynamicGridViewModel != null
                ? gridField.GridRecords.Select(g => g.GetRecordFieldFieldViewModel(targetPropertyInfo.Name)?.Value.Key).ToArray()
                : gridField.Value == null ? new string[0] : gridField.Value.Cast<object>().Select(o => o.GetPropertyValue(targetPropertyInfo.Name)).Cast<RecordField>().Select(f => f.Key).ToArray();

            var recordType = recordForm.FormService.GetDependantValue(subGridReference + "." + targetPropertyInfo.Name, GetEnumeratedType(recordForm, subGridReference).AssemblyQualifiedName, recordForm);
            var lookupService = GetLookupService(recordForm, subGridReference);

            var limitFields = new string[0];
            try
            {
                limitFields = gridField
                    .GetRecordService()
                    .GetPicklistKeyValues(targetPropertyInfo.Name, gridField.RecordType, recordType, null)
                    .Select(p => p.Key)
                    .ToArray();
            }
            catch(Exception)
            {
                //oh well
            }

            //removed field searchable as inadvertently left out fields
            var fields = lookupService.GetFieldMetadata(recordType)
                .Where(f => (!alreadySelected?.Any(k => k == f.SchemaName) ?? true))
                .OrderBy(r => r.DisplayName)
                .ToArray();

            if(limitFields.Any())
            {
                fields = fields.Where(f => limitFields.Contains(f.SchemaName)).ToArray();
            }

            return fields
                .Select(f => new PicklistOption(f.SchemaName, f.DisplayName))
                .ToArray();
        }
    }
}