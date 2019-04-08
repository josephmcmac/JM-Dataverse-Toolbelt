using System;
using System.Collections.Generic;
using System.Linq;

namespace JosephM.Application.ViewModel.RecordEntry.Form
{
    public class CustomFormFunction
    {
        public string Id { get; set; }
        public Func<RecordEntryFormViewModel, string> LabelFunc { get; set; }
        public string Description { get; set; }
        public Action<RecordEntryFormViewModel> Function { get; set; }
        public Func<RecordEntryFormViewModel, bool> VisibleFunction { get; set; }
        public Func<RecordEntryFormViewModel, IEnumerable<CustomFormFunction>> GetChildFormFunctions { get; set; }

        public CustomFormFunction(string id, string label, Action<RecordEntryFormViewModel> function, Func<RecordEntryFormViewModel, bool> visibleFunction = null, string description = null)
        {
            Id = id;
            Function = function;
            LabelFunc = (r) => label;
            VisibleFunction = (r) => { return true; };
            if (visibleFunction != null)
                VisibleFunction = visibleFunction;
            GetChildFormFunctions = (r) => new CustomFormFunction[0];
            Description = description;
        }

        public CustomFormFunction(string id, Func<RecordEntryFormViewModel, string> getLabel, Action<RecordEntryFormViewModel> function, Func<RecordEntryFormViewModel, bool> visibleFunction = null, string description = null)
        {
            Id = id;
            Function = function;
            LabelFunc = getLabel;
            VisibleFunction = (r) => { return true; };
            if (visibleFunction != null)
                VisibleFunction = visibleFunction;
            GetChildFormFunctions = (r) => new CustomFormFunction[0];
            Description = description;
        }

        public CustomFormFunction(string id, string label, Func<RecordEntryFormViewModel, IEnumerable<CustomFormFunction>> getChildFormFunctions)
        {
            Id = id;
            LabelFunc = (r) => label;
            Function = (r) => { };
            GetChildFormFunctions = getChildFormFunctions;
            VisibleFunction = (r) => { return GetChildFormFunctions != null && GetChildFormFunctions(r).Any(c => c.VisibleFunction(r)); };
        }
    }
}
