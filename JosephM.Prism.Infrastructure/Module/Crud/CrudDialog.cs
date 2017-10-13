using JosephM.Application.ViewModel.Dialog;
using JosephM.Application.ViewModel.Grid;
using JosephM.Application.ViewModel.RecordEntry;
using JosephM.Core.FieldType;
using JosephM.Prism.Infrastructure.Module.Crud.BulkUpdate;
using JosephM.Record.Extentions;
using JosephM.Record.IService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace JosephM.Prism.Infrastructure.Module.Crud
{
    public class CrudDialog : DialogViewModel
    {
        public IRecordService RecordService { get; set; }
        public QueryViewModel QueryViewModel { get; private set; }

        public CrudDialog(IDialogController dialogController, IRecordService recordService)
            : base(dialogController)
        {
            RecordService = recordService;
        }

        protected override void CompleteDialogExtention()
        {
        }

        protected override void LoadDialogExtention()
        {
            //todo need a verification script for this
            //including the open, edit and save
            LoadingViewModel.IsLoading = true;
            Thread.Sleep(100);
            //this bit messy because may take a while to load the record types
            //so spawn on async thread, then back to the main thread for th ui objects
            DoOnAsynchThread(() =>
            {
                try
                {
                    var recordTypesForBrowsing = RecordService.GetAllRecordTypes()
                        .Where(r =>
                        RecordService.GetRecordTypeMetadata(r).Searchable)
                        .ToArray();

                    DoOnMainThread(() =>
                    {
                        try
                        {
                            var customFunctionList = new List<CustomGridFunction>()
                            {
                                new CustomGridFunction("BULKUPDATE", "Bulk Update", new []
                                {
                                    new CustomGridFunction("BULKUPDATEALL", "Update All Results", (g) =>
                                    {
                                        TriggerBulkUpdate(false);
                                    }),
                                    new CustomGridFunction("BULKUPDATESELECTED", "Update Selected", (g) =>
                                    {
                                        TriggerBulkUpdate(true);
                                    }, (g) => g.SelectedRows.Any()),
                                })
                            };

                            QueryViewModel = new QueryViewModel(recordTypesForBrowsing, RecordService, ApplicationController, allowQuery: true, customFunctions: customFunctionList);
                            Controller.LoadToUi(QueryViewModel);
                        }
                        catch (Exception ex)
                        {
                            ApplicationController.ThrowException(ex);
                        }
                        finally
                        {
                            LoadingViewModel.IsLoading = false;
                        }
                    });
                }
                catch(Exception ex)
                {
                    ApplicationController.ThrowException(ex);
                    LoadingViewModel.IsLoading = false;
                }

            });
        }

        private void TriggerBulkUpdate(bool selectedOnly)
        {
            var recordType = QueryViewModel.RecordType;
            IEnumerable<string> recordsToUpdate = null;
            if(selectedOnly)
            {
                recordsToUpdate = QueryViewModel.DynamicGridViewModel.SelectedRows.Select(r => r.Record.Id).ToArray();
            }
            else
            {
                var query = QueryViewModel.GenerateQuery();
                query.Fields = new string[0];
                query.Top = -1;
                recordsToUpdate = RecordService.RetreiveAll(query).Select(r => r.Id).ToArray();
            }

            var request = new BulkUpdateRequest(new RecordType(recordType, RecordService.GetDisplayName(recordType)), recordsToUpdate);
            var bulkUpdateDialog = new BulkUpdateDialog(RecordService, (IDialogController)ApplicationController.ResolveType(typeof(IDialogController)), request);
            LoadChildForm(bulkUpdateDialog);

        }
    }
}
