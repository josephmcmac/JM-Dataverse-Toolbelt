using JosephM.Core.Attributes;
using JosephM.Core.Service;

namespace JosephM.CodeGenerator.JavaScriptOptions
{
    public class JavaScriptOptionsResponse : ServiceResponseBase<ServiceResponseItem>
    {
        [Multiline]
        public string Javascript { get; set; }
    }
}