using JosephM.Application.ViewModel.Grid;
using JosephM.Application.ViewModel.RecordEntry.Form;
using JosephM.Application.ViewModel.Shared;
using JosephM.Core.FieldType;
using System;
using System.Collections.Generic;
using System.Linq;

namespace JosephM.Application.ViewModel.RecordEntry.Field
{
    public abstract class MultiSelectFieldViewModel<T>
        : FieldViewModel<IEnumerable<T>>, IMultiSelectFieldViewModel
        where T : PicklistOption
    {
        protected MultiSelectFieldViewModel(string fieldName, string label, RecordEntryViewModelBase recordForm)
            : base(fieldName, label, recordForm)
        {
            EditAction = LoadMultiSelectDialog;
        }

        private Action _editAction;
        public Action EditAction
        {
            get
            {
                return _editAction;
            }
            set
            {
                _editAction = value;
                if (_editAction != null)
                    EditButton = new XrmButtonViewModel("New", () => { try { _editAction(); } catch (Exception ex) { ApplicationController.ThrowException(ex); } }, ApplicationController);
                OnPropertyChanged(nameof(EditAction));
            }
        }

        private XrmButtonViewModel _editButton;
        public XrmButtonViewModel EditButton
        {
            get { return _editButton; }
            set
            {
                _editButton = value;
                OnPropertyChanged(nameof(EditButton));
            }
        }

        public void RefreshSelectedItemsIntoValue(IEnumerable<T> selectedOptions)
        {
            Value = selectedOptions?.ToArray() ?? new T[0];
            OnPropertyChanged(nameof(StringDisplay));
        }

        private IEnumerable<T> _itemsSource;

        public IEnumerable<T> ItemsSource
        {
            get { return _itemsSource; }
            set
            {
                _itemsSource = value;
                OnPropertyChanged(nameof(ItemsSource));
            }
        }

        public override string StringDisplay
        {
            get { return Value == null ? null : string.Join(",", Value.OrderBy(p => p.Value).Select(p => p.Value)); }
        }

        internal void SetItemsSource(IEnumerable<T> items)
        {
            ItemsSource = items;
        }

        public DynamicGridViewModel DynamicGridViewModel { get; set; }

        public void LoadMultiSelectDialog()
        {
            RecordEntryViewModel.DoOnMainThread(() =>
            {
                try
                {
                    //previously this was a popup grid for selection - but seemed to affect performance
                    //with heap of rows each with heap of bindings in the popup grid
                    //so changed to a child dialog when selected

                    var mainFormInContext = RecordEntryViewModel;
                    if (RecordEntryViewModel is GridRowViewModel)
                        mainFormInContext = RecordEntryViewModel.ParentForm;

                    //okay i need to load a dialog
                    //displaying a grid of the selectable options with a checkbox
                    Action<IEnumerable<T>> onSave = (selectedOptions) =>
                    {
                        //copy into the
                        mainFormInContext.LoadingViewModel.IsLoading = true;
                        try
                        {
                            RefreshSelectedItemsIntoValue(selectedOptions);
                            mainFormInContext.ClearChildForm();
                        }
                        catch (Exception ex)
                        {
                            RecordEntryViewModel.ApplicationController.ThrowException(ex);
                        }
                        finally
                        {
                            mainFormInContext.LoadingViewModel.IsLoading = false;
                        }
                    };

                    var childForm = new MultiSelectDialogViewModel<T>(ItemsSource, Value, onSave, () => mainFormInContext.ClearChildForm(), ApplicationController);
                    mainFormInContext.LoadChildForm(childForm);
                }
                catch (Exception ex)
                {
                    RecordEntryViewModel.ApplicationController.ThrowException(ex);
                }
                finally
                {
                    RecordEntryViewModel.LoadingViewModel.IsLoading = false;
                }
            });
        }
    }

    public interface IMultiSelectFieldViewModel
    {
    }
}