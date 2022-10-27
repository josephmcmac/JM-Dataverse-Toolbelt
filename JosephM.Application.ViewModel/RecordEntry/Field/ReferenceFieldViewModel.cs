﻿using JosephM.Application.ViewModel.RecordEntry.Form;
using JosephM.Application.ViewModel.Shared;
using JosephM.Core.Extentions;
using JosephM.Record.IService;
using System;
using System.Collections.Generic;
using System.Linq;

namespace JosephM.Application.ViewModel.RecordEntry.Field
{
    public abstract class ReferenceFieldViewModel<T> : FieldViewModel<T>, IReferenceFieldViewModel
    {
        protected ReferenceFieldViewModel(string fieldName, string fieldLabel, RecordEntryViewModelBase recordForm, bool usePicklist)
            : base(fieldName, fieldLabel, recordForm)
        {
            EditableFormWidth = 125;
            UsePicklist = usePicklist;
            if (Value != null)
                SetEnteredTestWithoutClearingValue(GetValueName());
        }

        public bool UsePicklist { get; set; }

        private IEnumerable<ReferencePicklistItem> _itemsSource;

        private object _lockoObject = new object();
        public IEnumerable<ReferencePicklistItem> ItemsSource
        {
            get
            {
                lock (_lockoObject)
                {
                    if (_itemsSource == null && UsePicklist)
                    {
                        LoadPicklistItems();
                    }
                    return _itemsSource;
                }
            }
            set
            {
                lock (_lockoObject)
                {
                    _itemsSource = value;
                    OnPropertyChanged(nameof(ItemsSource));
                    OnPropertyChanged(nameof(ItemsSourceAsync));
                    SelectedItem = MatchSelectedItemInItemsSourceToValue();
                }
            }
        }

        public IEnumerable<ReferencePicklistItem> ItemsSourceAsync
        {
            get
            {
                lock (_lockoObject)
                {
                    if (_itemsSource == null && UsePicklist)
                    {
                        DoOnAsynchThread(() =>
                        {
                            LoadingViewModel.IsLoading = true;
                            try
                            {
                                LoadPicklistItems();
                            }
                            finally
                            {
                                LoadingViewModel.IsLoading = false;
                            }
                        });
                    }
                    return _itemsSource;
                }
            }
        }

        public void LoadPicklistItems()
        {
            try
            {
                _itemsSource = new ReferencePicklistItem[0];
                if (LookupService != null)
                {
                    ItemsSource = (FormService?.OrderPicklistItems(FieldName, GetRecordTypeOfThisField(), GetPicklistOptions()) ?? GetPicklistOptions().OrderBy(p => p.Name)).ToArray();
                    if (Value == null && ItemsSource.Count(i => i.Record != null) == 1 && (FormService?.InitialisePicklistIfOneOption(FieldName, GetRecordTypeOfThisField()) ?? false))
                        SelectedItem = ItemsSource.First(i => i.Record != null);
                }
                var matchingItem = MatchSelectedItemInItemsSourceToValue();
                if (matchingItem == null)
                {
                    SelectedItem = GetValueAsPicklistItem();
                    if (SelectedItem != null)
                        ItemsSource = ItemsSource.Union(new[] { SelectedItem });
                }
                else
                    SelectedItem = matchingItem;
                OnPropertyChanged(nameof(SelectedItem));
            }
            catch(Exception ex)
            {
                AddError($"Error Loading Picklist Options\n\n{ex.DisplayString()}");
                ApplicationController.LogEvent("Error Loading Picklist Options", new Dictionary<string, string>
                {
                    { "Is Error", true.ToString() },
                    { "Record Type", Record.Type },
                    { "Field", FieldName },
                    { "ReferencedType", RecordTypeToLookup },
                    { "Error Trace", ex.DisplayString() }
                });
            }
        }

        public abstract ReferencePicklistItem GetValueAsPicklistItem();

        private ReferencePicklistItem _selectedItem;

        public ReferencePicklistItem SelectedItem
        {
            get
            {
                return _selectedItem;
            }
            set
            {
                if (_selectedItem != value)
                {
                    _selectedItem = value;
                    MatchValueToSelectedItems();
                    OnPropertyChanged("SelectedItem");
                }
            }
        }

        public class ReferencePicklistItem
        {
            public ReferencePicklistItem(IRecord record, string name)
            {
                Record = record;
                Name = name;
            }

            public IRecord Record { get; set; }
            public string Name { get; set; }
        }

        protected abstract ReferencePicklistItem MatchSelectedItemInItemsSourceToValue();

        protected abstract void MatchValueToSelectedItems();

        protected abstract IEnumerable<ReferencePicklistItem> GetPicklistOptions();

