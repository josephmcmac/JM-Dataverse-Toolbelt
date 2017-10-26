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
            AddHelpUrl("Customisation Exporter", "CustomisationExport");
        }

        public override string MenuGroup => "Customisations";
    }
}