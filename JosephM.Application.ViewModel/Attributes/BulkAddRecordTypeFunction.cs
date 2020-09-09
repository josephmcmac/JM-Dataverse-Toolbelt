using JosephM.Application.ViewModel.RecordEntry.Form;
using JosephM.Core.FieldType;
using System;
using System.Collections.Generic;
using System.Linq;

namespace JosephM.Application.ViewModel.Attributes
{
    [AttributeUsage(
        AttributeTargets.Class,
        AllowMultiple = false)]
    public class BulkAddRecordTypeFunction : BulkAddMultiSelectFunction
    {
        public BulkAddRecordTypeFunction(bool allowTypeMultipleTimes = false)
        {
            AllowTypeMultipleTimes = allowTypeMultipleTimes;
        }

        public override Type TargetPropertyType
        {
            get { return typeof(RecordType); }
        }

        public bool AllowTypeMultipleTimes { get; }

        public override IEnumerable<PicklistOption> GetSelectionOptions(RecordEntryViewModelBase recordForm, string subGridReference)
        {
            var gridField = GetEntryViewModel(recordForm).GetEnumerableFieldViewModel(subGridReference);
            var targetPropertyname = GetTargetProperty(recordForm, subGridReference).Name;
            var gridRecords = gridField.GridRecords;

            var picklistOptions = recordForm.RecordService.GetPicklistKeyValues(targetPropertyname, gridField.RecordType, subGridReference, null);
            return picklistOptions
                .Where(r => AllowTypeMultipleTimes || (!gridRecords?.Any(g => g.GetRecordTypeFieldViewModel(targetPropertyname).Value?.Key == r.Key) ?? true))
                .OrderBy(r => r.Value)
                .ToArray();
        }
    }
}