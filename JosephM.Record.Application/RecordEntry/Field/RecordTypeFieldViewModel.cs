#region

using System.Collections.Generic;
using JosephM.Core.FieldType;
using JosephM.Record.Application.RecordEntry.Form;

#endregion

namespace JosephM.Record.Application.RecordEntry.Field
{
    public class RecordTypeFieldViewModel : FieldViewModel<RecordType>
    {
        public RecordTypeFieldViewModel(string fieldName, string label, RecordEntryViewModelBase recordForm)
            : base(fieldName, label, recordForm)
        {
        }

        public override RecordType Value
        {
            get { return ValueObject as RecordType; }
            set
            {
                ValueObject = value;
                OnPropertyChanged("Value");
            }
        }

        private IEnumerable<RecordType> _itemsSource;

        public IEnumerable<RecordType> ItemsSource
        {
            get { return _itemsSource; }
            set
            {
                _itemsSource = value;
                OnPropertyChanged("ItemsSource");
            }
        }
    }
}