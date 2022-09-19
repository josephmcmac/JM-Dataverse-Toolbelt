using JosephM.Core.Attributes;
using JosephM.Core.Service;

namespace JosephM.Xrm.DataImportExport.XmlImport
{
    public class ExportXmlResponse : ServiceResponseBase<ExportXmlResponseItem>
    {
        [Hidden]
        public string Folder { get; internal set; }
    }
}