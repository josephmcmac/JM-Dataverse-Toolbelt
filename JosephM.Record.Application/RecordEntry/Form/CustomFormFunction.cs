using System;
using System.Collections.Generic;
using System.Linq;

namespace JosephM.Application.ViewModel.RecordEntry.Form
{
    public class CustomFormFunction
    {
        public string Id { get; set; }
        public string Label { get; set; }
        public Action<RecordEntryFormViewModel> Function { get; set; }
        public Func<RecordEntryFormViewModel, bool> VisibleFunction { get; set; }
        public IEnumerable<CustomFormFunction> ChildFormFunctions { get; set; }

        public CustomFormFunction(string id, string label, Action<RecordEntryFormViewModel> function, Func<RecordEntryFormViewModel, bool> visibleFunction = null)
        {
            Id = id;
            Function = function;
            Label = label;
            VisibleFunction = (g) => { return true; };
            if (visibleFunction != null)
                VisibleFunction = visibleFunction;
            ChildFormFunctions = new CustomFormFunction[0];
        }

        public CustomFormFunction(string id, string label, IEnumerable<CustomFormFunction> childFormFunctions)
        {
            Id = id;
            Label = label;
            Function = (g) => { };
            ChildFormFunctions = childFormFunctions;
            VisibleFunction = (g) => { return ChildFormFunctions != null && ChildFormFunctions.Any(c => c.VisibleFunction(g)); };
        }
    }
}
