using System.Collections.Generic;

namespace JosephM.Application.ViewModel.RecordEntry.Form
{
    public class AutocompleteFunctions
    {
        private Dictionary<string, AutocompleteFunction> _functions = new Dictionary<string, AutocompleteFunction>();
        public void AddAutocompleteFunction(string propertyName, AutocompleteFunction autocompleteFunction)
        {
            if (_functions.ContainsKey(propertyName))
                _functions[propertyName] = autocompleteFunction;
            else
                _functions.Add(propertyName, autocompleteFunction);
        }

        public AutocompleteFunction GetAutocompleteFunction(string propertyName)
        {
            return _functions.ContainsKey(propertyName)
                ? _functions[propertyName]
                : null;
        }
    }
}
