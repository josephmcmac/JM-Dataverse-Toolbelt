using JosephM.Core.Attributes;

namespace JosephM.Application.ViewModel.RecordEntry.Form
{
    public class AutocompleteOption
    {
        private readonly string _name;

        public AutocompleteOption(string value)
        {
            Value = value;
        }

        public AutocompleteOption(string name, string value)
        {
            _name = name;
            Value = value;
        }

        [GridWidth(150)]
        public string Name => _name;

        [GridWidth(450)]
        public string Value { get; set; }
    }
}
