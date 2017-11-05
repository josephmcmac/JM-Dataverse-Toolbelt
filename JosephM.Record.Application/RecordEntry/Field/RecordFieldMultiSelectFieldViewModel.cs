#region

using System.Linq;
using JosephM.Application.ViewModel.RecordEntry.Form;
using JosephM.Core.FieldType;
using JosephM.Record.Extentions;

#endregion

namespace JosephM.Application.ViewModel.RecordEntry.Field
{
    public class RecordFieldMultiSelectFieldViewModel : MultiSelectFieldViewModel<RecordField>
    {
        public RecordFieldMultiSelectFieldViewModel(string fieldName, string label, RecordEntryViewModelBase recordForm)
            : base(fieldName, label, recordForm)
        {
        }

        private string _recordTypeForField;
        public string RecordTypeForField
        {
            get { return _recordTypeForField; }
            set
            {
                _recordTypeForField = value;
                var reference = GetRecordForm().ParentFormReference == null
                    ? _recordTypeForField
                    : _recordTypeForField + ":" + GetRecordForm().ParentFormReference;
                SetItemsSource(GetRecordService()
                    .GetPicklistKeyValues(FieldName, GetRecordType(), reference, RecordEntryViewModel.GetRecord())
                    .Select(p => new RecordField(p.Key, p.Value)).ToArray());
            }
        }
    }
}