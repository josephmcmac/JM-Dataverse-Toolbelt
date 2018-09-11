using System;

namespace JosephM.Application.ViewModel.RecordEntry.Form
{
    public class OnChangeFunction
    {
        public OnChangeFunction(Action<RecordEntryViewModelBase, string> function)
        {
            Function = function;
        }

        private Action<RecordEntryViewModelBase, string> Function { get; set; }

        public void Execute(RecordEntryViewModelBase entryViewModel, string changeField)
        {
            Function(entryViewModel, changeField);
        }
    }
}
