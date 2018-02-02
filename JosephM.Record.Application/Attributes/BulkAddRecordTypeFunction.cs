using JosephM.Application.ViewModel.Grid;
using JosephM.Application.ViewModel.RecordEntry.Form;
using JosephM.Core.Extentions;
using JosephM.Core.FieldType;
using JosephM.Record.IService;
using JosephM.Record.Service;
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
        public override Type TargetPropertyType
        {
            get { return typeof(RecordType); }
        }

        public override IEnumerable<PicklistOption> GetSelectionOptions(RecordEntryViewModelBase recordForm, string subGridReference)
        {
            var gridField = GetEntryViewModel(recordForm).GetEnumerableFieldViewModel(subGridReference);
            var targetPropertyname = GetTargetProperty(recordForm, subGridReference).Name;
            var gridRecords = gridField.GridRecords;

            var lookupService = GetLookupService(recordForm, subGridReference);
            var includeExplicit = new[] { "subject" };
            var types = lookupService
                .GetAllRecordTypes()
                .Where(r => !gridRecords?.Any(g => g.GetRecordTypeFieldViewModel(targetPropertyname).Value?.Key == r) ?? true)
                .Select(r => lookupService.GetRecordTypeMetadata(r))
                .Where(r => r.Searchable || includeExplicit.Contains(r.SchemaName))
                .OrderBy(r => r.DisplayName)
                .ToArray();

            return types
                .Select(f => new PicklistOption(f.SchemaName, f.DisplayName))
                .ToArray();
        }
    }
}