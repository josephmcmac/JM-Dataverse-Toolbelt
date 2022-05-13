using System.Linq;
using JosephM.Application.ViewModel.RecordEntry.Form;
using JosephM.Core.FieldType;

namespace JosephM.Application.ViewModel.RecordEntry.Field
{
    public class RecordFieldFieldViewModel : DropdownFieldViewModel<RecordField>
    {
        public RecordFieldFieldViewModel(string fieldName, string label, RecordEntryViewModelBase recordForm)
            : base(fieldName, label, recordForm)
        {
        }

        public override RecordField Value
        {
            get { return ValueObject as RecordField; }
            set
            {
                ValueObject = value;
                OnPropertyChanged(nameof(Value));
            }
        }
        private string _recordTypeForField;

        public string RecordTypeForField
        {
            get { return _recordTypeForField; }
            set
            {
                _recordTypeForField = value;
                var reference = GetRecordForm().ParentFormReference == null
                    ? _recordTypeForField
                    : _recordTypeForField + ":" + GetRecordForm().ParentFormReference;

                DoOnAsynchThread(() =>
                {
                    LoadingViewModel.IsLoading = true;
                    try
                    {
                        ItemsSource = GetRecordService()
                            .GetPicklistKeyValues(FieldName, GetRecordTypeOfThisField(), reference, RecordEntryViewModel.GetRecord())
                            .Select(p => new RecordField(p.Key, p.Value));
                    }
                    finally
                    {
                        LoadingViewModel.IsLoading = false;
                    }
                });
            }
        }
    }
}