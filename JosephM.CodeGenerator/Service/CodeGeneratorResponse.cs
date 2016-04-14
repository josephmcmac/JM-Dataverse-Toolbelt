using JosephM.Core.Service;

namespace JosephM.CodeGenerator.Service
{
    public class CodeGeneratorResponse : ServiceResponseBase<CodeGeneratorResponseItem>
    {
        public string Folder { get; set; }
        public string FileName { get; set; }
        public string Javascript { get; set; }
    }
}