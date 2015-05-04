using System;
using JosephM.Core.Service;

namespace JosephM.Prism.TestModule.Prism.TestDialog
{
    public class TestDialogResponseItem : ServiceResponseItem
    {
        public string Type { get; set; }

        public TestDialogResponseItem(string type, Exception ex)
        {
            Type = type;
            Exception = ex;
        }
    }
}