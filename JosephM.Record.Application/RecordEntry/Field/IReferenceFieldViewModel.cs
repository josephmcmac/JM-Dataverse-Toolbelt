#region

using System.ComponentModel;
using JosephM.Application.ViewModel.RecordEntry.Form;
using JosephM.Record.IService;

#endregion

namespace JosephM.Application.ViewModel.RecordEntry.Field
{
    public interface IReferenceFieldViewModel : INotifyPropertyChanged
    {
        string RecordTypeToLookup { get; set; }

        IRecordService LookupService { get; }

        RecordEntryViewModelBase RecordEntryViewModel { get; }

        void Search();

        void SelectLookupGrid();

        bool Searching { get; }

        string EnteredText { get; set; }

        bool IsEditable { get; }
    }
}