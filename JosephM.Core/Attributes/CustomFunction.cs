using System;

namespace JosephM.Core.Attributes
{
    [AttributeUsage(
        AttributeTargets.Property,
        AllowMultiple = false)]
    public class CustomFunction : Attribute
    {
        public string FunctionName { get; set; }

        public CustomFunction(string functionName)
        {
            FunctionName = functionName;
        }
    }
}