using JosephM.Core.Attributes;
using JosephM.Core.Service;

namespace JosephM.CodeGenerator.Service
{
    public class CodeGeneratorResponse : ServiceResponseBase<CodeGeneratorResponseItem>
    {
        [Hidden]
        public string Folder { get; set; }
        [Hidden]
        public string FileName { get; set; }
        [Hidden]
        public string Javascript { get; set; }
    }
}