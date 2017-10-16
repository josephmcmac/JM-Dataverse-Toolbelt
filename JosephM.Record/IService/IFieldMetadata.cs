using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JosephM.Record.Metadata;
using JosephM.Core.Attributes;
using JosephM.Record.Attributes;

namespace JosephM.Record.IService
{
    public interface IFieldMetadata : IMetadata
    {
        [DisplayOrder(50)]
        RecordFieldType FieldType { get; }
        [Hidden]
        bool IsMandatory { get; }
        [DisplayOrder(40)]
        [QuickFind]
        string DisplayName { get; }
        [Hidden]
        int MaxLength { get; }
        [Hidden]
        decimal MinValue { get; }
        [Hidden]
        decimal MaxValue { get; }
        [Hidden]
        bool IsPrimaryKey { get; }
        [DisplayOrder(80)]
        [GridWidth(400)]
        [QuickFind]
        string Description { get; }
        [Hidden]
        bool Audit { get; }
        [Hidden]
        bool Searchable { get; }
        [Hidden]
        TextFormat TextFormat { get; }
        [Hidden]
        bool IncludeTime { get; }
        [Hidden]
        int DecimalPrecision { get; }
        [Hidden]
        IntegerType IntegerFormat { get; }
        [Hidden]
        bool IsNonNullable { get; }
        [Hidden]
        bool IsSharedPicklist { get; }
        [Hidden]
        string PicklistName { get; }
        [DisplayOrder(70)]
        [GridWidth(100)]
        bool IsCustomField { get; }
        [Hidden]
        bool Readable { get; }
        [Hidden]
        bool Writeable { get; }
        [Hidden]
        bool Createable { get; }
        [Hidden]
        bool IsDisplayRelated { get; }
        [Hidden]
        int Order { get; }
    }
}
