﻿using System;
using System.Collections.Generic;
using System.Linq;
using JosephM.Application.ViewModel.RecordEntry.Form;
using JosephM.Core.Extentions;
using JosephM.Core.FieldType;
using JosephM.Record.Extentions;
using JosephM.Record.IService;
using JosephM.Record.Query;
using JosephM.Record.Service;
using JosephM.Application.ViewModel.RecordEntry.Metadata;

namespace JosephM.Application.ViewModel.RecordEntry.Field
{
    public class LookupFieldViewModel : ReferenceFieldViewModel<Lookup>
    {
        public LookupFieldViewModel(string fieldName, string fieldLabel, RecordEntryViewModelBase recordForm,
                 string referencedRecordType, bool usePicklist, bool isEditable)
                 : base(fieldName, fieldLabel, recordForm, usePicklist)
        {
            try
            {
                if (referencedRecordType != null)
                {
                    var splitIt = referencedRecordType.Split(',');
                    if (splitIt.Count() == 1)
                    {
                        SelectedRecordType = new RecordType(splitIt.First(), splitIt.First());
                    }
                    else
                    {
                        var recordTypes = LookupService == null
                            ? new RecordType[0]
                            : splitIt
                            .Select(s => LookupService.GetRecordTypeMetadata(s))
                            .Select(r => new RecordType(r.SchemaName, r.DisplayName))
                            .OrderBy(rt => rt.Value)
                            .ToArray();
                        RecordTypeItemsSource = recordTypes;
                    }
                }
                if (Value != null)
                {
                    if (Value.Name.IsNullOrWhiteSpace())
                        Value.Name = "Record Name Not Set";
                    SetEnteredTestWithoutClearingValue(Value.Name);
                    RecordTypeToLookup = Value.RecordType;
                }
                if (isEditable && SelectedRecordType != null)
                {
                    SetNewAction();
                    if (!UsePicklist)
                        LoadLookupGrid();
                }
            }
            catch(Exception ex)
            {
                recordForm.ApplicationController.ThrowException(ex);
            }
        }

        private void SetNewAction()
        {
            if (RecordEntryViewModel.AllowNewLookup && LookupFormService != null && LookupFormService.GetFormMetadata(RecordTypeToLookup, LookupService) != null && FormService!= null && FormService.AllowAddNew(FieldName, GetRecordTypeOfThisField()))
            {
                NewAction = () =>
                {
                    var formController = new FormController(LookupService, LookupFormService, ApplicationController);
                    var newRecord = LookupService.NewRecord(RecordTypeToLookup);

                    Action onSave = () =>
                    {
                        Value = LookupService.ToLookup(newRecord, getNameIfEmpty: true);
                        if (UsePicklist)
                        {
                            var newPicklistItem = new ReferencePicklistItem(newRecord, Value.Name);
                            ItemsSource = ItemsSource
                            .Union(new[] { newPicklistItem })
                            .OrderBy(r => r.Name)
                            .ToArray();
                            SelectedItem = newPicklistItem;
                        }
                        SetEnteredTestWithoutClearingValue(Value.Name);
                        RecordEntryViewModel.ClearChildForm();
                    };

                    var newForm = new CreateOrUpdateViewModel(newRecord, formController, onSave, RecordEntryViewModel.ClearChildForm);
                    RecordEntryViewModel.LoadChildForm(newForm);
                };
            }
            else
                NewAction = null;
        }

        private RecordType _selectedRecordType;
        public RecordType SelectedRecordType
        {
            get { return _selectedRecordType; }
            set
            {
                if (_selectedRecordType != value
                    && value?.Key != Value?.RecordType)
                {
                    Value = null;
                    EnteredText = null;
                }
                _selectedRecordType = value;
                OnPropertyChanged(nameof(SelectedRecordType));
                if (_selectedRecordType != null)
                {
                    LoadLookupGrid();
                }
                OnPropertyChanged(nameof(TypePopulated));
                OnPropertyChanged(nameof(EditableAndTypePopulated));
                OnPropertyChanged(nameof(TypePopulatedOrReadOnly));
            }
        }

