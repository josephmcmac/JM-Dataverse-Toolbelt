using JosephM.Application.ViewModel.Grid;
using JosephM.Application.ViewModel.RecordEntry.Form;
using JosephM.Record.IService;
using JosephM.Record.Metadata;
using JosephM.Record.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using JosephM.Core.Extentions;

namespace JosephM.Application.ViewModel.RecordEntry.Field
{
    public class StringAutocompleteViewModel : ViewModelBase
    {
        private bool _displayTypeAhead;
        private string _loadOptionsError;

        public StringAutocompleteViewModel(StringFieldViewModel stringField, AutocompleteFunction autocompleteFunction)
            : base(stringField.ApplicationController)
        {
            SearchText = stringField.Value;
            StringField = stringField;
            AutocompleteFunction = autocompleteFunction;
            var typeAheadOptions = new TypeAheadOptions();

            var typeAheadRecordService = new ObjectRecordService(typeAheadOptions, ApplicationController);
            var formController = FormController.CreateForObject(typeAheadOptions, ApplicationController, null);

            Func<bool, GetGridRecordsResponse> getGridRecords = (ignorePages) =>
                {
                    try
                    {
                        LoadOptionsError = null;
                        var autoCompleteStrings = autocompleteFunction
                            .GetAutocompleteStrings(StringField.RecordEntryViewModel);
                        if (autoCompleteStrings == null)
                            return new GetGridRecordsResponse(new IRecord[0]);
                        var searchToLower = SearchText?.ToLower();
                        typeAheadOptions.Options = autoCompleteStrings
                            .Where(ta => searchToLower == null || AutocompleteFunction.SearchFields.Any(sf => ta.GetPropertyValue(sf)?.ToString().ToLower().StartsWith(searchToLower) ?? false))
                            .OrderBy(ta => (string)ta.GetPropertyValue(AutocompleteFunction.SortField))
                            .ThenBy(ta => (string)ta.GetPropertyValue(AutocompleteFunction.ValueField));

                        var records = typeAheadOptions
                            .Options
                            .Select(o => new ObjectRecord(o))
                            .Take(MaxRecordsForLookup)
                            .ToArray();
                        if (stringField.DisplayAutocomplete
                            && (!records.Any()
                                || (records.Count() == 1 && records.First().GetStringField(AutocompleteFunction.ValueField)?.ToLower() == searchToLower)))
                        {
                            stringField.DisplayAutocomplete = false;
                        }
                        return new GetGridRecordsResponse(records);
                    }
                    catch(Exception ex)
                    {
                        LoadOptionsError = $"Autocomplete could not be loaded\n\n{ex.Message}{(ex.InnerException == null ? null : ("\n\n" + ex.InnerException.Message))}\n\n{ex.StackTrace}";
                        return new GetGridRecordsResponse(new IRecord[0]);
                    }
                };

            DynamicGridViewModel = new DynamicGridViewModel(ApplicationController)
            {
                FormController = formController,
                GetGridRecords = getGridRecords,
                OnDoubleClick = OnDoubleClick,
                ViewType = ViewType.LookupView,
                RecordService = typeAheadRecordService,
                RecordType = AutocompleteFunction.RecordType,
                IsReadOnly = true,
                DisplayHeaders = false,
                NoMargins = true,
                FieldMetadata = AutocompleteFunction.GridFields
            };
        }

        protected bool DisplayTypeAhead { get => _displayTypeAhead; set => _displayTypeAhead = value; }

        protected int MaxRecordsForLookup
        {
            get { return 50; }
        }

        public void OnKeyDown()
        {
        }

        public void OnDoubleClick()
        {
            SetToSelectedRow();
        }

        public void SetToSelectedRow()
        {
            if (DynamicGridViewModel.SelectedRow != null)
            {
                StringField.Value = DynamicGridViewModel.SelectedRow.GetStringFieldFieldViewModel(AutocompleteFunction.ValueField).Value;
                StringField.DisplayAutocomplete = false;
            }
        }

        public void MoveDown()
        {
            try
            {
                if (DynamicGridViewModel.GridRecords != null && DynamicGridViewModel.GridRecords.Any())
                {
                    var index = -1;
                    if (DynamicGridViewModel.SelectedRow != null)
                        index = DynamicGridViewModel.GridRecords.IndexOf(DynamicGridViewModel.SelectedRow);
                    index++;
                    if (index > DynamicGridViewModel.GridRecords.Count - 1)
                        index = 0;
                    DynamicGridViewModel.SelectedRow = DynamicGridViewModel.GridRecords[index];
                }
            }
            catch
            {
            }
        }

        public void MoveUp()
        {
            try
            {
                if (DynamicGridViewModel.GridRecords != null && DynamicGridViewModel.GridRecords.Any())
                {
                    var index = DynamicGridViewModel.GridRecords.Count;
                    if (DynamicGridViewModel.SelectedRow != null)
                        index = DynamicGridViewModel.GridRecords.IndexOf(DynamicGridViewModel.SelectedRow);
                    index--;
                    if (index < 0)
                        index = DynamicGridViewModel.GridRecords.Count - 1;
                    DynamicGridViewModel.SelectedRow = DynamicGridViewModel.GridRecords[index];
                }
            }
            catch
            {
            }
        }

        public DynamicGridViewModel DynamicGridViewModel { get; set; }
        public StringFieldViewModel StringField { get; }
        private AutocompleteFunction AutocompleteFunction { get; }
        public bool AutoSearch => AutocompleteFunction.Autosearch;
        public string SearchText { get; set; }

        public class TypeAheadOptions
        {
            public IEnumerable<object> Options { get; set; }
        }

        public string LoadOptionsError
        {
            get { return _loadOptionsError; }
            set
            {
                _loadOptionsError = value;
                OnPropertyChanged(nameof(LoadOptionsError));
            }
        }
    }
}