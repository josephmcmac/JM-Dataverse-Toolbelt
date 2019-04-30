using JosephM.Application.ViewModel.RecordEntry.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;

namespace JosephM.Application.ViewModel.RecordEntry.Form
{
    public class AutocompleteFunction
    {
        public AutocompleteFunction(Func<RecordEntryViewModelBase, IEnumerable<AutocompleteOption>> getAutocompleteStringsFunction, double? gridWidth = null, Func<RecordEntryViewModelBase, bool> isValidForFormFunction = null, bool displayInGrid = true, bool autosearch = true, bool cacheAsStaticList = false)
            : this(getAutocompleteStringsFunction, typeof(AutocompleteOption), nameof(AutocompleteOption.Value), new[] { new GridFieldMetadata(nameof(AutocompleteOption.Value), gridWidth ?? 450) }, isValidForFormFunction: isValidForFormFunction, displayInGrid: displayInGrid, autosearch: autosearch)
        {
            CacheAsStaticList = cacheAsStaticList;
        }

        public AutocompleteFunction(Func<RecordEntryViewModelBase, IEnumerable<object>> getAutocompleteStringsFunction, Type objectType, string valueField, IEnumerable<GridFieldMetadata> gridFields, string sortField = null, Func<RecordEntryViewModelBase, bool> isValidForFormFunction = null, bool displayInGrid = true, bool autosearch = true)
        {
            DisplayInGrid = displayInGrid;
            Autosearch = autosearch;
            GetAutocompleteStringsFunction = getAutocompleteStringsFunction;
            ValueField = valueField;
            GridFields = gridFields;
            SortField = sortField ?? valueField;
            SearchFields = GridFields.Select(g => g.FieldName);
            RecordType = objectType.AssemblyQualifiedName;
            IsValidForFormFunction = isValidForFormFunction;
        }

        public bool CacheAsStaticList { get; set; }
        public IEnumerable<string> SearchFields { get; set; }
        public IEnumerable<GridFieldMetadata> GridFields { get; }
        public string SortField { get; }
        public bool DisplayInGrid { get; set; }
        public bool Autosearch { get; }
        public bool DisplayNames { get; }
        private Func<RecordEntryViewModelBase, IEnumerable<object>> GetAutocompleteStringsFunction { get; set; }
        public string ValueField { get; }
        public string RecordType { get; }

        public IEnumerable<object> GetAutocompleteStrings(RecordEntryViewModelBase entryViewModel)
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
