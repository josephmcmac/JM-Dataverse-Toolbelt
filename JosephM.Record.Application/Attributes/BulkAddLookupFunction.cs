using JosephM.Application.ViewModel.Grid;
using JosephM.Application.ViewModel.RecordEntry.Form;
using JosephM.Core.FieldType;
using JosephM.Record.Extentions;
using JosephM.Record.IService;
using System;
using System.Linq;

namespace JosephM.Application.ViewModel.Attributes
{
    [AttributeUsage(
        AttributeTargets.Class,
        AllowMultiple = false)]
    public class BulkAddLookupFunction : BulkAddQueryFunction
    {
        public override Type TargetPropertyType
        {
            get { return typeof(Lookup); }
        }

        public override string GetTargetType(RecordEntryViewModelBase recordForm, string subGridReference)
        {
            return recordForm.FormService.GetLookupTargetType(subGridReference + "." + GetTargetProperty(recordForm, subGridReference).Name, GetEnumeratedType(recordForm, subGridReference).FullName, recordForm);
        }

        public override IRecordService GetQueryLookupService(RecordEntryViewModelBase recordForm, string subGridReference)
        {
            return GetLookupService(recordForm, subGridReference);
        }

        public override void AddSelectedItem(GridRowViewModel selectedRow, RecordEntryViewModelBase recordForm, string subGridReference)
        {
            var gridField = GetEntryViewModel(recordForm).GetEnumerableFieldViewModel(subGridReference);
            var targetPropertyname = GetTargetProperty(recordForm, subGridReference).Name;
            var newRecord = recordForm.RecordService.NewRecord(GetEnumeratedType(recordForm, subGridReference).AssemblyQualifiedName);
            var lookup = GetLookupService(recordForm, subGridReference).ToLookupWithAltDisplayNameName(selectedRow.GetRecord());

            newRecord.SetField(targetPropertyname, lookup, recordForm.RecordService);
            //if (gridField.GridRecords.Any(g => g.GetLookupFieldFieldViewModel(targetPropertyname).Value == lookup))
            //    return;
            //gridField.InsertRecord(newRecord, 0);
            InsertNewItem(recordForm, subGridReference, newRecord);
        }
    }
}