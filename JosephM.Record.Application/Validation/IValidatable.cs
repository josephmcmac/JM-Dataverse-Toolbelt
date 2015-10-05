#region

using JosephM.Application.ViewModel.RecordEntry.Form;

#endregion

namespace JosephM.Application.ViewModel.Validation
{
    public interface IValidatable
    {
        RecordEntryViewModelBase GetRecordForm();

        string ReferenceName { get; }
    }
}