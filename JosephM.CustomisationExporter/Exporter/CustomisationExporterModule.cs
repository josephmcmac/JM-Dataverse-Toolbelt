using JosephM.Application.Desktop.Module.ServiceRequest;
using JosephM.Application.ViewModel.Dialog;
using JosephM.Application.ViewModel.Extentions;
using JosephM.Application.ViewModel.Grid;
using JosephM.Application.ViewModel.RecordEntry.Form;
using JosephM.Core.Attributes;
using JosephM.Record.Service;
using JosephM.Record.Xrm.XrmRecord;
using JosephM.XrmModule.Crud;
using JosephM.XrmModule.SavedXrmConnections;
using System.Linq;

namespace JosephM.CustomisationExporter.Exporter
{
    [MyDescription("Export Customisations In A CRM Instance Into CSV Files")]
    public class CustomisationExporterModule :
        ServiceRequestModule
            <CustomisationExporterDialog, CustomisationExporterService, CustomisationExporterRequest,
                CustomisationExporterResponse, CustomisationExporterResponseItem>
    {
        public override string MainOperationName { get { return "Export"; } }

        public override string MenuGroup => "Customisations";

        public override void RegisterTypes()
        {
            base.RegisterTypes();
            AddExportButtonToSavedConnectionsGrid();
            AddDialogCompletionLinks();
        }

        private void AddDialogCompletionLinks()
        {
            this.AddCustomFormFunction(new CustomFormFunction("OPENTYPES", "Open Types"
                , (r) => r.ApplicationController.StartProcess(r.GetRecord().GetStringField(nameof(CustomisationExporterResponse.TypesFileNameQualified)))
                , (r) => !string.IsNullOrWhiteSpace(r.GetRecord().GetStringField(nameof(CustomisationExporterResponse.TypesFileName))))
                , typeof(CustomisationExporterResponse));
            this.AddCustomFormFunction(new CustomFormFunction("OPENFIELDS", "Open Fields"
                , (r) => r.ApplicationController.StartProcess(r.GetRecord().GetStringField(nameof(CustomisationExporterResponse.FieldsFileNameQualified)))
                , (r) => !string.IsNullOrWhiteSpace(r.GetRecord().GetStringField(nameof(CustomisationExporterResponse.FieldsFileName))))
                , typeof(CustomisationExporterResponse));
            this.AddCustomFormFunction(new CustomFormFunction("OPENRELATIONSHIPS", "Open Relationships"
                , (r) => r.ApplicationController.StartProcess(r.GetRecord().GetStringField(nameof(CustomisationExporterResponse.RelationshipsFileNameQualified)))
                , (r) => !string.IsNullOrWhiteSpace(r.GetRecord().GetStringField(nameof(CustomisationExporterResponse.RelationshipsFileName))))
                , typeof(CustomisationExporterResponse));
            this.AddCustomFormFunction(new CustomFormFunction("OPENOPTIONS", "Open Options"
                , (r) => r.ApplicationController.StartProcess(r.GetRecord().GetStringField(nameof(CustomisationExporterResponse.OptionSetsFileNameQualified)))
                , (r) => !string.IsNullOrWhiteSpace(r.GetRecord().GetStringField(nameof(CustomisationExporterResponse.OptionSetsFileName))))
                , typeof(CustomisationExporterResponse));
            this.AddCustomFormFunction(new CustomFormFunction("OPENFOLDER", "Open Folder"
                , (r) => r.ApplicationController.StartProcess(r.GetRecord().GetStringField(nameof(CustomisationExporterResponse.Folder)))
                , (r) => !string.IsNullOrWhiteSpace(r.GetRecord().GetStringField(nameof(CustomisationExporterResponse.Folder))))
                , typeof(CustomisationExporterResponse));
        }

        private void AddExportButtonToSavedConnectionsGrid()
        {
            var customGridFunction = new CustomGridFunction("CUSTOMISATIONEXPORT", "Customisation Export", (g) =>
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
                        var xrmTextSearchService = new CustomisationExporterService(xrmRecordService);
                        var dialog = new CustomisationExporterDialog(xrmTextSearchService, new DialogController(ApplicationController), xrmRecordService);
                        dialog.SetTabLabel(instance.Name + " " + dialog.TabLabel);
                        g.ApplicationController.NavigateTo(dialog);
                    }
                }
            }, (g) => g.GridRecords != null && g.GridRecords.Any());
            this.AddCustomGridFunction(customGridFunction, typeof(SavedXrmRecordConfiguration));
        }
    }
}