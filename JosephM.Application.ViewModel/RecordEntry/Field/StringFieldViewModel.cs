using JosephM.Application.ViewModel.Grid;
using JosephM.Application.ViewModel.RecordEntry.Form;
using JosephM.Application.ViewModel.Shared;

namespace JosephM.Application.ViewModel.RecordEntry.Field
{
    public class StringFieldViewModel : FieldViewModel<string>, IAutocompleteViewModel
    {
        public StringFieldViewModel(string fieldName, string label, RecordEntryViewModelBase recordForm)
            : base(fieldName, label, recordForm)
        {
            AutocompleteFunction = FormService?.GetAutocompletesFunction(GetRecordTypeOfThisField(), FieldName, ApplicationController);
            if (AutocompleteFunction != null
                && AutocompleteFunction.IsValidForForm(RecordEntryViewModel)
                && (AutocompleteFunction.DisplayInGrid || !(RecordEntryViewModel is GridRowViewModel)))
            {
                AutocompleteViewModel = new AutocompleteViewModel(this, AutocompleteFunction);
                SearchButton = new XrmButtonViewModel("Search", () => { AutocompleteViewModel.SearchText = Value; Search(); }, ApplicationController);
            }
        }

        public void Search()
        {
            if (AutocompleteViewModel != null)
            {
                DisplayAutocomplete = true;
                AutocompleteViewModel.DynamicGridViewModel.ReloadGrid();
            }
        }

        public override string Value
        {
            get { return ValueObject == null ? null : ValueObject.ToString(); }
            set { ValueObject = value; }
        }

        public int? MaxLength { get; set; }

        public bool IsMultiline { get; set; }

        public int NumberOfLines
        {
            get { return IsMultiline ? 10 : 1; }
        }

        private bool _displayAutocomplete;
        public bool DisplayAutocomplete
        {
            get
            {
                return _displayAutocomplete;
            }
            set
            {
                _displayAutocomplete = value;
                OnPropertyChanged(nameof(DisplayAutocomplete));
            }
        }

        public AutocompleteViewModel AutocompleteViewModel { get; set; }

        public void SelectAutocompleteGrid()
        {
            if (DisplayAutocomplete && AutocompleteViewModel != null)
            {
                //move logical and key foucs to the grid view
                //have to add in the false focus first to ensire the ui properly processes the cvhange to focus on it
                AutocompleteViewModel.DynamicGridViewModel.IsFocused = false;
                AutocompleteViewModel.MoveDown();
                AutocompleteViewModel.DynamicGridViewModel.IsFocused = true;
            }
        }

        void IAutocompleteViewModel.SetValue(GridRowViewModel selectedRow)
        {
            Value = selectedRow.GetStringFieldFieldViewModel(AutocompleteFunction.ValueField).Value;
        }

        string IAutocompleteViewModel.SearchText
        {
            get => Value;
            set => Value = value;
        }

        private XrmButtonViewModel _searchButton;

        public XrmButtonViewModel SearchButton
        {
            get { return _searchButton; }
            set
            {
                _searchButton = value;
                OnPropertyChanged(nameof(SearchButton));
            }
        }

        public AutocompleteFunction AutocompleteFunction { get; }
    }
}