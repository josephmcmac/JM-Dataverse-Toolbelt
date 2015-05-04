#region

using Microsoft.Xrm.Sdk;
using JosephM.Core.FieldType;
using JosephM.ObjectMapping;

#endregion

namespace JosephM.Record.Xrm.Mappers
{
    public class LookupMapper : ClassMapperFor<EntityReference, Lookup>
    {
        public LookupMapper()
        {
            AddPropertyMap<EntityReference, Lookup>("LogicalName", "RecordType");
        }
    }
}