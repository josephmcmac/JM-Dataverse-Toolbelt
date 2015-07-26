#region

using System.Collections.Generic;
using System.Linq;
using JosephM.Core.Extentions;
using JosephM.Core.FieldType;
using JosephM.Record.Application.Grid;
using JosephM.Record.Application.RecordEntry.Form;
using JosephM.Record.Application.Shared;
using JosephM.Record.IService;
using JosephM.Record.Query;

#endregion

namespace JosephM.Record.Application.RecordEntry.Field
{
    public class LookupFieldViewModel : ReferenceFieldViewModel<Lookup>
    {
        public LookupFieldViewModel(string fieldName, string fieldLabel, RecordEntryViewModelBase recordForm,
            string referencedRecordType)
            : base(fieldName, fieldLabel, recordForm)
        {
            RecordTypeToLookup = referencedRecordType;
            LoadLookupGrid();
        }

        private string _referencedRecordType;
        public override string RecordTypeToLookup
        {
            get { return _referencedRecordType; }
            set
            {
                _referencedRecordType = value;
                LookupGridViewModel = new LookupGridViewModel(this, OnRecordSelected);
            }
        }

        public override void SetValue(IRecord selectedRecord)
        {
            if (selectedRecord == null)
                Value = null;
            else
            {
                var recordName = selectedRecord.GetStringField(LookupService.GetPrimaryField(selectedRecord.Type));
                Value = new Lookup(RecordTypeToLookup, selectedRecord.Id, recordName);
            }
        }

        public override IRecordService LookupService
        {
            get { return RecordEntryViewModel.RecordService.GetLookupService(FieldName, RecordEntryViewModel.GetRecordType(), RecordEntryViewModel.ParentFormReference, RecordEntryViewModel.GetRecord()); }
        }

        protected override string GetValueName()
        {
            if (Value == null)
                return null;
            else
                return Value.Name;
        }

        protected override IEnumerable<IRecord> GetSearchResults()
        {
            var service = RecordEntryViewModel.RecordService.GetLookupService(FieldName,
                RecordEntryViewModel.GetRecordType(), RecordEntryViewModel.ParentFormReference, RecordEntryViewModel.GetRecord());


            var primaryField = LookupService.GetPrimaryField(RecordTypeToLookup);
            var conditions = GetConditions();
            if (!EnteredText.IsNullOrWhiteSpace())
            {
                conditions =
                    conditions.Union(new [] { new Condition(primaryField, ConditionType.BeginsWith, EnteredText)});
            }
            return LookupService.GetFirstX(RecordTypeToLookup, MaxRecordsForLookup, null, conditions, new [] { new SortExpression(primaryField, SortType.Ascending) });
        }
    }
}