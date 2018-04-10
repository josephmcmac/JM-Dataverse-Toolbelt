using JosephM.Application.Prism.Module.ServiceRequest;
using JosephM.Core.Attributes;

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
    }
}