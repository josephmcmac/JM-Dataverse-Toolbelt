#region

using System;
using System.Collections.Generic;
using JosephM.Core.Extentions;
using JosephM.Core.FieldType;
using JosephM.Record.Application.RecordEntry.Form;

#endregion

namespace JosephM.Record.Application.RecordEntry.Field
{
    public class PicklistFieldViewModel : FieldViewModel<PicklistOption>
    {
        public PicklistFieldViewModel(string fieldName, string label, RecordEntryViewModelBase recordForm)
            : base(fieldName, label, recordForm)
        {
        }

        public override PicklistOption Value
        {
            get
            {
                if (ValueObject is Enum)
                {
                    var item = (Enum) ValueObject;
                    return new PicklistOption(item.ToString(), item.GetDisplayString());
                }
                return ValueObject as PicklistOption;
            }
            set { ValueObject = value; }
        }

        public IEnumerable<PicklistOption> ItemsSource { get; set; }
    }
}