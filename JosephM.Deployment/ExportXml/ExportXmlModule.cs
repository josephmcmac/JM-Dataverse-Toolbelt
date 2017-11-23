#region

using JosephM.Core.Attributes;
using JosephM.Prism.Infrastructure.Module;

#endregion

namespace JosephM.Deployment.ExportXml
{
    [MyDescription("Export A Set Of Records Into XML Files")]
    public class ExportXmlModule
        : ServiceRequestModule<ExportXmlDialog, ExportXmlService, ExportXmlRequest, ExportXmlResponse, ExportXmlResponseItem>
    {
        public override string MenuGroup => "Deployment";
    }
}