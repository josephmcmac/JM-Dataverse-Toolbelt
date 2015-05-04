#region

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using JosephM.Record.Application.RecordEntry.Field;
using JosephM.Record.Application.RecordEntry.Form;
using JosephM.Record.Application.RecordEntry.Metadata;
using JosephM.Record.Application.Shared;
using JosephM.Record.Application.Validation;
using JosephM.Record.IService;

#endregion

namespace JosephM.Record.Application.Grid
{
    /// <summary>
    ///     Row In A IDynamicGridViewModel Which May Also Interface To Have Data Entered Into A Record
    /// </summary>
    public class GridRowViewModel : RecordEntryViewModelBase
    {
        public GridRowViewModel(IRecord record, IDynamicGridViewModel gridViewModel)
            : base(gridViewModel.FormController)
        {
            Record = record;
            GridViewModel = gridViewModel;
            LoadFields();
            DeleteRowViewModel = new XrmButtonViewModel("Delete", DeleteRow, ApplicationController);
        }

        public IDynamicGridViewModel GridViewModel { get; private set; }

        private string RecordType
        {
            get { return Record.Type; }
        }

        private void LoadFields()
        {
            foreach (var field in GridFields)
            {
                var viewModel = field.CreateFieldViewModel(RecordType, RecordService, this, ApplicationController);
                viewModel.IsEditable = !GridViewModel.IsReadOnly;
                AddField(viewModel);
            }
            RunOnChanges();
        }

        public void DeleteRow()
        {
            GridViewModel.DynamicGridViewModelItems.DeleteRow(this);
        }

        public XrmButtonViewModel DeleteRowViewModel { get; set; }

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

        public static ObservableCollection<GridRowViewModel> LoadRows(IEnumerable<IRecord> records,
            IDynamicGridViewModel gridVm)
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
                    //AddChangedField(f);
                    foreach (var action in FormService.GetOnChanges(f.FieldName, RecordType))
                        action(this);
                    //FormInstance.OnChange(f.FieldName, gri);
                }
            };
        }

        protected internal override IEnumerable<ValidationRuleBase> GetValidationRules(string fieldName)
        {
            return FormService.GetValidationRules(fieldName, RecordType);
        }
    }
}