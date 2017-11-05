using System;
using JosephM.Core.Security;

namespace JosephM.Core.FieldType
{
    public class Money
    {
        public decimal Amount { get; set; }

        public Money(decimal amount)
        {
            Amount = amount;
        }
    }
}