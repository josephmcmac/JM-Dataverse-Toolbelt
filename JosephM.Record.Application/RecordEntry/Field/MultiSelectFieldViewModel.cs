using JosephM.Application.ViewModel.RecordEntry.Form;
using JosephM.Application.ViewModel.Shared;
using JosephM.Core.FieldType;
using System.Collections.Generic;
using System.Linq;
using System;
using JosephM.Core.Attributes;
using JosephM.Application.ViewModel.Grid;
using JosephM.Record.Service;
using JosephM.Record.Query;
using JosephM.Record.Metadata;
using JosephM.Record.IService;

namespace JosephM.Application.ViewModel.RecordEntry.Field
{
    public abstract class MultiSelectFieldViewModel<T>
        : FieldViewModel<IEnumerable<T>>, IMultiSelectFieldViewModel
        where T : PicklistOption
    {
        protected MultiSelectFieldViewModel(string fieldName, string label, RecordEntryViewModelBase recordForm)
            : base(fieldName, label, recordForm)
        {
            MultiSelectButton = new XrmButtonViewModel("MultiSelect", MultiSelect, ApplicationController);
        }

        private void MultiSelect()
        {
            MultiSelectsVisible = true;
        }

        private bool _multiSelectsVisible;

        public bool MultiSelectsVisible
        {
            get { return _multiSelectsVisible; }
            set
            {
                _multiSelectsVisible = value;
                OnPropertyChanged(nameof(MultiSelectsVisible));
            }
        }

        public void RefreshSelectedItemsIntoValue()
        {
            Value = ItemsSource == null ? new T[0] : ItemsSource
                .Where(i => i.Select)
                .Select(i => i.PicklistItem)
                .ToArray();
            OnPropertyChanged(nameof(StringDisplay));
        }


        private IEnumerable<SelectablePicklistOption> _itemsSource;

        public IEnumerable<SelectablePicklistOption> ItemsSource
        {
            get { return _itemsSource; }
            set
            {
                _itemsSource = value;
                OnPropertyChanged(nameof(ItemsSource));
            }
        }

        public string StringDisplay
        {
            get { return Value == null ? null : string.Join(",", Value.OrderBy(p => p.Value).Select(p => p.Value)); }
        }

        public XrmButtonViewModel MultiSelectButton { get; private set; }

        internal void SetItemsSource(IEnumerable<T> items)
        {
            ItemsSource = items == null ? new SelectablePicklistOption[0] : items.Select(i => new SelectablePicklistOption(i, RefreshSelectedItemsIntoValue, Value != null && Value.Any(v => v == i))).ToArray();
            var optionsObject = new SelectablePicklistOptions()
            {
                Options = ItemsSource
            };
            var recordService = new ObjectRecordService(optionsObject, ApplicationController);
            Func<IEnumerable<IRecord>> getRecordsFunc = () => recordService.RetreiveAll(new QueryDefinition(typeof(SelectablePicklistOption).AssemblyQualifiedName));

            DynamicGridViewModel = new DynamicGridViewModel(ApplicationController)
            {
                ViewType = ViewType.LookupView,
                FormController = new FormController(recordService, null, ApplicationController),
                RecordType = typeof(SelectablePicklistOption).AssemblyQualifiedName,
                RecordService = recordService,
                GetGridRecords = (b) => new GetGridRecordsResponse(getRecordsFunc()),
                IsReadOnly = true
            };
            Action onClick = () =>
            {
                var selectedItem = DynamicGridViewModel.SelectedRow;
                if (selectedItem != null)
                {
                    var isSelectedField = selectedItem.GetBooleanFieldFieldViewModel(nameof(SelectablePicklistOption.Select));
                    isSelectedField.Value = !isSelectedField.Value;
                    DynamicGridViewModel.SelectedRow = null;
                }
            };
            DynamicGridViewModel.OnClick = onClick;
            DoOnAsynchThread(() => {
                DynamicGridViewModel.GridRecords = GridRowViewModel.LoadRows(getRecordsFunc(), DynamicGridViewModel);
                OnPropertyChanged(nameof(DynamicGridViewModel));
                OnPropertyChanged(nameof(StringDisplay));
            });
        }

        public DynamicGridViewModel DynamicGridViewModel { get; set; }

        public class SelectablePicklistOptions
        {
            public IEnumerable<SelectablePicklistOption> Options { get; set; }
        }

        public class SelectablePicklistOption
        {
            public SelectablePicklistOption(T item, Action onSelectionChanged, bool isSelected)
            {
                _isselected = isSelected;
                PicklistItem = item;
                OnSelectionChanged = onSelectionChanged;

            }

            private bool _isselected;
            [DisplayOrder(10)]
            [GridWidth(75)]
            public bool Select
            {
                get
                {
                    return _isselected;
                }
                set
                {
                    _isselected = value;
                    OnSelectionChanged();
                }
            }
            [DisplayOrder(20)]
            public string Item { get { return PicklistItem?.Value; } }
            [Hidden]
            public T PicklistItem { get; set; }
            [Hidden]
            public Action OnSelectionChanged { get; private set; }
        }
    }

    public interface IMultiSelectFieldViewModel
    {
        bool MultiSelectsVisible
        {
            get; set;
        }
    }
}