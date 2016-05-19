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

#endregion

namespace JosephM.Application.ViewModel.RecordEntry.Field
{
    public class LookupFieldViewModel : ReferenceFieldViewModel<Lookup>
    {
        public LookupFieldViewModel(string fieldName, string fieldLabel, RecordEntryViewModelBase recordForm,
            string referencedRecordType, bool usePicklist)
            : base(fieldName, fieldLabel, recordForm)
        {
            UsePicklist = usePicklist;
            RecordTypeToLookup = referencedRecordType;
            if (Value != null)
            {
                if (Value.Name.IsNullOrWhiteSpace())
                    Value.Name = "Record Name Not Set";
                SetEnteredTestWithoutClearingValue(Value.Name);
            }
            if (UsePicklist)
            {
                
                ItemsSource = GetSearchResults().Select(r => LookupService.ToLookup(r)).ToArray();
            }
            else
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

        public bool UsePicklist { get; set; }

        protected override bool SetEnteredText { get { return !UsePicklist; } }

        private IEnumerable<Lookup> _itemsSource;

        public IEnumerable<Lookup> ItemsSource
        {
            get { return _itemsSource; }
            set
            {
                _itemsSource = value;
                OnPropertyChanged("ItemsSource");
                if (Value != null && _itemsSource != null)
                {
                    var matchingItems = _itemsSource.Where(p => p.Id == Value.Id);
                    if (matchingItems.Any())
                        Value = matchingItems.First() ?? Value;
                }
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
            if(LookupService == null)
                throw new NullReferenceException(string.Format("Error searching field {0}. {1} is null", FieldName, nameof(LookupService)));
            var primaryField = LookupService.GetPrimaryField(RecordTypeToLookup);
            var conditions = GetConditions();
            if (!UsePicklist && !EnteredText.IsNullOrWhiteSpace())
            {
                conditions =
                    conditions.Union(new [] { new Condition(primaryField, ConditionType.BeginsWith, EnteredText)});
            }
            return LookupService.GetFirstX(RecordTypeToLookup, UsePicklist ? -1 : MaxRecordsForLookup, null, conditions, new [] { new SortExpression(primaryField, SortType.Ascending) });
        }
    }
}