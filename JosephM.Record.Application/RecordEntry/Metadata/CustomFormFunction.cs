using JosephM.Application.ViewModel.RecordEntry.Form;
using System;
using System.Collections.Generic;
using System.Linq;

namespace JosephM.Application.ViewModel.RecordEntry.Metadata
{
    public class CustomFormFunction
    {
        public string Id { get; set; }
        public string Label { get; set; }
        public Action<RecordEntryViewModelBase> Function { get; set; }
        public IEnumerable<CustomFormFunction> ChildFunctions { get; set; }

        public CustomFormFunction(string id, string label, Action<RecordEntryViewModelBase> function)
        {
            Id = id;
            Function = function;
            Label = label;
            ChildFunctions = new CustomFormFunction[0];
        }

        public CustomFormFunction(string id, string label, IEnumerable<CustomFormFunction> childFunctions)
        {
            Id = id;
            Label = label;
            Function = (re) => { };
            ChildFunctions = childFunctions;
        }
    }
}
