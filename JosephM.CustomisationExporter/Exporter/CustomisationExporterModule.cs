using JosephM.Prism.Infrastructure.Module;

namespace JosephM.CustomisationExporter.Exporter
{
    public class CustomisationExporterModule :
        ServiceRequestModule
            <CustomisationExporterDialog, CustomisationExporterService, CustomisationExporterRequest,
                CustomisationExporterResponse, CustomisationExporterResponseItem>
    {
    }
}