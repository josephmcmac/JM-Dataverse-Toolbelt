#region

using System;
using System.Collections.Generic;
using System.Reflection;
using JosephM.Core.Attributes;
using JosephM.Core.Extentions;
using JosephM.Core.FieldType;
using JosephM.Record.IService;
using JosephM.Record.Attributes;

#endregion

namespace JosephM.Record.Metadata
{
    /// <summary>
    ///     Base Type For Metadata Describing A Data Field In A Record. Extendable For Specific Field Types
    /// </summary>
    public abstract class FieldMetadata : IFieldMetadata
    {
        public virtual bool IsPersistent { get { return true; } }

        internal FieldMetadata(string internalName, string label)
            : this(null, internalName, label)
        {
            SchemaName = internalName;
            DisplayName = label;
        }

        internal FieldMetadata(string recordType, string internalName, string label)
        {
            SchemaName = internalName;
            DisplayName = label;
            RecordType = recordType;
            Readable = true;
            Writeable = true;
            Createable = true;
            IsDisplayRelated = true;
        }

        public int Order { get; set; }
        public bool NotNullable = true;

        public string Description { get; set; }
        public string RecordType { get; set; }
        public string SchemaName { get; set; }
        public string DisplayName { get; private set; }
        public abstract RecordFieldType FieldType { get; }
        public bool IsMandatory { get; set; }
        public bool Audit { get; set; }
        public bool Searchable { get; set; }

        public bool Readable { get; set; }
        public bool Writeable { get; set; }
        public bool Createable { get; set; }
        public bool IsDisplayRelated { get; set; }

        public bool IsPrimaryKey { get; set; }
        public int MaxLength { get; set; }
        public decimal MinValue { get; set; }
        public decimal MaxValue { get; set; }
        public int DecimalPrecision { get; set; }
        public IntegerType IntegerFormat { get; set; }
        public TextFormat TextFormat { get; set; }
        public bool IncludeTime { get; set; }
        public bool IsNonNullable { get; set; }
        public virtual bool IsSharedPicklist { get { return false; } }
        public virtual string PicklistName { get { return null; } }
        public bool IsCustomField { get; set; }

        public string MetadataId { get; set; }

        public string SchemaNameQualified
        {
            get
            {
                return string.Format("{0}.{1}", RecordType, SchemaName ?? DisplayName);
            }
        }

        public static FieldMetadata Create(PropertyInfo propertyInfo, IDictionary<string, Type> objectTypeMaps = null)
        {
            var recordType = propertyInfo.ReflectedType != null ? propertyInfo.ReflectedType.Name : null;
            var type = propertyInfo.PropertyType;
            var isNullableType = type.Name == "Nullable`1";
            if (isNullableType)
                type = type.GetGenericArguments()[0];
            var internalName = propertyInfo.Name;
            var label = propertyInfo.GetDisplayName();

            FieldMetadata fm = null;
            if (type == typeof(bool))
                fm = new BooleanFieldMetadata(recordType, internalName, label);
            else if (type.IsEnum)
            {
                var options = new List<PicklistOption>();
                foreach (Enum item in type.GetEnumValues())
                    options.Add(new PicklistOption(item.ToString(), item.GetDisplayString()));
                fm = new PicklistFieldMetadata(recordType, internalName, label, options);
            }
            else if (type == typeof(Password))
                fm = new PasswordFieldMetadata(recordType, internalName, label);
            else if (type == typeof(Folder))
                fm = new FolderFieldMetadata(recordType, internalName, label);
            else if (type == typeof(string))
            {
                fm = new StringFieldMetadata(recordType, internalName, label);
                if (propertyInfo.GetCustomAttribute<Multiline>() != null)
                    fm.TextFormat = TextFormat.TextArea;
            }
            else if (type == typeof(IEnumerable<string>))
                fm = new StringEnumerableFieldMetadata(recordType, internalName, label);
            else if (type == typeof(int))
            {
                fm = new IntegerFieldMetadata(recordType, internalName, label) { NotNullable = !isNullableType };
            }
            else if (type == typeof(Lookup))
                fm = new LookupFieldMetadata(recordType, internalName, label, null);
            else if (type == typeof(RecordType))
                fm = new RecordTypeFieldMetadata(internalName, label);
            else if (type == typeof(RecordField))
                fm = new RecordFieldFieldMetadata(internalName, label);
            else if (type == typeof(FileReference))
                fm = new FileRefFieldMetadata(internalName, label);
            else if (type == typeof(DateTime))
                fm = new DateFieldMetadata(internalName, label);
            else if (type.IsIEnumerableOfT())
            {
                if (objectTypeMaps != null && objectTypeMaps.ContainsKey(internalName))
                {
                    fm = new EnumerableFieldMetadata(internalName, label, objectTypeMaps[internalName].AssemblyQualifiedName);
                }
                else
                    fm = new EnumerableFieldMetadata(internalName, label, type.GetGenericArguments()[0].AssemblyQualifiedName);
            }
            else if (type == typeof(decimal))
                fm = new DecimalFieldMetadata(internalName, label);
            else
                fm = new ObjectFieldMetadata(recordType, internalName, label, propertyInfo.ReflectedType);
            if (fm == null)
                throw new ArgumentOutOfRangeException(type + " not implemented");
            fm.IsMandatory = true;
            fm.Readable = propertyInfo.GetSetMethod() != null;
            fm.Createable = propertyInfo.GetGetMethod() != null;
            fm.Writeable = propertyInfo.GetGetMethod() != null;
            fm.Searchable = propertyInfo.GetCustomAttribute<NotSearchable>() == null;
            var orderAttribute = propertyInfo.GetCustomAttribute<DisplayOrder>();
            if (orderAttribute != null)
                fm.Order = orderAttribute.Order;
            else
                fm.Order = 100000;
            return fm;
        }
    }
}