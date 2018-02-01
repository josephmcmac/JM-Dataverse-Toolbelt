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
using JosephM.ObjectMapping;

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
            [DoNotAllowDelete]
            [DoNotAllowAdd]
            public IEnumerable<SelectablePicklistOption> Options { get; set; }
        }

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
                    //used a child object entry form which worked well enough
                    var mapper = new ClassSelfMapper();
                    var dialogObject = new SelectablePicklistOptions()
                    {
                        Options = ItemsSource.Select(i => mapper.Map(i)).ToArray()
                    };
                    Action onSave = () => {
                        //copy into the
                        mainFormInContext.LoadingViewModel.IsLoading = true;
                        try
                        {
                            ItemsSource = dialogObject.Options;
                            RefreshSelectedItemsIntoValue();
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

                    var recordService = new ObjectRecordService(dialogObject, ApplicationController);
                    var formController = FormController.CreateForObject(dialogObject, ApplicationController, null);
                    var childForm = new ObjectEntryViewModel(onSave, () => mainFormInContext.ClearChildForm(), dialogObject, formController);
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

        public class SelectablePicklistOption
        {
            public SelectablePicklistOption()
            {
                    
            }            

            public SelectablePicklistOption(T item, Action onSelectionChanged, bool isSelected)
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

    public interface IMultiSelectFieldViewModel
    {
    }
}