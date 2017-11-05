#region

using System;
using System.Collections.Generic;
using JosephM.Application.ViewModel.RecordEntry.Form;
using JosephM.Core.FieldType;
using System.Collections;
using JosephM.Core.Extentions;

#endregion

namespace JosephM.Application.ViewModel.RecordEntry.Field
{
    public class PicklistMultiSelectFieldViewModel : MultiSelectFieldViewModel<PicklistOption>
    {
        public PicklistMultiSelectFieldViewModel(string fieldName, string label, RecordEntryViewModelBase recordForm)
            : base(fieldName, label, recordForm)
        {
        }

        public override IEnumerable<PicklistOption> Value
        {
            get
            {
                if (ValueObject != null)
                {
                    var objectList = new List<object>();
                    foreach (var item in (IEnumerable)ValueObject)
                        objectList.Add(PicklistOption.EnumToPicklistOption((Enum)item));
                    return (IEnumerable<PicklistOption>)typeof(PicklistOption).ToNewTypedEnumerable(objectList);
                }
                return new PicklistOption[0];
            }
            set
            {
                ValueObject = value;
            }
        }
    }
}