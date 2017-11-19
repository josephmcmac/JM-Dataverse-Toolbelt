using JosephM.Core.Attributes;
using JosephM.Core.Service;

namespace JosephM.CodeGenerator.CSharp
{
    public class CSharpResponse : ServiceResponseBase<ServiceResponseItem>
    {
        [Hidden]
        public string Folder { get; set; }
        [Hidden]
        public string FileName { get; set; }
        [Hidden]
        public string CSharpCode { get; set; }
    }
}