using System;
using System.Collections.Generic;
using System.Linq;

namespace JosephM.Application.ViewModel.Grid
{
    public class CustomGridFunction
    {
        public string Id { get; set; }
        public string Label { get; set; }
        public Action<DynamicGridViewModel> Function { get; set; }
        public Func<DynamicGridViewModel, bool> VisibleFunction { get; set; }
        public IEnumerable<CustomGridFunction> ChildGridFunctions { get; set; }

        public CustomGridFunction(string id, string label, Action function)
            : this(id, label, (g) => { function(); })
        {
        }

        public CustomGridFunction(string id, string label, Action<DynamicGridViewModel> function, Func<DynamicGridViewModel, bool> visibleFunction = null)
        {
            Id = id;
            Function = function;
            Label = label;
            VisibleFunction = (g) => { return true; };
            if (visibleFunction != null)
                VisibleFunction = visibleFunction;
            ChildGridFunctions = new CustomGridFunction[0];
        }

        public CustomGridFunction(string id, string label, IEnumerable<CustomGridFunction> childGridFunctions)
        {
            Id = id;
            Label = label;
            Function = (g) => { };
            ChildGridFunctions = childGridFunctions;
            VisibleFunction = (g) => { return ChildGridFunctions != null && ChildGridFunctions.Any(c => c.VisibleFunction(g)); };
        }
    }
}
