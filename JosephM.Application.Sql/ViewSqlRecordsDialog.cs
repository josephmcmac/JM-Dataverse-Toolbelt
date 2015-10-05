using System;
using System.Collections.Generic;
using JosephM.Application.ViewModel.Dialog;
using JosephM.Application.ViewModel.Grid;
using JosephM.Record.Sql;

namespace JosephM.Migration.Prism.Module.Sql
{
    public abstract class ViewSqlRecordsDialog : DialogViewModel
    {
        protected ISqlRecordMetadataService RecordService { get; set; }

        protected ViewSqlRecordsDialog(ISqlRecordMetadataService recordService, IDialogController dialogController)
            : base(dialogController)
        {
            RecordService = recordService;
        }

        protected override void LoadDialogExtention()
        {
            LoadSearchResults();
        }

        public ListViewModel ListViewModel { get; set; }

        public void LoadSearchResults()
        {
            LoadingViewModel.IsLoading = true;
            DoOnAsynchThread(() =>
            {
                try
                {
                    ListViewModel = new ListViewModel(RecordType, RecordService, ApplicationController,
                        CreateEditDialog == null ? (Action<GridRowViewModel>)null : OnEdit,
                        CreateEditDialog == null ? (Action)null : OnAdd, CustomFunctions);
                    Controller.LoadToUi(ListViewModel);
                }
                finally
                {
                    LoadingViewModel.IsLoading = false;
                }
            });
        }

        protected virtual Func<DialogViewModel> CreateAddDialog { get; set; }

        protected virtual Func<string, DialogViewModel> CreateEditDialog { get; set; }

        private void OnAdd()
        {
            var dialog = CreateAddDialog();
            dialog.OverideCompletionScreenMethod = ClearChildFormAndRefresh;
            dialog.OnCancel = ClearChildForm;
            ListViewModel.LoadChildForm(dialog);
        }

        public void OnEdit(GridRowViewModel gridRow)
        {
            var dialog = CreateEditDialog(gridRow.GetRecord().Id);
            dialog.OverideCompletionScreenMethod = ClearChildFormAndRefresh;
            dialog.OnCancel = ClearChildForm;
            ListViewModel.LoadChildForm(dialog);
        }

        protected void ClearChildForm()
        {
            ListViewModel.ClearChildForm();
        }

        protected void ClearChildFormAndRefresh()
        {
            ListViewModel.DynamicGridViewModel.ReloadGrid();
            ClearChildForm();
        }

        protected override void CompleteDialogExtention()
        {
        }

        protected abstract string RecordType { get; }

        public virtual IEnumerable<CustomGridFunction> CustomFunctions
        {
            get { return new CustomGridFunction[0]; }
        }
    }
}