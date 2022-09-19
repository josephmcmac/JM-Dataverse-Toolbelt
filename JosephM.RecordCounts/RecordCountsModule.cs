﻿using JosephM.Application.Desktop.Module.ServiceRequest;
using JosephM.Application.ViewModel.Dialog;
using JosephM.Application.ViewModel.Extentions;
using JosephM.Application.ViewModel.Grid;
using JosephM.Core.Attributes;
using JosephM.Core.AppConfig;
using JosephM.Record.Service;
using JosephM.Record.Xrm.XrmRecord;
using JosephM.Xrm;
using JosephM.XrmModule.Crud;
using JosephM.XrmModule.SavedXrmConnections;
using System.Linq;

namespace JosephM.RecordCounts
{
    [MyDescription("Get total number of records by type")]
    public class RecordCountsModule :
        ServiceRequestModule
            <RecordCountsDialog, RecordCountsService, RecordCountsRequest,
                RecordCountsResponse, RecordCountsResponseItem>
    {
        public override string MenuGroup => "Text Search & Record Counts";

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
                        var xrmRecordService = new XrmRecordService(instance, ApplicationController.ResolveType<IOrganizationConnectionFactory>(), formService: new XrmFormService());
                        var xrmTextSearchService = new RecordCountsService(xrmRecordService);
                        var dialog = new RecordCountsDialog(xrmTextSearchService, new DialogController(ApplicationController), xrmRecordService)
                        {
                            LoadedFromConnection = true
                        };
                        dialog.SetTabLabel(instance.Name + " " + dialog.TabLabel);
                        g.ApplicationController.NavigateTo(dialog);
                    }
                }
            }, (g) => g.GridRecords != null && g.GridRecords.Any());
            this.AddCustomGridFunction(customGridFunction, typeof(SavedXrmRecordConfiguration));
        }
    }
}