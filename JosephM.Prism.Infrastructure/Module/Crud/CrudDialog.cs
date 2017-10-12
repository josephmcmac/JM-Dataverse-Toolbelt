using JosephM.Application.ViewModel.Dialog;
using JosephM.Application.ViewModel.Grid;
using JosephM.Application.ViewModel.RecordEntry;
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
                            var queryViewModel = new QueryViewModel(recordTypesForBrowsing, RecordService, ApplicationController, allowQuery: true);
                            Controller.LoadToUi(queryViewModel);
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
    }
}
