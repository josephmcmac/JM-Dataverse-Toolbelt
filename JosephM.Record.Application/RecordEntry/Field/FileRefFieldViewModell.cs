using JosephM.Application.ViewModel.RecordEntry.Form;
using JosephM.Core.FieldType;

namespace JosephM.Application.ViewModel.RecordEntry.Field
{
    public class FileRefFieldViewModel : FieldViewModel<FileReference>
    {
        public string FileMask { get; set; }

        public FileRefFieldViewModel(string fieldName, string label, RecordEntryViewModelBase recordForm, string fileMask)
            : base(fieldName, label, recordForm)
        {
            FileMask = fileMask;
        }
    }
}