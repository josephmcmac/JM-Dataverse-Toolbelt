using System;

namespace JosephM.Application.ViewModel.RecordEntry.Form
{
    public class FormLoadedFunction
    {
        public FormLoadedFunction(Action<RecordEntryViewModelBase> function)
        {
            Function = function;
        }

        private Action<RecordEntryViewModelBase> Function { get; set; }

        public void Execute(RecordEntryViewModelBase entryViewModel)
        {
            Function(entryViewModel);
        }
    }
}
