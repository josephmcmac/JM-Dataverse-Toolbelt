using JosephM.Application.Desktop.Module.ServiceRequest;
using JosephM.Application.ViewModel.Dialog;
using JosephM.Application.ViewModel.Extentions;
using JosephM.Application.ViewModel.Grid;
using JosephM.Record.Service;
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
            AddWebBrowseGridFunction();
        }

        private void AddWebBrowseGridFunction()
        {
            var customGridFunction = new CustomGridFunction("LOADTOGRID", "Load Matches To Grid", (DynamicGridViewModel g) =>
            {
                if (g.SelectedRows.Count() != 1)
                    g.ApplicationController.UserMessage("1 Row Must Be Selected For Loading Records");
                else
                {
                    var selectedItem = g.SelectedRow.GetRecord() as ObjectRecord;
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
    }
}