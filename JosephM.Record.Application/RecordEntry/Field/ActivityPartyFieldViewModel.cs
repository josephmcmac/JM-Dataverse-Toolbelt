#region

using System;
using System.Linq;
using JosephM.Application.ViewModel.RecordEntry.Form;
using JosephM.Core.Service;
using JosephM.Record.IService;

#endregion

namespace JosephM.Application.ViewModel.RecordEntry.Field
{
    public class ActivityPartyFieldViewModel : FieldViewModel<IRecord[]>
    {
        public ActivityPartyFieldViewModel(string fieldName, string label, RecordEntryViewModelBase recordForm)
            : base(fieldName, label, recordForm)
        {
        }

        public string DisplayString
        {
            get
            {
                return GetRecordService().GetFieldAsDisplayString(Record, FieldName);
            }
        }
    }
}