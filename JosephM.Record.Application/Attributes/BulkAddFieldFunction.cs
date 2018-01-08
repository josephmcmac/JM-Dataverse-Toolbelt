using JosephM.Application.ViewModel.Grid;
using JosephM.Application.ViewModel.RecordEntry.Form;
using JosephM.Core.Extentions;
using JosephM.Core.FieldType;
using JosephM.Record.IService;
using System;
using System.Collections.Generic;
using System.Linq;
using JosephM.Record.Extentions;
using JosephM.Application.ViewModel.SettingTypes;
using JosephM.Record.Service;
using System.Threading;

namespace JosephM.Application.ViewModel.Attributes
{
    [AttributeUsage(
        AttributeTargets.Class,
        AllowMultiple = false)]
    public class BulkAddFieldFunction : BulkAddFunction
    {
        public override Type TargetPropertyType
        {
            get { return typeof(RecordField); }
        }

        public override IRecordService GetQueryLookupService(RecordEntryViewModelBase recordForm, string subGridReference)
        {
            var targetPropertyInfo = GetTargetProperty(recordForm, subGridReference);

            var gridField = GetEntryViewModel(recordForm).GetEnumerableFieldViewModel(subGridReference);

            var alreadySelected = gridField.DynamicGridViewModel != null
                ? gridField.GridRecords.Select(g => g.GetRecordFieldFieldViewModel(targetPropertyInfo.Name)?.Value.Key).ToArray()
                : gridField.Value == null ? new string[0] : gridField.Value.Cast<object>().Select(o => o.GetPropertyValue(targetPropertyInfo.Name)).Cast<RecordField>().Select(f => f.Key).ToArray();

            var recordType = recordForm.FormService.GetDependantValue(subGridReference + "." + targetPropertyInfo.Name, GetEnumeratedType(recordForm, subGridReference).AssemblyQualifiedName, recordForm);
            var lookupService = GetLookupService(recordForm, subGridReference);

            //removed field searchable as inadvertently left out fields
            var fields = lookupService.GetFieldMetadata(recordType)
                .Where(f => (!alreadySelected?.Any(k => k == f.SchemaName) ?? true))
                .OrderBy(r => r.DisplayName)
                .ToArray();

            var queryTypesObject = new FieldsObject
            {
                Fields = fields
            };
            return new ObjectRecordService(queryTypesObject, recordForm.ApplicationController);
        }

        public override string GetTargetType(RecordEntryViewModelBase recordForm, string subGridReference)
        {
            return typeof(IFieldMetadata).AssemblyQualifiedName;
        }

        public override bool TypeAhead {  get { return true; } }

        public override bool AllowQuery { get { return false; } }

        public override void AddSelectedItem(GridRowViewModel selectedRow, RecordEntryViewModelBase recordForm, string subGridReference)
        {
            var gridField = GetEntryViewModel(recordForm).GetEnumerableFieldViewModel(subGridReference);
            var targetPropertyname = GetTargetProperty(recordForm, subGridReference).Name;
            var newRecord = recordForm.RecordService.NewRecord(GetEnumeratedType(recordForm, subGridReference).AssemblyQualifiedName);

            var selectedRowrecord = selectedRow.GetRecord() as ObjectRecord;
            if (selectedRowrecord != null)
            {
                var newRecordType = new RecordField();
                newRecordType.Key = (string)selectedRowrecord.Instance.GetPropertyValue(nameof(IFieldMetadata.SchemaName));
                newRecordType.Value = (string)selectedRowrecord.Instance.GetPropertyValue(nameof(IFieldMetadata.DisplayName));
                newRecord.SetField(targetPropertyname, newRecordType, recordForm.RecordService);

                //if (gridField.GridRecords.Any(g => g.GetRecordFieldFieldViewModel(targetPropertyname).Value == newRecordType))
                //    return;
                InsertNewItem(recordForm, subGridReference, newRecord);
            }
        }

        public class FieldsObject
        {
            public IEnumerable<IFieldMetadata> Fields { get; set; }
        }
    }
}