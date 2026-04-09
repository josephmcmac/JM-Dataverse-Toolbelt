using JosephM.Application.Desktop.Module.ServiceRequest;
using JosephM.Application.ViewModel.Dialog;
using JosephM.Application.ViewModel.Extentions;
using JosephM.Application.ViewModel.Grid;
using JosephM.Core.Attributes;
using JosephM.Record.Service;
using JosephM.Record.Xrm.XrmRecord;
using JosephM.SavedViewsExport.CloneSavedViews;
using JosephM.SavedViewsExport.ShareSavedViews;
using System.Linq;

namespace JosephM.SavedViewsExport
{
    [MyDescription("Export personal saved views")]
    public class SavedViewsExportModule :
        ServiceRequestModule
            <SavedViewsExportDialog, SavedViewsExportService, SavedViewsExportRequest,
                SavedViewsExportResponse, SavedViewsExportResponseItem>
    {
        public override string MenuGroup => "Customisations";

        public override void RegisterTypes()
        {
            base.RegisterTypes();
        }

        public override void InitialiseModule()
        {
            base.InitialiseModule();
            AddCloneFunctionToSavedViewsGrid();
            AddShareFunctionToSavedViewsGrid();
        }

        private void AddCloneFunctionToSavedViewsGrid()
        {
            var cloneFunction = new CustomGridFunction("CLONESAVEDVIEWS", "Clone Saved Views", new[]
            {
                new CustomGridFunction("CLONESAVEDVIEWSSELECTED", "Selected Only", (g) =>
                {
                    TriggerCloneSavedViews(g, true);
                }, (g) => g.SelectedRows.Any()),
                new CustomGridFunction("CLONESAVEDVIEWSALL", "All Results", (g) =>
                {
                    TriggerCloneSavedViews(g, false);
                }, (g) => g.GridRecords != null && g.GridRecords.Any())
            });
            this.AddCustomGridFunction(cloneFunction, typeof(SavedView));
        }

        private void TriggerCloneSavedViews(DynamicGridViewModel dynamicGridViewModel, bool selectedOnly)
        {
            ApplicationController.DoOnAsyncThread(() =>
            {
                var recordsToUpdate = selectedOnly
                ? dynamicGridViewModel.SelectedRows.Select(gr => gr.Record).ToArray()
                : dynamicGridViewModel.GetGridRecords(true).Records.ToArray();

                var savedViewObjects = recordsToUpdate
                .Cast<ObjectRecord>()
                .Select(r => r.Instance)
                .Cast<SavedView>()
                .ToArray();
                var xrmRecordService = savedViewObjects.First().GetSourceConnection();
                var request = new CloneSavedViewsRequest(savedViewObjects);
                var dialog = new CloneSavedViewsDialog(xrmRecordService, (IDialogController)ApplicationController.ResolveType(typeof(IDialogController)), request, dynamicGridViewModel.ParentForm.ClearChildForm);
                dynamicGridViewModel.ParentForm.LoadChildForm(dialog);
            });
        }

        private void AddShareFunctionToSavedViewsGrid()
        {
            var cloneFunction = new CustomGridFunction("SHARESAVEDVIEWS", "Share Saved Views", new[]
            {
                new CustomGridFunction("SHARESAVEDVIEWSSELECTED", "Selected Only", (g) =>
                {
                    TriggerShareSavedViews(g, true);
                }, (g) => g.SelectedRows.Any()),
                new CustomGridFunction("SHARESAVEDVIEWSALL", "All Results", (g) =>
                {
                    TriggerShareSavedViews(g, false);
                }, (g) => g.GridRecords != null && g.GridRecords.Any())
            });
            this.AddCustomGridFunction(cloneFunction, typeof(SavedView));
        }

        private void TriggerShareSavedViews(DynamicGridViewModel dynamicGridViewModel, bool selectedOnly)
        {
            ApplicationController.DoOnAsyncThread(() =>
            {
                var recordsToUpdate = selectedOnly
                ? dynamicGridViewModel.SelectedRows.Select(gr => gr.Record).ToArray()
                : dynamicGridViewModel.GetGridRecords(true).Records.ToArray();

                var savedViewObjects = recordsToUpdate
                .Cast<ObjectRecord>()
                .Select(r => r.Instance)
                .Cast<SavedView>()
                .ToArray();
                var xrmRecordService = savedViewObjects.First().GetSourceConnection();
                var request = new ShareSavedViewsRequest(savedViewObjects);
                var dialog = new ShareSavedViewsDialog(xrmRecordService, (IDialogController)ApplicationController.ResolveType(typeof(IDialogController)), request, dynamicGridViewModel.ParentForm.ClearChildForm);
                dynamicGridViewModel.ParentForm.LoadChildForm(dialog);
            });
        }
    }
}