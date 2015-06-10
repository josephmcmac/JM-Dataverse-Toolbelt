using JosephM.Core.FieldType;
using JosephM.Record.Application.RecordEntry.Form;

namespace JosephM.Record.Application.RecordEntry.Field
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