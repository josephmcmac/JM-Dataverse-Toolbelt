using JosephM.Application.ViewModel.RecordEntry.Form;
using JosephM.Core.FieldType;
using System.Collections.Generic;
using System.Linq;

namespace JosephM.Application.ViewModel.RecordEntry.Field
{
    public abstract class DropdownFieldViewModel<T>
        : FieldViewModel<T>
        where T : PicklistOption
    {
        protected DropdownFieldViewModel(string fieldName, string label, RecordEntryViewModelBase recordForm)
            : base(fieldName, label, recordForm)
        {
        }

        protected virtual void OnItemsLoaded()
        {

        }

        private IEnumerable<T> _itemsSource;

        public IEnumerable<T> ItemsSource
        {
            get { return _itemsSource; }
            set
            {
                _itemsSource = value;
                if (Value != null && _itemsSource != null)
                {
                    var matchingItems = _itemsSource.Where(p => p.Key == Value.Key);
                    if (matchingItems.Any())
                        Value.Value = matchingItems.First().Value ?? Value.Value;
                    if (!_itemsSource.Any())
                    {
                        _itemsSource = new[] { Value };
                    }
                }
                if (Value == null && ItemsSource.Count() == 1 && (FormService?.InitialisePicklistIfOneOption(FieldName, GetRecordTypeOfThisField()) ?? false))
                {
                    Value = ItemsSource.First();
                }
                OnPropertyChanged(nameof(ItemsSource));
            }
        }
    }
}