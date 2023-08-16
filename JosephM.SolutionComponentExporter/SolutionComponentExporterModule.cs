using JosephM.Application.Desktop.Module.ServiceRequest;
using JosephM.Application.ViewModel.Extentions;
using JosephM.Application.ViewModel.RecordEntry.Form;
using JosephM.Core.Attributes;

namespace JosephM.SolutionComponentExporter
{
    [MyDescription("Export information about a solutions components into Excel")]
    public class SolutionComponentExporterModule :
        ServiceRequestModule
            <SolutionComponentExporterDialog, SolutionComponentExporterService, SolutionComponentExporterRequest,
                SolutionComponentExporterResponse, SolutionComponentExporterResponseItem>
    {
        public override string MainOperationName { get { return "Export Solution Components"; } }

        public override string MenuGroup => "Customisations";

        public override void RegisterTypes()
        {
            base.RegisterTypes();
            AddDialogCompletionLinks();
        }

        private void AddDialogCompletionLinks()
        {
            this.AddCustomFormFunction(new CustomFormFunction("OPENEXCEL", "Open Excel File"
                , (r) => r.ApplicationController.StartProcess(r.GetRecord().GetStringField(nameof(SolutionComponentExporterResponse.ExcelFileNameQualified)))
                , (r) => !string.IsNullOrWhiteSpace(r.GetRecord().GetStringField(nameof(SolutionComponentExporterResponse.ExcelFileName))))
                , typeof(SolutionComponentExporterResponse));
            this.AddCustomFormFunction(new CustomFormFunction("OPENFOLDER", "Open Folder"
                , (r) => r.ApplicationController.StartProcess(r.GetRecord().GetStringField(nameof(SolutionComponentExporterResponse.Folder)))
                , (r) => !string.IsNullOrWhiteSpace(r.GetRecord().GetStringField(nameof(SolutionComponentExporterResponse.Folder))))
                , typeof(SolutionComponentExporterResponse));
        }
    }
}