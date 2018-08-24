using Microsoft.Xrm.Sdk.Query;
using JosephM.ObjectMapping;
using JosephM.Record.Query;

namespace JosephM.Record.Xrm.Mappers
{
    public class ConditionTypeMapper : EnumMapper<ConditionType, ConditionOperator>
    {
        protected override ConditionType DefaultEnum1Option => ConditionType.UnmatchedConditionType;
    }
}