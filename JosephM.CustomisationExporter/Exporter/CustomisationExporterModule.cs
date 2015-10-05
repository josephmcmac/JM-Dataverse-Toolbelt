using JosephM.Prism.Infrastructure.Module;

namespace JosephM.CustomisationExporter.Exporter
{
    public class CustomisationExporterModule :
        ServiceRequestModule
            <CustomisationExporterDialog, CustomisationExporterService, CustomisationExporterRequest,
                CustomisationExporterResponse, CustomisationExporterResponseItem>
    {
        public override void InitialiseModule()
        {
            base.InitialiseModule();
            AddHelp("Export Customisations", "Customisation Exporter Help.htm");
        }
    }
}