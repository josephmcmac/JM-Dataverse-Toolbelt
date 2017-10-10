#region

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using JosephM.Application.ViewModel.RecordEntry.Field;
using JosephM.Application.ViewModel.RecordEntry.Form;
using JosephM.Application.ViewModel.RecordEntry.Metadata;
using JosephM.Application.ViewModel.Shared;
using JosephM.Application.ViewModel.Validation;
using JosephM.Record.IService;

#endregion

namespace JosephM.Application.ViewModel.Grid
{
    /// <summary>
    ///     Row In A IDynamicGridViewModel Which May Also Interface To Have Data Entered Into A Record
    /// </summary>
    public class GridRowViewModel : RecordEntryViewModelBase
    {
        public GridRowViewModel(IRecord record, DynamicGridViewModel gridViewModel)
            : base(gridViewModel.FormController, gridViewModel.OnlyValidate)
        {
            Record = record;
            GridViewModel = gridViewModel;
            LoadFields();
            DeleteRowViewModel = new XrmButtonViewModel("Delete", DeleteRow, ApplicationController);
            EditRowViewModel = new XrmButtonViewModel("Edit", EditRow, ApplicationController);
        }

        public DynamicGridViewModel GridViewModel { get; private set; }

        private string RecordType
        {
            get { return Record.Type; }
        }

        public bool IsSelected { get; set; }

        private void LoadFields()
        {
            foreach (var field in GridFields)
            {
                var viewModel = field.CreateFieldViewModel(RecordType, RecordService, this, ApplicationController);
                viewModel.IsEditable = !GridViewModel.IsReadOnly;
                AddField(viewModel);
            }
        }

        public void EditRow()
        {
            GridViewModel.EditRow(this);
        }

        public void DeleteRow()
        {
            GridViewModel.DeleteRow(this);
        }

        public XrmButtonViewModel DeleteRowViewModel { get; set; }

        public XrmButtonViewModel EditRowViewModel { get; set; }

        public IRecord Record { get; set; }

        public IEnumerable<GridFieldMetadata> GridFields
        {
            get { return GridViewModel.RecordFields; }
        }

        private readonly List<FieldViewModelBase> _gridFields = new List<FieldViewModelBase>();

        public FieldViewModelBase this[string fieldName]
        {
            get
            {
                if (_gridFields.Any(gr => gr.FieldName == fieldName))
                    return _gridFields.First(gr => gr.FieldName == fieldName);
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

        internal override RecordEntryViewModelBase ParentForm
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

        public static ObservableCollection<GridRowViewModel> LoadRows(IEnumerable<IRecord> records,
            DynamicGridViewModel gridVm)
        {
            var gridRows = new ObservableCollection<GridRowViewModel>();

            foreach (var record in records)
            {
                gridRows.Add(new GridRowViewModel(record, gridVm));
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
                    foreach (var action in FormService.GetOnChanges(f.FieldName, RecordType))
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
    }
}