#region

using System;
using System.Collections.Generic;
using JosephM.Record.IService;
using System.Linq;

#endregion

namespace JosephM.Record.Metadata
{
    public static class RecordMetadataFactory
    {
        public static IEnumerable<FieldMetadata> GetClassFieldMetadata(Type configTypeName, IDictionary<string, Type> objectTypeMaps = null)
        {
            var result = new List<FieldMetadata>();

            var properties = configTypeName.GetProperties();

            foreach (var item in properties)
            {
                result.Add(FieldMetadata.Create(item, objectTypeMaps));
            }

            if (configTypeName.IsInterface)
            {
                var interfaces = configTypeName.GetInterfaces();
                foreach(var interface_ in interfaces)
                {
                    foreach (var item in interface_.GetProperties())
                    {
                        if(!result.Any(m => m.SchemaName == item.Name))
                            result.Add(FieldMetadata.Create(item, objectTypeMaps));
                    }
                }
            }
            else
            {
                if (configTypeName.BaseType != null)
                {
                    foreach (var item in configTypeName.BaseType.GetProperties())
                    {
                        if (!result.Any(m => m.SchemaName == item.Name))
                            result.Add(FieldMetadata.Create(item, objectTypeMaps));
                    }
                }
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