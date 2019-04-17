using JosephM.Application.ViewModel.RecordEntry.Form;

namespace JosephM.Application.ViewModel.RecordEntry.Field
{
    public class StringFieldViewModel : FieldViewModel<string>
    {
        public StringFieldViewModel(string fieldName, string label, RecordEntryViewModelBase recordForm)
            : base(fieldName, label, recordForm)
        {
            var autocompletes = FormService?.GetAutocompletesStrings(this);
            if (autocompletes != null)
                AutocompleteViewModel = new StringAutocompleteViewModel(this, autocompletes);
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

        public StringAutocompleteViewModel AutocompleteViewModel { get; set; }

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
    }
}