using JosephM.Core.FieldType;
using JosephM.Record.Application.RecordEntry.Form;

namespace JosephM.Record.Application.RecordEntry.Field
{
    //todo this unnecessary now could just use fileviewmodel
    public class ExcelFileFieldViewModel : FieldViewModel<ExcelFile>
    {
        public ExcelFileFieldViewModel(string fieldName, string label, RecordEntryViewModelBase recordForm)
            : base(fieldName, label, recordForm)
        {
        }
    }
}