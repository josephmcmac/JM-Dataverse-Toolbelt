using JosephM.Core.Attributes;
using JosephM.Core.FieldType;
using JosephM.Core.Service;
using System;

namespace JosephM.Deployment.DataImport
{
    public class DataImportResponseItem : ServiceResponseItem
    {
        [DisplayOrder(10)]
        public string Entity { get; }

        [DisplayOrder(20)]
        public string Name { get; }

        [DisplayOrder(30)]
        [PropertyInContextByPropertyNotNull(nameof(Field))]
        public string Field { get; }

        [GridWidth(400)]
        [DisplayOrder(40)]
        public string Message { get; }

        [PropertyInContextByPropertyNotNull(nameof(Link))]
        [GridWidth(62)]
        [DisplayOrder(50)]
        public Url Link { get; }

        public DataImportResponseItem(string entity, string field, string name, string message, Exception ex, Url url = null)
            : this(message, ex)
        {
            Entity = entity;
            Field = field;
            Name = name;
            Link = url;
        }

        public DataImportResponseItem(string message, Exception ex)
        {
            Message = message;
            Exception = ex;
        }
    }
}