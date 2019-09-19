using JosephM.Application.ViewModel.RecordEntry.Form;
using JosephM.Core.FieldType;
using System.Collections.Generic;
using System.Linq;

namespace JosephM.Application.ViewModel.RecordEntry.Field
{
    public class BooleanFieldViewModel : FieldViewModel<bool?>
    {
        public BooleanFieldViewModel(string fieldName, string label, RecordEntryViewModelBase recordForm, IEnumerable<PicklistOption> picklist)
            : base(fieldName, label, recordForm)
        {
            PicklistOptions = picklist;
            UsePicklist = PicklistOptions != null && PicklistOptions.Any();
        }

        public override bool? Value
        {
            get { return (bool?)ValueObject; }
            set { ValueObject = value; }
        }

        public bool UsePicklist { get; set; }
        public IEnumerable<PicklistOption> PicklistOptions { get; set; }

        public PicklistOption SelectedOption
        {
            get
            {
                if (Value.HasValue)
                {
                    return Value.Value ? TrueOption : FalseOption;
                }
                return null;
            }
            set
            {
                if (value != null)
                {
                    Value = value == TrueOption;
                }
                else
                    Value = null;
                OnPropertyChanged(nameof(SelectedOption));
            }
        }

        private PicklistOption TrueOption
        {
            get
            {
                return UsePicklist && PicklistOptions.Any(p => p.Key == "1")
                    ? PicklistOptions.First(p => p.Key == "1")
                    : null;
            }
        }

        private PicklistOption FalseOption
        {
            get
            {
                return UsePicklist && PicklistOptions.Any(p => p.Key == "0")
                    ? PicklistOptions.First(p => p.Key == "0")
                    : null;
            }
        }

        public override string StringDisplay
        {
            get
            {
                if (!Value.HasValue)
                {
                    return null;
                }
                if(Value.Value)
                {
                    return TrueOption?.Value ?? Value.Value.ToString();
                }
                else
                {
                    return FalseOption?.Value ?? Value.Value.ToString();
                }
            }
        }
    }
}