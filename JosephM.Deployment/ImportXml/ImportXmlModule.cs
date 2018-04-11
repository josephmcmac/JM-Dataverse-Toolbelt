using JosephM.Application.Desktop.Module.ServiceRequest;
using JosephM.Core.Attributes;


namespace JosephM.Deployment.ImportXml
{
    [MyDescription("Import Records Which Have Been Exported As XML Files Into A CRM Instance")]
    public class ImportXmlModule
        : ServiceRequestModule<ImportXmlDialog, ImportXmlService, ImportXmlRequest, ImportXmlResponse, DataImportResponseItem>
    {
        public override string MenuGroup => "Deployment";
    }
}