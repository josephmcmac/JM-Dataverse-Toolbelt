using JosephM.Application.ViewModel.Grid;
using JosephM.Application.ViewModel.RecordEntry.Form;
using JosephM.Application.ViewModel.RecordEntry.Metadata;
using JosephM.Application.ViewModel.Shared;
using JosephM.Core.FieldType;
using System.Collections.Generic;
using System.Linq;

namespace JosephM.Application.ViewModel.RecordEntry.Field
{
    public class RecordTypeFieldViewModel : DropdownFieldViewModel<RecordType>, IAutocompleteViewModel
    {
        public RecordTypeFieldViewModel(string fieldName, string label, RecordEntryViewModelBase recordForm, bool usePicklist)
            : base(fieldName, label, recordForm)
        {
            var autocompleteFunction = new AutocompleteFunction(GetAutocomplete, typeof(RecordTypeAutocomplete), nameof(RecordTypeAutocomplete.LogicalName), new[] { new GridFieldMetadata(nameof(RecordTypeAutocomplete.LogicalName)), new GridFieldMetadata(nameof(RecordTypeAutocomplete.DisplayName)) }, autosearch: false)
            {
                CacheAsStaticList = true
            };

            AutocompleteViewModel = new AutocompleteViewModel(this, autocompleteFunction);
            SearchButton = new XrmButtonViewModel("Search", () => { AutocompleteViewModel.SearchText = SearchText; Search(); }, ApplicationController);
            UsePicklist = usePicklist;
            SearchText = Value?.Value;
        }

        public AutocompleteViewModel AutocompleteViewModel { get; set; }

        public override RecordType Value
        {
            get
            {
                return ValueObject is string
                    ? new RecordType(ValueObject.ToString(), ValueObject.ToString())
                    : ValueObject as RecordType;
            }
            set
            {
                ValueObject = value;
                OnPropertyChanged(nameof(Value));
            }
        }

        private string _searchText;

        public string SearchText
        {
            get { return _searchText; }
            set
            {
                _searchText = value;
                SetValue(null);
                OnPropertyChanged(nameof(SearchText));
            }
        }

        public void SetValue(GridRowViewModel selectedRow)
        {
            if(selectedRow != null)
            {
                Value = new RecordType(selectedRow.GetStringFieldFieldViewModel(nameof(RecordTypeAutocomplete.LogicalName)).Value, selectedRow.GetStringFieldFieldViewModel(nameof(RecordTypeAutocomplete.DisplayName)).Value);
                SearchText = Value.Value;
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

        public IEnumerable<object> GetAutocomplete(RecordEntryViewModelBase recordEntryViewModel)
        {
            if(ItemsSource == null)
            {
                return new AutocompleteOption[0];
            }
            return ItemsSource
                .Select(kv => new RecordTypeAutocomplete(kv.Key, kv.Value))
                .ToArray();
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

        public bool UsePicklist { get; }

        public class RecordTypeAutocomplete
        {
            public RecordTypeAutocomplete(string logicalName, string displayName)
            {
                LogicalName = logicalName;
                DisplayName = displayName;
            }

            public string LogicalName { get; set; }
            public string DisplayName { get; set; }
        }

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

        protected override void OnItemsLoaded()
        {
            base.OnItemsLoaded();
            AutocompleteViewModel.DynamicGridViewModel.ReloadGrid();
        }
    }
}