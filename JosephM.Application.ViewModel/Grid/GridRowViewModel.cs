using JosephM.Application.ViewModel.RecordEntry.Field;
using JosephM.Application.ViewModel.RecordEntry.Form;
using JosephM.Application.ViewModel.RecordEntry.Metadata;
using JosephM.Application.ViewModel.Shared;
using JosephM.Application.ViewModel.Validation;
using JosephM.Record.Extentions;
using JosephM.Record.IService;
using JosephM.Record.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;

namespace JosephM.Application.ViewModel.Grid
{
    /// <summary>
    ///     Row In A IDynamicGridViewModel Which May Also Interface To Have Data Entered Into A Record
    /// </summary>
    public class GridRowViewModel : RecordEntryViewModelBase
    {
        public GridRowViewModel(IRecord record, DynamicGridViewModel gridViewModel, bool isReadOnly = false)
            : base(gridViewModel.FormController, gridViewModel.OnlyValidate)
        {
            IsReadOnly = isReadOnly;
            Record = record;
            GridViewModel = gridViewModel;
            LoadFields();
            DeleteRowViewModel = new XrmButtonViewModel("Remove", DeleteRow, ApplicationController, description: "Remove This Item");
            EditRowViewModel = new XrmButtonViewModel("Open", EditRow, ApplicationController, description: "Open This Item");
            EditRowNewTabViewModel = new XrmButtonViewModel("Open In New Tab", EditRowNewTab, ApplicationController, description: "Open This Item In New Tab");
            EditRowNewWindowViewModel = new XrmButtonViewModel("Open In New Window", EditRowNewTab, ApplicationController, description: "Open This Item In New Window");
            OpenWebViewModel = new XrmButtonViewModel("Open In Browser", OpenWeb, ApplicationController, description: "Open This Item In Browser");
        }

        public override HorizontalJustify GetHorizontalJustify(RecordFieldType fieldType)
        {
            return GridViewModel == null ? HorizontalJustify.Left : GridViewModel.GetHorizontalJustify(fieldType);
        }

        public DynamicGridViewModel GridViewModel { get; private set; }

        private string RecordType
        {
            get { return Record.Type; }
        }

        public bool IsSelected { get; set; }

        public bool IsVisible
        {
            get
            {
                return _isVisible;
            }
            set
            {
                _isVisible = value;
                OnPropertyChanged(nameof(IsVisible));
            }
        }


        private void LoadFields()
        {
            foreach (var field in GridFields)
            {
                try
                {
                    var viewModel = field.CreateFieldViewModel(RecordType, RecordService, this, ApplicationController);
                    if (field.AliasedFieldName == null)
                    {
                        var isWriteable = RecordService?.GetFieldMetadata(field.FieldName, RecordType).Createable == true
                            || RecordService?.GetFieldMetadata(field.FieldName, RecordType).Writeable == true;

                        viewModel.IsEditable = !IsReadOnly
                            && isWriteable
                            && FormService != null
                            && FormService.AllowGridFieldEditEdit(ParentFormReference)
                            && (!(viewModel is LookupFieldViewModel) || FormService.AllowLookupFunctions);
                    }
                    AddField(viewModel);
                }
                catch (Exception ex)
                {
                    ApplicationController.ThrowException(ex);
                }
            }
        }

        public void EditRow()
        {
            GridViewModel.EditRow(this);
        }

        public void EditRowNewTab()
        {
            GridViewModel.EditRowNew(this);
        }

        public void OpenWeb()
        {
            var url = RecordService.GetWebUrl(RecordType, Record.Id);
            ApplicationController.StartProcess(url);
        }

        public bool DisplayContextMenu { get { return CanEdit || CanEditNewTab || CanEditNewWindow || CanDelete || CanOpenWeb; } }

        public bool CanEdit { get { return GridViewModel.CanEdit; } }

        public bool CanEditNewTab { get { return GridViewModel.CanEditNewTab; } }

        public bool CanEditNewWindow { get { return GridViewModel.CanEditNewWindow; } }

        public bool CanDelete { get { return GridViewModel.CanDelete; } }

        public bool CanOpenWeb
        {
            get
            {
                return RecordService.GetRecordTypeMetadata(RecordType).Searchable
                    && RecordService.GetWebUrl(RecordType, Record.Id) != null;
            }
        }

        public void DeleteRow()
        {
            if (GridViewModel.SelectedRows != null)
            {
                foreach (var item in GridViewModel.SelectedRows.ToArray())
                    if (item != this)
                        GridViewModel.DeleteRow(item);
            }
            GridViewModel.DeleteRow(this);
        }

        public XrmButtonViewModel DeleteRowViewModel { get; set; }

        public XrmButtonViewModel EditRowViewModel { get; set; }
        public XrmButtonViewModel EditRowNewTabViewModel { get; private set; }
        public XrmButtonViewModel EditRowNewWindowViewModel { get; private set; }
        public XrmButtonViewModel OpenWebViewModel { get; private set; }
        public IRecord Record { get; set; }

        public IEnumerable<GridFieldMetadata> GridFields
        {
            get { return GridViewModel.FieldMetadata; }
        }

        private readonly List<FieldViewModelBase> _gridFields = new List<FieldViewModelBase>();
        private bool _isVisible = true;

        public FieldViewModelBase this[string indexFieldName]
        {
            get
            {
                if (_gridFields.Any(gr => gr.IndexFieldName == indexFieldName))
                    return _gridFields.First(gr => gr.IndexFieldName == indexFieldName);
                return null;
            }
            set { throw new NotImplementedException(); }
        }

        public void AddField(FieldViewModelBase gridField)
        {
            _gridFields.Add(gridField);
        }

        public override IEnumerable<FieldViewModelBase> FieldViewModels
        {
            get { return _gridFields; }
        }

        public override RecordEntryViewModelBase ParentForm
        {
            get
            {
                return GridViewModel.ParentForm;
            }
        }

        internal override string ParentFormReference
        {
            get
            {
                return GridViewModel.ReferenceName;
            }
        }

        public static IEnumerable<GridRowViewModel> CreateGridRows(IEnumerable<IRecord> records,
            DynamicGridViewModel gridVm, bool isReadOnly = false)
        {
            var gridRows = new List<GridRowViewModel>();
            foreach (var record in records)
            {
                gridRows.Add(new GridRowViewModel(record, gridVm, isReadOnly: isReadOnly));
            }
            return gridRows;
        }

        public override IRecord GetRecord()
        {
            return Record;
        }

        public override Action<FieldViewModelBase> GetOnFieldChangeDelegate()
        {
            return f =>
            {
                if (FormService != null)
                {
                    foreach (var action in FormService.GetOnChanges(f.FieldName, RecordType, this))
                    {
                        try
                        {
                            action(this);
                        }
                        catch (Exception ex)
                        {
                            ApplicationController.ThrowException(ex);
                        }
                    }
                }
            };
        }

        protected internal override IEnumerable<ValidationRuleBase> GetValidationRules(string fieldName)
        {
            return FormService.GetSubgridValidationRules(fieldName, RecordType);
        }

        public override void LoadChildForm(object viewModel)
        {
            ParentForm.LoadChildForm(viewModel);
        }

        public override void ClearChildForm()
        {
            ParentForm.ClearChildForm();
        }
    }
}