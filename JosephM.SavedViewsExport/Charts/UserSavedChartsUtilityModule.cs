using JosephM.Application.Desktop.Module.ServiceRequest;
using JosephM.Application.ViewModel.Dialog;
using JosephM.Application.ViewModel.Extentions;
using JosephM.Application.ViewModel.Grid;
using JosephM.Core.Attributes;
using JosephM.Record.Service;
using JosephM.UserSavedObjectsUtility.Charts.Clone;
using JosephM.UserSavedObjectsUtility.Charts.Share;
using System.Linq;

namespace JosephM.UserSavedObjectsUtility.Charts
{
    [MyDescription("Export, clone, and share, user saved charts")]
    public class UserSavedChartsUtilityModule :
        ServiceRequestModule
            <UserSavedChartsUtilityDialog, UserSavedChartsUtilityService, UserSavedChartsUtilityRequest,
                UserSavedChartsUtilityResponse, UserSavedChartsUtilityResponseItem>
    {
        public override string MenuGroup => "Customisations";

        public override string MainOperationName => "Saved Chart Utility";

        public override void RegisterTypes()
        {
            base.RegisterTypes();
        }

        public override void InitialiseModule()
        {
            base.InitialiseModule();
            AddCloneFunctionToSavedSavedChartsUtilityGrid();
            AddShareFunctionToSavedSavedChartsUtilityGrid();
        }

        private void AddCloneFunctionToSavedSavedChartsUtilityGrid()
        {
            var cloneFunction = new CustomGridFunction("CLONESAVEDCHART", "Clone Saved Chart", new[]
            {
                new CustomGridFunction("CLONESAVEDCHARTSELECTED", "Selected Only", (g) =>
                {
                    TriggerCloneSavedSavedChartsUtility(g, true);
                }, (g) => g.SelectedRows.Any()),
                new CustomGridFunction("CLONESAVEDCHARTALL", "All Results", (g) =>
                {
                    TriggerCloneSavedSavedChartsUtility(g, false);
                }, (g) => g.GridRecords != null && g.GridRecords.Any())
            });
            this.AddCustomGridFunction(cloneFunction, typeof(SavedChart));
        }

        private void TriggerCloneSavedSavedChartsUtility(DynamicGridViewModel dynamicGridViewModel, bool selectedOnly)
        {
            ApplicationController.DoOnAsyncThread(() =>
            {
                var recordsToUpdate = selectedOnly
                ? dynamicGridViewModel.SelectedRows.Select(gr => gr.Record).ToArray()
                : dynamicGridViewModel.GetGridRecords(true).Records.ToArray();

                var savedViewObjects = recordsToUpdate
                .Cast<ObjectRecord>()
                .Select(r => r.Instance)
                .Cast<SavedChart>()
                .ToArray();
                var xrmRecordService = savedViewObjects.First().GetSourceConnection();
                var request = new CloneSavedChartsRequest(savedViewObjects);
                var dialog = new CloneSavedChartsDialog(xrmRecordService, (IDialogController)ApplicationController.ResolveType(typeof(IDialogController)), request, dynamicGridViewModel.ParentForm.ClearChildForm);
                dynamicGridViewModel.ParentForm.LoadChildForm(dialog);
            });
        }

        private void AddShareFunctionToSavedSavedChartsUtilityGrid()
        {
            var cloneFunction = new CustomGridFunction("SHARESAVEDCHART", "Share Saved Chart", new[]
            {
                new CustomGridFunction("SHARESAVEDCHARTSELECTED", "Selected Only", (g) =>
                {
                    TriggerShareSavedSavedChartsUtility(g, true);
                }, (g) => g.SelectedRows.Any()),
                new CustomGridFunction("SHARESAVEDCHARTALL", "All Results", (g) =>
                {
                    TriggerShareSavedSavedChartsUtility(g, false);
                }, (g) => g.GridRecords != null && g.GridRecords.Any())
            });
            this.AddCustomGridFunction(cloneFunction, typeof(SavedChart));
        }

        private void TriggerShareSavedSavedChartsUtility(DynamicGridViewModel dynamicGridViewModel, bool selectedOnly)
        {
            ApplicationController.DoOnAsyncThread(() =>
            {
                var recordsToUpdate = selectedOnly
                ? dynamicGridViewModel.SelectedRows.Select(gr => gr.Record).ToArray()
                : dynamicGridViewModel.GetGridRecords(true).Records.ToArray();

                var savedViewObjects = recordsToUpdate
                .Cast<ObjectRecord>()
                .Select(r => r.Instance)
                .Cast<SavedChart>()
                .ToArray();
                var xrmRecordService = savedViewObjects.First().GetSourceConnection();
                var request = new ShareSavedChartRequest(savedViewObjects);
                var dialog = new ShareSavedChartDialog(xrmRecordService, (IDialogController)ApplicationController.ResolveType(typeof(IDialogController)), request, dynamicGridViewModel.ParentForm.ClearChildForm);
                dynamicGridViewModel.ParentForm.LoadChildForm(dialog);
            });
        }
    }
}