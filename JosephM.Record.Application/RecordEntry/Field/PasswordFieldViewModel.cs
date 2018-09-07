using System.ComponentModel.DataAnnotations;
using JosephM.Application.ViewModel.RecordEntry.Form;
using JosephM.Core.Extentions;
using JosephM.Core.FieldType;

namespace JosephM.Application.ViewModel.RecordEntry.Field
{
    public class PasswordFieldViewModel : FieldViewModel<Password>
    {
        public PasswordFieldViewModel(string fieldName, string label, RecordEntryViewModelBase recordForm)
            : base(fieldName, label, recordForm)
        {
        }

        public int? MaxLength { get; set; }

        [DataType(DataType.Password)]
        public override Password Value
        {
            get { return base.Value; }
            set { base.Value = value; }
        }

        public string ReadOnlyStars
        {
            get { return Value == null
                    ? null
                    : "*".ReplicateString(Value.GetRawPassword()?.Length ?? 0); }
        }
    }
}