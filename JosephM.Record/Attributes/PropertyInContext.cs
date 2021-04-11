using JosephM.Core.Attributes;
using JosephM.Record.IService;
using System;

namespace JosephM.Record.Attributes
{
    public abstract class PropertyInContext : Attribute
    {
        public abstract bool IsInContext(IRecordService recordService, IRecord record);
    }
}
