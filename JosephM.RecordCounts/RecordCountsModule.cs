using JosephM.Application.Desktop.Module.ServiceRequest;
using JosephM.Application.ViewModel.Dialog;
using JosephM.Application.ViewModel.Extentions;
using JosephM.Application.ViewModel.Grid;
using JosephM.Core.Attributes;
using JosephM.Record.Service;
using JosephM.Record.Xrm.XrmRecord;
using JosephM.XrmModule.Crud;
using JosephM.XrmModule.SavedXrmConnections;
using System.Linq;

namespace JosephM.RecordCounts
{
    [MyDescription("Generate Counts Of Records In The CRM Instance Either Globally Or Per User/Owner")]
    public class RecordCountsModule :
        ServiceRequestModule
            <RecordCountsDialog, RecordCountsService, RecordCountsRequest,
                RecordCountsResponse, RecordCountsResponseItem>
    {
        public override string MenuGroup => "Reports";

        public override void RegisterTypes()
        {
            base.RegisterTypes();
            AddRecordCountButtonToSavedConnectionsGrid();
        }

        private void AddRecordCountButtonToSavedConnectionsGrid()
        {
            var customGridFunction = new CustomGridFunction("RECORDCOUNT", "Record Counts", (g) =>
            {
                if (g.SelectedRows.Count() != 1)
                {
                    g.ApplicationController.UserMessage("Please Select One Row To Run This Function");
                }
                else
                {
                    var selectedRow = g.SelectedRows.First();
                    var instance = ((ObjectRecord)selectedRow.Record).Instance as SavedXrmRecordConfiguration;
                    if (instance != null)
                    {
                        var xrmRecordService = new XrmRecordService(instance, formService: new XrmFormService());
                        var xrmTextSearchService = new RecordCountsService(xrmRecordService);
                        var dialog = new RecordCountsDialog(xrmTextSearchService, new DialogController(ApplicationController), xrmRecordService);
                        dialog.SetTabLabel(instance.Name + " " + dialog.TabLabel);
                        g.ApplicationController.NavigateTo(dialog);
                    }
                }
            }, (g) => g.GridRecords != null && g.GridRecords.Any());
            this.AddCustomGridFunction(customGridFunction, typeof(SavedXrmRecordConfiguration));
        }
    }
}