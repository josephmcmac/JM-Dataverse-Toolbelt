using System;

namespace JosephM.Application.ViewModel.Grid
{
    public class CustomGridFunction
    {
        public string Label { get; set; }
        public Action Function { get; set; }

        public CustomGridFunction(string label, Action function)
        {
            Function = function;
            Label = label;
        }
    }
}