        public bool TypePopulated
        {
            get
            {
                return RecordTypeToLookup != null;
            }
        }

        public bool TypePopulatedOrReadOnly
        {
            get
            {
                return TypePopulated || !IsEditable;
            }
        }

        public bool EditableAndTypePopulated
        {
            get
            {
                return IsEditable && TypePopulated;
            }
        }

        public bool DisplayTypeSelection
        {
            get
            {
                return IsEditable && RecordTypeItemsSource != null;
            }
        }

        private IEnumerable<RecordType> _recordTypeItemsSource;

        public IEnumerable<RecordType> RecordTypeItemsSource
        {
            get { return _recordTypeItemsSource; }
            set
            {
                _recordTypeItemsSource = value;
                OnPropertyChanged(nameof(RecordTypeItemsSource));
            }
        }

        protected override ReferencePicklistItem MatchSelectedItemInItemsSourceToValue()
        {
            if (Value == null)
                return null;
            else if (ItemsSource != null)
            {
                foreach (var item in ItemsSource)
                {
                    if (item.Record != null && item.Record.ToLookup() == Value)
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
            if (SelectedItem != null && SelectedItem.Record != null)
                newValue = LookupService.ToLookup(SelectedItem.Record);
            if (newValue != Value)
                Value = newValue;
        }

        protected override IEnumerable<ReferencePicklistItem> GetPicklistOptions()
        {
            return GetSearchResults()
                .Select(r => new ReferencePicklistItem(r, string.Join(" - ", FormService.GetPicklistDisplayFields(FieldName, GetRecordTypeOfThisField(), LookupService, RecordTypeToLookup).Select(r.GetStringField))))
                .Union(new[] { new ReferencePicklistItem(null, null) })
                .ToArray();
        }

        public override string RecordTypeToLookup
        {
            get { return SelectedRecordType?.Key; }
            set
            {
                if (value == null)
                {
                    SelectedRecordType = null;
                }
                else
                {
                    string recordTypeLabel = value;
                    try
                    {
                        recordTypeLabel = LookupService.GetDisplayName(value);
                    }
                    catch(Exception)
                    {
                        recordTypeLabel = value;
                    }
                    //dynamics was oddly loading none as the record type for some system jobs. Possibly where the type got deleted?
                    SelectedRecordType = new RecordType(value, LookupService == null || value == "none" ? value : recordTypeLabel);
                    LookupGridViewModel = new LookupGridViewModel(this, OnRecordSelected);
                }
            }
        }

        protected override bool SetEnteredText { get { return !UsePicklist; } }

        public override void SetValue(IRecord selectedRecord)
        {
            if (selectedRecord == null)
                Value = null;
            else
            {
                Value = LookupService.ToLookupWithAltDisplayNameName(selectedRecord);
            }
        }

        public override IRecordService LookupService
        {
            get { return RecordEntryViewModel.RecordService.GetLookupService(FieldName, RecordEntryViewModel.GetRecordType(), RecordEntryViewModel.ParentFormReference, RecordEntryViewModel.GetRecord()); }
        }

        public FormServiceBase LookupFormService
        {
            get
            {
                //just hack to get around the project heirachys without having to move all the form code into Record project
                //unsure the IFormService is of the type FormServiceBase
                var formService = LookupService?.GetFormService();
                if(formService != null && !(formService is FormServiceBase))
                {
                    throw new NotSupportedException(string.Format("The {0} is An Unexpected Type Of {1}. It Is Required To Be A {2}", typeof(IFormService).Name, formService.GetType().Name, typeof(FormServiceBase).Name));
                }
                return formService as FormServiceBase;
            }
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
            if (LookupService == null)
            {
                throw new ArgumentNullException($"Error searching field {FieldName}. {FieldName} is null", nameof(LookupService));
            }
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

        protected int MaxRecordsForLookup
        {
            get { return 11; }
        }
    }
}