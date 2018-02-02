using JosephM.Application.Application;
using JosephM.Application.ViewModel.Shared;
using JosephM.Application.ViewModel.TabArea;
using JosephM.Core.Attributes;
using JosephM.Core.FieldType;
using System;
using System.Collections.Generic;
using System.Linq;

namespace JosephM.Application.ViewModel.RecordEntry.Field
{
    public class MultiSelectDialogViewModel<T> : TabAreaViewModelBase, IMultiSelectDialog
        where T : PicklistOption
    {
        public MultiSelectDialogViewModel(IEnumerable<T> options, IEnumerable<T> selectedOptions, Action<IEnumerable<T>> onApply, Action onCancel, IApplicationController applicationController)
            : base(applicationController)
        {
            ItemsSource = options == null
                ? new SelectablePicklistOption[0]
                : options
                .Select(i => new SelectablePicklistOption(i, selectedOptions != null && selectedOptions.Any(s => s.Key == i.Key)))
                .ToArray();
            ApplyButtonViewModel = new XrmButtonViewModel("Apply Changes", () => onApply(ItemsSource.Where(i => i.Select).Select(i => i.PicklistItem).ToArray()), ApplicationController, "Apply The Selection Changes");
            CancelButtonViewModel = new XrmButtonViewModel("Cancel Changes", onCancel, ApplicationController, "Cancel The Selection Changes And Return");
        }

        public XrmButtonViewModel ApplyButtonViewModel { get; set; }

        public XrmButtonViewModel CancelButtonViewModel { get; set; }

        public IEnumerable<SelectablePicklistOption> ItemsSource { get; private set; }

        public class SelectablePicklistOption
        {
            public SelectablePicklistOption()
            {

            }

            public SelectablePicklistOption(T item, bool isSelected)
            {
                _isselected = isSelected;
                PicklistItem = item;
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
                }
            }
            [DisplayOrder(20)]
            public string Item { get { return PicklistItem?.Value; } }
            [Hidden]
            public T PicklistItem { get; set; }
        }
    }

    public interface IMultiSelectDialog
    {
    }
}