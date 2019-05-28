using JosephM.Record.Extentions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace JosephM.Record.IService
{
    /// <summary>
    /// This class is for configuring types which have root/parent/child type relationships
    /// so when they are migrated between environments they correctly resolve targets
    /// despite names not necessarily being populated or unique
    /// </summary>
    public class TypeConfigs
    {
        private IEnumerable<Config> _configs;
        public TypeConfigs(IEnumerable<Config> configs)
        {
            _configs = configs ?? new Config[0];
        }

        public class Config
        {
            public string Type { get; set; }
            public string ParentLookupType { get; set; }
            public string ParentLookupField { get; set; }
            public bool BlockCreateChild { get; set; }
            public IEnumerable<string> UniqueChildFields { get; set; }
            public IEnumerable<string> ExplicitDisplayNameFields { get; set; }
        }

        public Config GetFor(string type)
        {
            return _configs.Any(c => c.Type == type)
                ? _configs.First(c => c.Type == type)
                : null;
        }

        public IEnumerable<string> GetComparisonFieldsFor(string type, IRecordService recordService)
        {
            var config = GetFor(type);
            var fields = new List<string>();
            if (config != null)
            {
                if (config.ParentLookupField != null)
                    fields.Add(config.ParentLookupField);
                if (config.UniqueChildFields != null)
                    fields.AddRange(config.UniqueChildFields);
                foreach(var field in fields.ToArray())
                {
                    if(recordService.IsLookup(field, type))
                    {
                        var reffedTypeConfig = GetFor(recordService.GetLookupTargetType(field, type));
                        if(reffedTypeConfig != null)
                        {
                            AddLinkedFields(reffedTypeConfig, fields, field + ".", recordService);
                        }
                    }
                }
            }
            fields.Add(recordService.GetPrimaryField(type));
            return fields;
        }

        private void AddLinkedFields(Config typeConfig, List<string> fieldsList, string prefix, IRecordService recordService)
        {

            var fields = new List<string>();
            if (typeConfig.ParentLookupField != null)
                fields.Add(typeConfig.ParentLookupField);
            if (typeConfig.UniqueChildFields != null)
                fields.AddRange(typeConfig.UniqueChildFields);
            foreach (var field in fields)
            {
                if (recordService.IsLookup(field, typeConfig.Type))
                {
                    var reffedTypeConfig = GetFor(recordService.GetLookupTargetType(field, typeConfig.Type));
                    if (reffedTypeConfig != null)
                    {
                        //if it is self referencing only go one level
                        //otherwise we infinite loop
                        if (prefix != null && prefix.EndsWith(field + "."))
                            continue;

                        AddLinkedFields(reffedTypeConfig, fieldsList, $"{prefix}{field}.", recordService);
                    }
                    else
                    {
                        fieldsList.Add($"{prefix}{field}");
                    }
                }
                else
                {
                    fieldsList.Add($"{prefix}{field}");
                }
            }
        }

        public IEnumerable<string> GetParentFieldsRequiredForComparison(string type)
        {
            var thisTypeConfig = GetFor(type);
            if (thisTypeConfig == null)
                return null;
            var thisTypesParentsConfig = GetFor(thisTypeConfig.ParentLookupType);
            if (thisTypesParentsConfig == null)
                return null;

            //if the parent also has a config then we need to use it when matching the parent
            //e.g. portal web page access rules -> web page where the web page may be a master or child web page
            //so lets include the parents config fields as aliased fields in the exported entity
            var fieldsToIncludeInParent = new List<string> { thisTypesParentsConfig.ParentLookupField };
            if (thisTypesParentsConfig.UniqueChildFields != null)
                fieldsToIncludeInParent.AddRange(thisTypesParentsConfig.UniqueChildFields);
            return fieldsToIncludeInParent;
        }
    }
}
