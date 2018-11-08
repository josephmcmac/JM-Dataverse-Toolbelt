using JosephM.Core.Attributes;
using JosephM.Core.FieldType;
using JosephM.Core.Service;
using System;

namespace JosephM.Deployment.DataImport
{
    public class DataImportResponseItem : ServiceResponseItem
    {
        [PropertyInContextByPropertyNotNull(nameof(RowNumber))]
        [GridWidth(125)]
        [DisplayOrder(5)]
        public int? RowNumber { get; }

        [DisplayOrder(10)]
        public string Entity { get; }

        [DisplayOrder(20)]
        public string Name { get; }

        [DisplayOrder(30)]
        [PropertyInContextByPropertyNotNull(nameof(Field))]
        public string Field { get; }

        [DisplayOrder(35)]
        [PropertyInContextByPropertyNotNull(nameof(FieldValue))]
        public string FieldValue { get; }

        [GridWidth(400)]
        [DisplayOrder(40)]
        public string Message { get; }

        [PropertyInContextByPropertyNotNull(nameof(Link))]
        [GridWidth(62)]
        [DisplayOrder(50)]
        public Url Link { get; }

        public DataImportResponseItem(string entity, string field, string name, string fieldValue, string message, Exception ex, Url url = null, int? rowNumber = null)
            : this(message, ex)
        {
            Entity = entity;
            Field = field;
            Name = name;
            FieldValue = fieldValue;
            Link = url;
            RowNumber = rowNumber;
        }

        public DataImportResponseItem(string message, Exception ex, int? rowNumber = null)
        {
            Message = message;
            Exception = ex;
            RowNumber = rowNumber;
        }
    }
}