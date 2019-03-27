using JosephM.Application.Desktop.Module.Crud.BulkReplace;
using JosephM.Application.ViewModel.Dialog;
using JosephM.Application.ViewModel.Extentions;
using JosephM.Application.ViewModel.Grid;
using JosephM.Application.ViewModel.Query;
using JosephM.Application.ViewModel.RecordEntry;
using JosephM.Application.ViewModel.RecordEntry.Form;
using JosephM.Application.ViewModel.RecordEntry.Metadata;
using JosephM.Core.FieldType;
using JosephM.Record.Extentions;
using JosephM.Record.IService;
using JosephM.Record.Metadata;
using JosephM.Record.Query;
using System;
using System.Collections.Generic;
using System.Linq;

namespace JosephM.Xrm.RecordExtract.TextSearch
{
    public class EditResultsDialog : DialogViewModel
    {
        public IRecordService RecordService { get { return SummaryItem.GetRecordService(); } }

        private TextSearchResponse.SummaryItem SummaryItem { get; set; }
        public Action Remove { get; }
        public DynamicGridViewModel DynamicGridViewModel { get; private set; }
        public List<GridFieldMetadata> ExplicitlySelectedColumns { get; private set; }

        public EditResultsDialog(IDialogController dialogController, TextSearchResponse.SummaryItem summaryItem, Action remove)
            : base(dialogController)
        {
            SummaryItem = summaryItem;
            Remove = remove;
        }

        protected override void CompleteDialogExtention()
        {
        }

        protected override void LoadDialogExtention()
        {
            RecreateGrid();
        }

        private void RecreateGrid()
        {
            if (DynamicGridViewModel != null)
                Controller.RemoveFromUi(DynamicGridViewModel);
            try
            {
                //okay lets somehow load the results for this row into a crud dialog view
                //which loads a dynamic grid - similar to query but no query - 
                DynamicGridViewModel = new DynamicGridViewModel(ApplicationController)
                {
                    PageSize = 25,
                    RecordService = RecordService,
                    RecordType = SummaryItem.RecordTypeSchemaName,
                    IsReadOnly = true,
                    FormController = new FormController(RecordService, null, ApplicationController),
                    GetGridRecords = (b) =>
                    {
                        return DynamicGridViewModel.GetGridRecord(GetAllTheseRecords(), b);
                    },
                    DisplayTotalCount = true,
                    GetTotalCount = () => GetAllTheseRecords().Count(),
                    MultiSelect = true,
                    GridLoaded = false,
                    FieldMetadata = ExplicitlySelectedColumns
                };
                var customFunctionList = new List<CustomGridFunction>()
                {
                    new CustomGridFunction("BACKTOSUMMARY", "Back To Summary", Remove),
                    new CustomGridFunction("EDITCOLUMNS", "Edit Columns", (g) => LoadColumnEdit(), (g) => DynamicGridViewModel != null),
                    new CustomGridFunction("CSV", "Download CSV", (g) => g.DownloadCsv(), (g) => g.GridRecords != null && g.GridRecords.Any()),
                    new CustomGridFunction("REPLACE", "Bulk Replace", new []
                        {
                            new CustomGridFunction("BULKREPLACESELECTED", "Selected Only", (g) =>
                            {
                                TriggerBulkReplace(true);
                            }, (g) => g.SelectedRows.Any()),
                            new CustomGridFunction("BULKREPLACEALL", "All Results", (g) =>
                            {
                                TriggerBulkReplace(false);
                            }, (g) => g.GridRecords != null && g.GridRecords.Any()),
                        })
                };

                var formService = RecordService.GetFormService() as FormServiceBase;
                if (formService != null)
                {
                    DynamicGridViewModel.EditRow = (g) =>
                    {
                        var formMetadata = formService.GetFormMetadata(SummaryItem.RecordTypeSchemaName, RecordService);
                        var formController = new FormController(RecordService, formService, ApplicationController);
                        var selectedRow = g;
                        if (selectedRow != null)
                        {
                            Action onSave = () =>
                            {
                                ClearChildForm();
                                _cachedRecords = null;
                                DynamicGridViewModel.ReloadGrid();
                            };
                            var record = selectedRow.Record;

                            var newForm = new CreateOrUpdateViewModel(RecordService.Get(record.Type, record.Id), formController, onSave, ClearChildForm);
                            LoadChildForm(newForm);
                        }
                    };
                }
                DynamicGridViewModel.AddGridButtons(customFunctionList);
                DynamicGridViewModel.ReloadGrid();
                Controller.LoadToUi(DynamicGridViewModel);
            }
            catch (Exception ex)
            {
                ApplicationController.ThrowException(ex);
            }
        }

        private IEnumerable<IRecord> _cachedRecords;
        private IEnumerable<IRecord> GetAllTheseRecords()
        {
            if(_cachedRecords == null)
            {
                _cachedRecords = RecordService
                .RetrieveAllOrClauses(SummaryItem.RecordTypeSchemaName, SummaryItem
                    .GetIds()
                    .Select(id => new Condition(RecordService.GetPrimaryKey(SummaryItem.RecordTypeSchemaName), ConditionType.Equal, id)).ToArray(), null);
            }
            return _cachedRecords;
        }

        private void TriggerBulkReplace(bool selectedOnly)
        {
            var recordsToUpdate = GetAllTheseRecords();
            if (selectedOnly)
            {
                var selctedIds = DynamicGridViewModel.SelectedRows.Select(gr => gr.GetRecord().Id).ToArray();
                recordsToUpdate = recordsToUpdate.Where(r => selctedIds.Contains(r.Id)).ToArray();
            }
            var request = new BulkReplaceRequest(new RecordType(SummaryItem.RecordTypeSchemaName, SummaryItem.RecordType), recordsToUpdate);
            var bulkUpdateDialog = new BulkReplaceDialog(RecordService, (IDialogController)ApplicationController.ResolveType(typeof(IDialogController)), request, () => { ClearChildForms(); _cachedRecords = null; DynamicGridViewModel.ReloadGrid(); });
            LoadChildForm(bulkUpdateDialog);
        }

        private void LoadColumnEdit()
        {
            DoOnMainThread(() =>
            {
                //okay lets spawn a dialog for editing the columns in the grid
                var currentColumns = DynamicGridViewModel.FieldMetadata
                    .OrderBy(f => f.Order)
                    .Select(f => new KeyValuePair<string, double>(f.FieldName, f.WidthPart))
                    .ToArray();

                Action<IEnumerable<ColumnEditDialogViewModel.SelectableColumn>> letsLoadTheColumns = (newColumnSet) =>
                {
                    DoOnMainThread(() =>
                    {
                        ExplicitlySelectedColumns = new List<GridFieldMetadata>();
                        for (var i = 1; i <= newColumnSet.Count(); i++)
                        {
                            var thisOne = newColumnSet.ElementAt(i - 1);
                            ExplicitlySelectedColumns.Add(new GridFieldMetadata(new ViewField(thisOne.FieldName, i, Convert.ToInt32(thisOne.Width))));
                        }
                        ClearChildForm();
                        RecreateGrid();
                    });
                };

                var columnEditDialog = new ColumnEditDialogViewModel(SummaryItem.RecordTypeSchemaName, currentColumns, RecordService, letsLoadTheColumns, ClearChildForm, ApplicationController);
                LoadChildForm(columnEditDialog);
            });
        }
    }
}
