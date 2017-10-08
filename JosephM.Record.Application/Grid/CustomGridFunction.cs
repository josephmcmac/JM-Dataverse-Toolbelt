using System;

namespace JosephM.Application.ViewModel.Grid
{
    public class CustomGridFunction
    {
        public string Label { get; set; }
        public Action<DynamicGridViewModel> Function { get; set; }
        public Func<DynamicGridViewModel, bool> VisibleFunction { get; set; }

        public CustomGridFunction(string label, Action function)
            : this(label, (g) => { function(); })
        {
        }

        public CustomGridFunction(string label, Action<DynamicGridViewModel> function, Func<DynamicGridViewModel, bool> visibleFunction = null)
        {
            Function = function;
            Label = label;
            VisibleFunction = (g) => { return true; };
            if (visibleFunction != null)
                VisibleFunction = visibleFunction;
        }
    }
}
