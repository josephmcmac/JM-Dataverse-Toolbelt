using JosephM.Application.ViewModel.RecordEntry.Form;
using JosephM.Record.IService;

namespace JosephM.Application.ViewModel.RecordEntry.Field
{
    public class ActivityPartyFieldViewModel : FieldViewModel<IRecord[]>
    {
        public ActivityPartyFieldViewModel(string fieldName, string label, RecordEntryViewModelBase recordForm)
            : base(fieldName, label, recordForm)
        {
        }
    }
}