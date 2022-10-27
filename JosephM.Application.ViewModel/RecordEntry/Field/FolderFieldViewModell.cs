using JosephM.Application.ViewModel.RecordEntry.Form;
using JosephM.Core.FieldType;

namespace JosephM.Application.ViewModel.RecordEntry.Field
{
    public class FolderFieldViewModel : FieldViewModel<Folder>
    {
        private bool _isDragOver;

        public FolderFieldViewModel(string fieldName, string label, RecordEntryViewModelBase recordForm)
            : base(fieldName, label, recordForm)
        {
            EditableFormWidth = 234;
        }

        public bool IsDragOver
        {
            get
            {
                return _isDragOver;
            }

            set
            {
                _isDragOver = value;
                OnPropertyChanged(nameof(IsDragOver));
            }
        }
    }
}