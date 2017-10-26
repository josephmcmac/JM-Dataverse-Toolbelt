using System;
using System.Collections.Generic;

namespace JosephM.Core.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class OrderPriority : Attribute
    {
        public OrderPriority(params string[] priorityValues)
        {
            PriorityValues = priorityValues;
        }

        public IEnumerable<string> PriorityValues { get; }
    }
}