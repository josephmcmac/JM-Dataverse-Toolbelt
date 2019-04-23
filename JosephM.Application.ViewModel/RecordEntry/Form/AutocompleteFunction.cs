using System;
using System.Collections.Generic;

namespace JosephM.Application.ViewModel.RecordEntry.Form
{
    public class AutocompleteFunction
    {
        public AutocompleteFunction(Func<RecordEntryViewModelBase, IEnumerable<AutocompleteOption>> getAutocompleteStringsFunction, Func<RecordEntryViewModelBase, bool> isValidForFormFunction = null, bool displayInGrid = true, bool autosearch = true, bool displayNames = false)
        {
            DisplayInGrid = displayInGrid;
            Autosearch = autosearch;
            DisplayNames = displayNames;
            GetAutocompleteStringsFunction = getAutocompleteStringsFunction;
            IsValidForFormFunction = isValidForFormFunction;
        }

        public bool DisplayInGrid { get; set; }
        public bool Autosearch { get; }
        public bool DisplayNames { get; }
        private Func<RecordEntryViewModelBase, IEnumerable<AutocompleteOption>> GetAutocompleteStringsFunction { get; set; }

        public IEnumerable<AutocompleteOption> GetAutocompleteStrings(RecordEntryViewModelBase entryViewModel)
        {
            return GetAutocompleteStringsFunction(entryViewModel);
        }

        private Func<RecordEntryViewModelBase, bool> IsValidForFormFunction { get; set; }

        public bool IsValidForForm(RecordEntryViewModelBase entryViewModel)
        {
            return IsValidForFormFunction == null
                ? true
                : IsValidForFormFunction(entryViewModel);
        }
    }
}
