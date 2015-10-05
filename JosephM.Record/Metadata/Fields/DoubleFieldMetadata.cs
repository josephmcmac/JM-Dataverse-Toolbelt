using System;

namespace JosephM.Record.Metadata
{
    /// <summary>
    ///     Stored the metadata for a field
    /// </summary>
    public class DoubleFieldMetadata : FieldMetadata
    {
        public DoubleFieldMetadata(string internalName, string label)
            : this(null,internalName, label)
        {
            //decimal to double convert in doublefieldmetadata could not work on export
            MinValue = Convert.ToDecimal(double.MinValue);
            MaxValue = Convert.ToDecimal(double.MaxValue); ;
        }

        public DoubleFieldMetadata(string recordType, string internalName, string label)
            : base(recordType, internalName, label)
        {
            MinValue = Convert.ToDecimal(Decimal.MinValue);
            MaxValue = Convert.ToDecimal(Decimal.MaxValue); ;
            DecimalPrecision = 10;
        }

        public override RecordFieldType FieldType
        {
            get { return RecordFieldType.Double; }
        }
    }
}