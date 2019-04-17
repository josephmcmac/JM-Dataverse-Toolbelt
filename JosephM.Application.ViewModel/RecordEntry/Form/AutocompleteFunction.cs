using System;
using System.Collections.Generic;

namespace JosephM.Application.ViewModel.RecordEntry.Form
{
    public class AutocompleteFunction
    {
        public AutocompleteFunction(Func<RecordEntryViewModelBase, IEnumerable<string>> getAutocompleteStringsFunction)
        {
            GetAutocompleteStringsFunction = getAutocompleteStringsFunction;
        }

        private Func<RecordEntryViewModelBase, IEnumerable<string>> GetAutocompleteStringsFunction { get; set; }

        public IEnumerable<string> GetAutocompleteStrings(RecordEntryViewModelBase entryViewModel)
        {
            return GetAutocompleteStringsFunction(entryViewModel);
        }
    }
}
