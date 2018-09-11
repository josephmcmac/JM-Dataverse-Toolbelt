using JosephM.Application.Desktop.Module.ServiceRequest;
using JosephM.Core.Attributes;


namespace JosephM.Deployment.ExportXml
{
    [MyDescription("Export A Set Of Records Into XML Files")]
    public class ExportXmlModule
        : ServiceRequestModule<ExportXmlDialog, ExportXmlService, ExportXmlRequest, ExportXmlResponse, ExportXmlResponseItem>
    {
        public override string MenuGroup => "Data Import/Export";
    }
}