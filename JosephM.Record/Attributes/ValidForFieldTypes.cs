using JosephM.Record.Metadata;
using System;
using System.Collections.Generic;

namespace JosephM.Record.Attributes
{
    public class ValidForFieldTypes : Attribute
    {
        public IntegerType? IntegerType { get; set; }

        public IEnumerable<RecordFieldType> FieldTypes { get; set; }
        public string TargetType { get; set; }
        public bool MultiSelectOnly { get; set; }

        public ValidForFieldTypes(params RecordFieldType[] fieldTypes)
        {
            FieldTypes = fieldTypes;
        }

        public ValidForFieldTypes(string targetType, params RecordFieldType[] fieldTypes)
        {
            TargetType = targetType;
            FieldTypes = fieldTypes;
        }

        public ValidForFieldTypes(IntegerType integerType, params RecordFieldType[] fieldTypes)
        {
            IntegerType = integerType;
            FieldTypes = fieldTypes;
        }
    }
}
