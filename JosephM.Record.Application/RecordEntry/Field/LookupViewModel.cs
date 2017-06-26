#region

using System;
using System.Collections.Generic;
using System.Linq;
using JosephM.Application.ViewModel.RecordEntry.Form;
using JosephM.Core.Extentions;
using JosephM.Core.FieldType;
using JosephM.Record.Extentions;
using JosephM.Record.IService;
using JosephM.Record.Query;
using JosephM.Record.Service;

#endregion

namespace JosephM.Application.ViewModel.RecordEntry.Field
{
    public class LookupFieldViewModel : ReferenceFieldViewModel<Lookup>
    {
        public LookupFieldViewModel(string fieldName, string fieldLabel, RecordEntryViewModelBase recordForm,
            string referencedRecordType, bool usePicklist)
            : base(fieldName, fieldLabel, recordForm, usePicklist)
        {
            RecordTypeToLookup = referencedRecordType;
            if (Value != null)
            {
                if (Value.Name.IsNullOrWhiteSpace())
                    Value.Name = "Record Name Not Set";
                SetEnteredTestWithoutClearingValue(Value.Name);
            }
            if (!UsePicklist)
                LoadLookupGrid();
        }

        protected override ReferencePicklistItem MatchSelectedItemInItemsSourceToValue()
        {
            if (Value == null)
                return null;
            else if (ItemsSource != null)
            {
                foreach (var item in ItemsSource)
                {
                    if (item.Record.ToLookup() == Value)
                    {
                        return item;
                    }
                }
            }
            return null;
        }

        public override ReferencePicklistItem GetValueAsPicklistItem()
        {
            if (Value == null)
                return null;
            var iReocrd = new RecordObject(RecordTypeToLookup);
            iReocrd.Id = Value.Id;
            return new ReferencePicklistItem(iReocrd, Value.Name);
        }

        protected override void MatchValueToSelectedItems()
        {
            Lookup newValue = null;
            if (SelectedItem != null)
                newValue = LookupService.ToLookup(SelectedItem.Record);
            if (newValue != Value)
                Value = newValue;
        }

        protected override IEnumerable<ReferencePicklistItem> GetPicklistOptions()
        {
            return GetSearchResults()
                .Select(r => new ReferencePicklistItem(r, r.GetStringField(FormService.GetPicklistDisplayField(FieldName, GetRecordType(), LookupService, RecordTypeToLookup))))
                .ToArray();
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

        protected override bool SetEnteredText { get { return !UsePicklist; } }

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
            if(LookupService == null)
                throw new NullReferenceException(string.Format("Error searching field {0}. {1} is null", FieldName, "LookupService"));
            if (UsePicklist)
            {
                return FormService.GetLookupPicklist(FieldName, RecordEntryViewModel.GetRecordType(),
                    RecordEntryViewModel.ParentFormReference, RecordEntryViewModel.GetRecord(), LookupService, RecordTypeToLookup);
            }
            else
            {
                var primaryField = LookupService.GetPrimaryField(RecordTypeToLookup);
                var conditions = FormService.GetLookupConditions(FieldName, RecordEntryViewModel.GetRecordType(),
                    RecordEntryViewModel.ParentFormReference, RecordEntryViewModel.GetRecord());
                if (!EnteredText.IsNullOrWhiteSpace())
                {
                    conditions =
                        conditions.Union(new[] {new Condition(primaryField, ConditionType.BeginsWith, EnteredText)});
                }
                return LookupService.GetFirstX(RecordTypeToLookup, UsePicklist ? -1 : MaxRecordsForLookup, null,
                    conditions, new[] {new SortExpression(primaryField, SortType.Ascending)});
            }
        }
    }
}