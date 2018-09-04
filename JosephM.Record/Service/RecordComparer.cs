using JosephM.Core.Extentions;
using JosephM.Record.IService;
using System;
using System.Collections.Generic;

namespace JosephM.Record.Service
{
    public class RecordComparer : IComparer<IRecord>
    {
        private string PropertyName { get; set; }
        public RecordComparer(string propertyName)
        {
            PropertyName = propertyName;
        }

        public int Compare(IRecord x, IRecord y)
        {
            var value1 = x.GetField(PropertyName);
            var value2 = y.GetField(PropertyName);
            if (value1 == null && value2 == null)
            {
                return 0;
            }
            if (value2 == null)
            {
                return 1;
            }
            else if (!(value1 is Enum) && value1 is IComparable)
            {
                return ((IComparable)value1).CompareTo(value2);
            }
            var sortString1 = value1.ToString();
            var sortString2 = value2.ToString();
            if (value1 is Enum)
                sortString1 = ((Enum)value1).GetDisplayString();
            if (value2 is Enum)
                sortString2 = ((Enum)value2).GetDisplayString();
            return String.Compare(sortString1, sortString2, StringComparison.Ordinal);
        }
    }
}
