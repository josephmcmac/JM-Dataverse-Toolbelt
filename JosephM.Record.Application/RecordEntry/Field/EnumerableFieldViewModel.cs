using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using JosephM.Core.FieldType;
using JosephM.Core.Service;
using JosephM.Record.Application.RecordEntry.Form;

namespace JosephM.Record.Application.RecordEntry.Field
{
    public class EnumerableFieldViewModel : FieldViewModel<IEnumerable>
    {
        public EnumerableFieldViewModel(string fieldName, string label, RecordEntryViewModelBase recordForm)
            : base(fieldName, label, recordForm)
        {
        }

        public string StringDisplay
        {
            get
            {
                var list = new List<string>();
                if (Enumerable != null)
                {
                    foreach (var item in Enumerable)
                        list.Add(item == null ? "" : item.ToString());
                }
                return string.Join(",", list.ToArray());
            }
        }

        public IEnumerable Enumerable
        {
            get { return ValueObject == null ? null : (IEnumerable) ValueObject; }
        }
    }
}