        public virtual void ConnectionForChanged()
        {
            if (UsePicklist)
            {
                //for some reason this was triggering too many times on load and not initialising selected item properly in Lookups
                //so have added constraint to only load _itemsSource once
                if (_itemsSource != null)
                    _itemsSource = null;
                SetLoading();
                DoOnAsynchThread(() =>
                {

                    try
                    {
                        ItemsSource = GetPicklistOptions().OrderBy(p => p.Name).ToArray();
                    }
                    catch (Exception ex)
                    {
                        RecordEntryViewModel.ApplicationController.ThrowException(ex);
                    }
                    finally
                    {
                        SetNotLoading();
                    }
                });
            }
            else
                LoadLookupGrid();
        }

        public void LoadLookupGrid()
        {
            LookupGridViewModel = new LookupGridViewModel(this, OnRecordSelected);
            XrmButton = new XrmButtonViewModel("Search", Search, ApplicationController);
        }

        public abstract string RecordTypeToLookup { get; set; }

        public void Search()
        {
            lock (_searchLock)
            {
                LookupGridVisible = true;
                if (LookupGridViewModel.DynamicGridViewModel.CurrentPage != 1)
                {
                    LookupGridViewModel.DynamicGridViewModel.CurrentPage = 1;
                }
                else
                {
                    LookupGridViewModel.DynamicGridViewModel.ReloadGrid();
                }
                OnPropertyChanged(nameof(LookupGridViewModel));
            }
        }

        public void OnRecordSelected(IRecord selectedRecord)
        {
            if (selectedRecord != null)
            {
                SetValue(selectedRecord);
                LookupGridVisible = false;
                var name = GetValueName();
                SetEnteredTestWithoutClearingValue(name);
            }
        }

        protected abstract string GetValueName();

        public abstract void SetValue(IRecord selectedRecord);

        protected virtual bool SetEnteredText
        {
            get { return true; }
        }
        protected void SetEnteredTestWithoutClearingValue(string recordName)
        {
            if (!SetEnteredText)
                return;
            _enteredText = recordName;
            OnPropertyChanged(nameof(EnteredText));
        }

        public abstract IRecordService LookupService { get; }

        private string _enteredText;

        public string EnteredText
        {
            get { return _enteredText; }
            set
            {
                _enteredText = value;
                SetValue(null);
                OnPropertyChanged(nameof(EnteredText));
            }
        }

        private XrmButtonViewModel _xrmButton;

        public XrmButtonViewModel XrmButton
        {
            get { return _xrmButton; }
            set
            {
                _xrmButton = value;
                OnPropertyChanged("XrmButton");
            }
        }

        public bool AllowNew
        {
            get
            {
                return NewAction != null;
            }
        }

        private Action _newAction;
        public Action NewAction
        {
            get
            {
                return _newAction;
            }
            set
            {
                _newAction = value;
                if (_newAction != null)
                    NewButton = new XrmButtonViewModel("New", () => { try { _newAction(); } catch (Exception ex) { ApplicationController.ThrowException(ex); } } , ApplicationController);
                OnPropertyChanged(nameof(AllowNew));
            }
        }

        private XrmButtonViewModel _newButton;
        public XrmButtonViewModel NewButton
        {
            get { return _newButton; }
            set
            {
                _newButton = value;
                OnPropertyChanged(nameof(NewButton));
            }
        }

        private bool _lookupGridVisible;

        public bool LookupGridVisible
        {
            get { return _lookupGridVisible; }
            set
            {
                _lookupGridVisible = value;
                OnPropertyChanged(nameof(LookupGridVisible));
            }
        }

        private LookupGridViewModel _lookupGridViewModel;

        public LookupGridViewModel LookupGridViewModel
        {
            get { return _lookupGridViewModel; }
            set
            {
                _lookupGridViewModel = value;
                OnPropertyChanged(nameof(LookupGridViewModel));
            }
        }

        private bool _searching;

        public bool Searching
        {
            get { return _searching; }
            set
            {
                _searching = value;
                OnPropertyChanged(nameof(Searching));
            }
        }

        private readonly object _searchLock = new object();

        protected abstract IEnumerable<IRecord> GetSearchResults();

        public void SelectLookupGrid()
        {
            if (LookupGridVisible)
            {
                //move logical and key foucs to the grid view
                //have to add in the false focus first to ensire the ui properly processes the cvhange to focus on it
                LookupGridViewModel.DynamicGridViewModel.IsFocused = false;
                LookupGridViewModel.MoveDown();
                LookupGridViewModel.DynamicGridViewModel.IsFocused = true;
            }
        }
    }
}