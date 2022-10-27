using JosephM.Application.ViewModel.RecordEntry.Form;
using JosephM.Core.FieldType;

namespace JosephM.Application.ViewModel.RecordEntry.Field
{
    public class FileRefFieldViewModel : FieldViewModel<FileReference>
    {
        private bool _isDragOver;

        public string FileMask { get; set; }

        public FileRefFieldViewModel(string fieldName, string label, RecordEntryViewModelBase recordForm, string fileMask)
            : base(fieldName, label, recordForm)
        {
            EditableFormWidth = 284;
            FileMask = fileMask;
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