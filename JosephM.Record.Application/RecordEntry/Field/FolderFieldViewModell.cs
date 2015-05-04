using JosephM.Core.FieldType;
using JosephM.Record.Application.RecordEntry.Form;

namespace JosephM.Record.Application.RecordEntry.Field
{
    public class FolderFieldViewModel : FieldViewModel<Folder>
    {
        public FolderFieldViewModel(string fieldName, string label, RecordEntryViewModelBase recordForm)
            : base(fieldName, label, recordForm)
        {
        }
    }
}