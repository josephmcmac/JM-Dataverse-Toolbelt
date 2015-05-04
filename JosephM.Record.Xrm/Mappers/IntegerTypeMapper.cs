#region

using Microsoft.Xrm.Sdk.Metadata;
using JosephM.ObjectMapping;
using JosephM.Record.Metadata;

#endregion

namespace JosephM.Record.Xrm.Mappers
{
    public class IntegerTypeMapper : EnumMapper<IntegerType, IntegerFormat>
    {
        protected override IntegerType DefaultEnum1Option
        {
            get { return IntegerType.None; }
        }
    }
}