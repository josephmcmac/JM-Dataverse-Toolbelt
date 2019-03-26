using JosephM.Application.Application;
using JosephM.Application.ViewModel.TabArea;
using System.Collections.Generic;

namespace JosephM.Application.ViewModel.RecordEntry.Form
{
    public class RecordEntryAggregatorViewModel : TabAreaViewModelBase
    {
        public RecordEntryAggregatorViewModel(IEnumerable<RecordEntryFormViewModel> entryForms, IApplicationController controller)
            : base(controller)
        {
            EntryForms = entryForms;
        }

        public IEnumerable<RecordEntryFormViewModel> EntryForms { get; }
    }
}