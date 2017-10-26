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

            var gridField = GetObjectFormService(recordForm).GetSubGridViewModel(subGridReference);
            var gridRecords = gridField.GridRecords;
            
            var recordType = recordForm.FormService.GetDependantValue(subGridReference + "." + targetPropertyInfo.Name, GetEnumeratedType(recordForm, subGridReference).AssemblyQualifiedName, recordForm);
            var lookupService = GetLookupService(recordForm, subGridReference);
            var fields = lookupService.GetFieldMetadata(recordType)
                .Where(f => f.Searchable && (!gridRecords?.Any(g => g.GetRecordFieldFieldViewModel(targetPropertyInfo.Name)?.Value.Key == f.SchemaName) ?? true))
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
            var gridField = GetObjectFormService(recordForm).GetSubGridViewModel(subGridReference);
            var targetPropertyname = GetTargetProperty(recordForm, subGridReference).Name;
            var newRecord = recordForm.RecordService.NewRecord(GetEnumeratedType(recordForm, subGridReference).AssemblyQualifiedName);

            var selectedRowrecord = selectedRow.GetRecord() as ObjectRecord;
            if (selectedRowrecord != null)
            {
                var newRecordType = new RecordField();
                newRecordType.Key = (string)selectedRowrecord.Instance.GetPropertyValue(nameof(IFieldMetadata.SchemaName));
                newRecordType.Value = (string)selectedRowrecord.Instance.GetPropertyValue(nameof(IFieldMetadata.DisplayName));

                newRecord.SetField(targetPropertyname, newRecordType, recordForm.RecordService);
                if (gridField.GridRecords.Any(g => g.GetRecordFieldFieldViewModel(targetPropertyname).Value == newRecordType))
                    return;
                newRecord.SetField(targetPropertyname, newRecordType, recordForm.RecordService);
                gridField.InsertRecord(newRecord, 0);
            }
        }

        public class FieldsObject
        {
            public IEnumerable<IFieldMetadata> Fields { get; set; }
        }
    }
}