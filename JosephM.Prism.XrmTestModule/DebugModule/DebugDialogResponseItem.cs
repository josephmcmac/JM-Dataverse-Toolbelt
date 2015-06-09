using System;
using JosephM.Core.Service;

namespace JosephM.Prism.XrmTestModule.DebugModule
{
    public class DebugDialogResponseItem : ServiceResponseItem
    {
        public string Type { get; set; }

        public DebugDialogResponseItem(string type, Exception ex)
        {
            Type = type;
            Exception = ex;
        }
    }
}