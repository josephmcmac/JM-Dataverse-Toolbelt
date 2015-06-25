#region

using System.Collections.Generic;
using JosephM.Core.FieldType;
using JosephM.Record.Application.RecordEntry.Form;

#endregion

namespace JosephM.Record.Application.RecordEntry.Field
{
    public class RecordFieldFieldViewModel : FieldViewModel<RecordField>
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
                OnPropertyChanged("Value");
            }
        }

        private IEnumerable<PicklistOption> _itemsSource;

        public IEnumerable<PicklistOption> ItemsSource
        {
            get { return _itemsSource; }
            set
            {
                _itemsSource = value;
                OnPropertyChanged("ItemsSource");
            }
        }

        private string _recordTypeForField;

        public string RecordTypeForField
        {
            get { return _recordTypeForField; }
            set
            {
                _recordTypeForField = value;
                ItemsSource = GetRecordService().GetPicklistKeyValues(FieldName, GetRecordType(), _recordTypeForField, RecordEntryViewModel.GetRecord());
            }
        }
    }
}