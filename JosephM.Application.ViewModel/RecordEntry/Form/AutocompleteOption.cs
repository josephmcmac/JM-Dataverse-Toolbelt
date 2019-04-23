using JosephM.Core.Attributes;

namespace JosephM.Application.ViewModel.RecordEntry.Form
{
    public class AutocompleteOption
    {
        public AutocompleteOption(string value)
        {
            Value = value;
        }

        public string Value { get; set; }
    }
}
