#region

using System;
using System.Collections.Generic;
using JosephM.Record.IService;

#endregion

namespace JosephM.Record.Metadata
{
    public static class RecordMetadataFactory
    {
        public static IEnumerable<FieldMetadata> GetClassFieldMetadata(Type configTypeName)
        {
            var result = new List<FieldMetadata>();

            var properties = configTypeName.GetProperties();

            foreach (var item in properties)
            {
                result.Add(FieldMetadata.Create(item));
            }

            return result;
        }

        public static T UnloadRecordToObject<T>(IRecord record) where T : new()
        {
            var result = new T();
            var properties = result.GetType().GetProperties();
            foreach (var item in properties)
                item.GetSetMethod().Invoke(result, new[] {record.GetField(item.Name)});
            return result;
        }
    }
}