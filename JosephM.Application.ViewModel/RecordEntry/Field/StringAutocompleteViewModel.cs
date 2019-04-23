using JosephM.Application.ViewModel.Grid;
using JosephM.Application.ViewModel.RecordEntry.Form;
using JosephM.Core.Attributes;
using JosephM.Record.IService;
using JosephM.Record.Metadata;
using JosephM.Record.Service;
using System;
using System.Collections.Generic;
using System.Linq;

namespace JosephM.Application.ViewModel.RecordEntry.Field
{
    public class StringAutocompleteViewModel : ViewModelBase
    {
        private bool _displayTypeAhead;

        public StringAutocompleteViewModel(StringFieldViewModel stringField, AutocompleteFunction autocompleteFunction)
            : base(stringField.ApplicationController)
        {
            SearchText = stringField.Value;
            StringField = stringField;
            AutocompleteFunction = autocompleteFunction;
            var typeAheadOptions = new TypeAheadOptions();
            typeAheadOptions.Options = new[]
            {
                //fake initial option before actual loading
                new AutocompleteOption(AutocompleteFunction.DisplayNames ? "Loading..." : null, "Loading...")
            };
                
            var typeAheadRecordService = new ObjectRecordService(typeAheadOptions, ApplicationController);
            var formController = FormController.CreateForObject(typeAheadOptions, ApplicationController, null);

            Func<bool, GetGridRecordsResponse> getGridRecords = (ignorePages) =>
                {
                    var autoCompleteStrings = autocompleteFunction
                        .GetAutocompleteStrings(StringField.RecordEntryViewModel);
                    if(autoCompleteStrings == null)
                        return new GetGridRecordsResponse(new IRecord[0]);
                    typeAheadOptions.Options = autoCompleteStrings
                        .Where(ta => SearchText == null || (ta.Value?.ToLower().StartsWith(SearchText.ToLower()) ?? false) || (ta.Name?.ToLower().StartsWith(SearchText.ToLower()) ?? false))
                        .OrderBy(s => s.Name).ThenBy(s => s.Value);
                    var records = typeAheadRecordService
                    .GetLinkedRecords(typeof(AutocompleteOption).AssemblyQualifiedName, typeof(TypeAheadOptions).AssemblyQualifiedName, nameof(TypeAheadOptions.Options), typeAheadOptions.ToString())
                    .Take(MaxRecordsForLookup)
                    .ToArray();
                    if(stringField.DisplayAutocomplete
                        && (!records.Any()
                            || (records.Count() == 1 && records.First().GetStringField(nameof(AutocompleteOption.Value)) == SearchText)))
                        stringField.DisplayAutocomplete = false;
                    return new GetGridRecordsResponse(records);
                };

            DynamicGridViewModel = new DynamicGridViewModel(ApplicationController)
            {
                FormController = formController,
                GetGridRecords = getGridRecords,
                OnDoubleClick = OnDoubleClick,
                ViewType = ViewType.LookupView,
                RecordService = typeAheadRecordService,
                RecordType = typeof(AutocompleteOption).AssemblyQualifiedName,
                IsReadOnly = true,
                DisplayHeaders= false,
                NoMargins = true
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
                StringField.Value = DynamicGridViewModel.SelectedRow.GetStringFieldFieldViewModel(nameof(AutocompleteOption.Value)).Value;
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
            public IEnumerable<AutocompleteOption> Options { get; set; }
        }
    }
}