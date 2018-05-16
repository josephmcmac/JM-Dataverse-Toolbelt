using JosephM.Application.Desktop.Module.ServiceRequest;
using JosephM.Application.ViewModel.Dialog;
using JosephM.Application.ViewModel.Extentions;
using JosephM.Application.ViewModel.Grid;
using JosephM.Record.Service;
using JosephM.Record.Xrm.XrmRecord;
using JosephM.XrmModule.Crud;
using JosephM.XrmModule.SavedXrmConnections;
using System;
using System.Linq;

namespace JosephM.Xrm.RecordExtract.TextSearch
{
    public abstract class TextSearchModuleBase<TTextSearchDialog, TTextSearchService> :
        ServiceRequestModule
            <TTextSearchDialog, TTextSearchService, TextSearchRequest, TextSearchResponse, TextSearchResponseItem>
        where TTextSearchService : TextSearchService
        where TTextSearchDialog : TextSearchDialogBase<TTextSearchService>
    {
        public override void RegisterTypes()
        {
            base.RegisterTypes();
            AddLoadResultItemToGridFunction();
            AddTextSearchButtonToSavedConnectionsGrid();
        }

        private void AddLoadResultItemToGridFunction()
        {
            //adds a button the search results summary
            //to load the matches for a particular type and/or field to a grid for editing
            var customGridFunction = new CustomGridFunction("LOADTOGRID", "Load Matches To Grid", (DynamicGridViewModel g) =>
            {
                if (g.SelectedRows.Count() != 1)
                    g.ApplicationController.UserMessage("1 Row Must Be Selected For Loading Records");
                else
                {
                    var selectedItem = g.SelectedRows.First().GetRecord() as ObjectRecord;
                    if(selectedItem == null)
                        throw new Exception($"Error selected item is not of type {typeof(ObjectRecord).Name}");
                    var summaryItem = selectedItem.Instance as TextSearchResponse.SummaryItem;
                    if (summaryItem == null)
                        throw new Exception($"Error selected item is not of type {typeof(TextSearchResponse.SummaryItem).Name}");

                    var dialogController = new DialogController(g.ApplicationController);
                    var dialog = new EditResultsDialog(dialogController, summaryItem, g.RemoveParentDialog);
                    g.LoadDialog(dialog);
                }
            }, (g) => g.GridRecords != null);
            this.AddCustomGridFunction(customGridFunction, typeof(TextSearchResponse.SummaryItem));
        }

        private void AddTextSearchButtonToSavedConnectionsGrid()
        {
            var customGridFunction = new CustomGridFunction("TEXTSEARCH", "Text Search Selected", (g) =>
            {
                if (g.SelectedRows.Count() != 1)
                {
                    g.ApplicationController.UserMessage("Please Select One Row To Search The Connection");
                }
                else
                {
                    var selectedRow = g.SelectedRows.First();
                    var instance = ((ObjectRecord)selectedRow.Record).Instance as SavedXrmRecordConfiguration;
                    if (instance != null)
                    {
                        var xrmRecordService = new XrmRecordService(instance, formService: new XrmFormService());
                        var xrmTextSearchService = new XrmTextSearchService(xrmRecordService, new DocumentWriter.DocumentWriter());
                        var dialog = new XrmTextSearchDialog(xrmTextSearchService, new DialogController(ApplicationController), xrmRecordService);
                        dialog.SetTabLabel("Text Search " + instance.Name);
                        g.LoadDialog(dialog);
                    }
                }
            }, (g) => g.GridRecords != null && g.GridRecords.Any());
            this.AddCustomGridFunction(customGridFunction, typeof(SavedXrmRecordConfiguration));
        }
    }
}