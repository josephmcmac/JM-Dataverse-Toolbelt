using System;
using JosephM.Record.Application.RecordEntry;
using JosephM.Record.Application.RecordEntry.Form;

namespace JosephM.Prism.TestModule.SearchModule
{
    public class SearchEntryViewModel : ObjectEntryViewModel
    {
        public SearchEntryViewModel(Action onSave, Action onCancel, object objectToEnter, FormController formController)
            : base(onSave, onCancel, objectToEnter, formController)
        {
        }

        public override string SaveButtonLabel
        {
            get { return "Search"; }
        }
    }
}
