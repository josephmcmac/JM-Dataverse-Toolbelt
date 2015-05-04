#region

using System.Collections.Generic;
using Microsoft.Xrm.Sdk.Metadata;
using JosephM.ObjectMapping;
using JosephM.Record.Metadata;

#endregion

namespace JosephM.Record.Xrm.Mappers
{
    public class FieldTypeMapper : EnumMapper<AttributeTypeCode, RecordFieldType>
    {
        private readonly IEnumerable<TypeMapping<AttributeTypeCode, RecordFieldType>> _mappings =
            new[]
            {
                new TypeMapping<AttributeTypeCode, RecordFieldType>(AttributeTypeCode.DateTime, RecordFieldType.Date),
                new TypeMapping<AttributeTypeCode, RecordFieldType>(AttributeTypeCode.Customer, RecordFieldType.Lookup),
                new TypeMapping<AttributeTypeCode, RecordFieldType>(AttributeTypeCode.Owner, RecordFieldType.Lookup),
                new TypeMapping<AttributeTypeCode, RecordFieldType>(AttributeTypeCode.PartyList,
                    RecordFieldType.ActivityParty)
            };

        protected override RecordFieldType DefaultEnum2Option
        {
            get { return RecordFieldType.Unknown; }
        }

        protected override IEnumerable<TypeMapping<AttributeTypeCode, RecordFieldType>> Mappings
        {
            get { return _mappings; }
        }
    }
}