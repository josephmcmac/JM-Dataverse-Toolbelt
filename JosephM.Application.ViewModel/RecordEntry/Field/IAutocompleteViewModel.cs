using JosephM.Application.Application;
using JosephM.Application.ViewModel.Grid;
using JosephM.Application.ViewModel.RecordEntry.Form;

namespace JosephM.Application.ViewModel.RecordEntry.Field
{
    public interface IAutocompleteViewModel
    {
        bool DisplayAutocomplete { get; set; }
        RecordEntryViewModelBase RecordEntryViewModel { get; set; }
        IApplicationController ApplicationController { get; }
        string SearchText { get; set; }
        void SetValue(GridRowViewModel selectedRow);
        void AddError(string v);
    }
}
