using JosephM.Application.Desktop.Module.ServiceRequest;
using JosephM.Application.ViewModel.Dialog;
using JosephM.Application.ViewModel.Extentions;
using JosephM.Application.ViewModel.Grid;
using JosephM.Core.Attributes;
using JosephM.Record.Service;
using JosephM.UserSavedObjectsUtility.Dashboards.Clone;
using JosephM.UserSavedObjectsUtility.Dashboards.Share;
using System.Linq;

namespace JosephM.UserSavedObjectsUtility.Dashboards
{
    [MyDescription("Export, clone, and share, user saved dashboards")]
    public class UserSavedDashboardsUtilityModule :
        ServiceRequestModule
            <UserSavedDashboardsUtilityDialog, UserSavedDashboardsUtilityService, UserSavedDashboardsUtilityRequest,
                UserSavedDashboardsUtilityResponse, UserSavedDashboardsUtilityResponseItem>
    {
        public override string MenuGroup => "Customisations";

        public override string MainOperationName => "Saved Dashboard Utility";

        public override void RegisterTypes()
        {
            base.RegisterTypes();
        }

        public override void InitialiseModule()
        {
            base.InitialiseModule();
            AddCloneFunctionToSavedSavedDashboardsUtilityGrid();
            AddShareFunctionToSavedSavedDashboardsUtilityGrid();
        }

        private void AddCloneFunctionToSavedSavedDashboardsUtilityGrid()
        {
            var cloneFunction = new CustomGridFunction("CLONESAVEDDASHBOARD", "Clone Saved Dashboard", new[]
            {
                new CustomGridFunction("CLONESAVEDDASHBOARDSELECTED", "Selected Only", (g) =>
                {
                    TriggerCloneSavedSavedDashboardsUtility(g, true);
                }, (g) => g.SelectedRows.Any()),
                new CustomGridFunction("CLONESAVEDDASHBOARDALL", "All Results", (g) =>
                {
                    TriggerCloneSavedSavedDashboardsUtility(g, false);
                }, (g) => g.GridRecords != null && g.GridRecords.Any())
            });
            this.AddCustomGridFunction(cloneFunction, typeof(SavedDashboard));
        }

        private void TriggerCloneSavedSavedDashboardsUtility(DynamicGridViewModel dynamicGridViewModel, bool selectedOnly)
        {
            ApplicationController.DoOnAsyncThread(() =>
            {
                var recordsToUpdate = selectedOnly
                ? dynamicGridViewModel.SelectedRows.Select(gr => gr.Record).ToArray()
                : dynamicGridViewModel.GetGridRecords(true).Records.ToArray();

                var savedViewObjects = recordsToUpdate
                .Cast<ObjectRecord>()
                .Select(r => r.Instance)
                .Cast<SavedDashboard>()
                .ToArray();
                var xrmRecordService = savedViewObjects.First().GetSourceConnection();
                var request = new CloneSavedDashboardsRequest(savedViewObjects);
                var dialog = new CloneSavedDashboardsDialog(xrmRecordService, (IDialogController)ApplicationController.ResolveType(typeof(IDialogController)), request, dynamicGridViewModel.ParentForm.ClearChildForm);
                dynamicGridViewModel.ParentForm.LoadChildForm(dialog);
            });
        }

        private void AddShareFunctionToSavedSavedDashboardsUtilityGrid()
        {
            var cloneFunction = new CustomGridFunction("SHARESAVEDDASHBOARD", "Share Saved Dashboard", new[]
            {
                new CustomGridFunction("SHARESAVEDDASHBOARDSELECTED", "Selected Only", (g) =>
                {
                    TriggerShareSavedSavedDashboardsUtility(g, true);
                }, (g) => g.SelectedRows.Any()),
                new CustomGridFunction("SHARESAVEDDASHBOARDALL", "All Results", (g) =>
                {
                    TriggerShareSavedSavedDashboardsUtility(g, false);
                }, (g) => g.GridRecords != null && g.GridRecords.Any())
            });
            this.AddCustomGridFunction(cloneFunction, typeof(SavedDashboard));
        }

        private void TriggerShareSavedSavedDashboardsUtility(DynamicGridViewModel dynamicGridViewModel, bool selectedOnly)
        {
            ApplicationController.DoOnAsyncThread(() =>
            {
                var recordsToUpdate = selectedOnly
                ? dynamicGridViewModel.SelectedRows.Select(gr => gr.Record).ToArray()
                : dynamicGridViewModel.GetGridRecords(true).Records.ToArray();

                var savedViewObjects = recordsToUpdate
                .Cast<ObjectRecord>()
                .Select(r => r.Instance)
                .Cast<SavedDashboard>()
                .ToArray();
                var xrmRecordService = savedViewObjects.First().GetSourceConnection();
                var request = new ShareSavedDashboardRequest(savedViewObjects);
                var dialog = new ShareSavedDashboardDialog(xrmRecordService, (IDialogController)ApplicationController.ResolveType(typeof(IDialogController)), request, dynamicGridViewModel.ParentForm.ClearChildForm);
                dynamicGridViewModel.ParentForm.LoadChildForm(dialog);
            });
        }
    }
}