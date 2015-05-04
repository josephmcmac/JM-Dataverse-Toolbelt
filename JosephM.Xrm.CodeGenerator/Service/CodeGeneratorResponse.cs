using JosephM.Core.Service;

namespace JosephM.Xrm.CodeGenerator.Service
{
    public class CodeGeneratorResponse : ServiceResponseBase<CodeGeneratorResponseItem>
    {
        public string Folder { get; set; }
        public string FileName { get; set; }
    }
}