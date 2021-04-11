using JosephM.Record.IService;
using System;

namespace JosephM.Record.Attributes
{
    /// <summary>
    ///     Attribute To Define An Alternative Display Name For A Class Type Through The TypeEntentions.GetDisplayName Method
    /// </summary>
    [AttributeUsage(
        AttributeTargets.Class | AttributeTargets.Property)]
    public class HiddenAttribute : PropertyInContext
    {
        public override bool IsInContext(IRecordService recordService, IRecord record)
        {
            return false;
        }
    }
}