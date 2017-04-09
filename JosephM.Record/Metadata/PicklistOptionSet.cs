#region

using System;
using System.Collections.Generic;
using JosephM.Core.FieldType;

#endregion

namespace JosephM.Record.Metadata
{
    public class PicklistOptionSet : IPicklistSet
    {
        public PicklistOptionSet()
        {
            PicklistOptions = new PicklistOption[] {};
        }

        public PicklistOptionSet(IEnumerable<PicklistOption> picklistOptions)
        {
            PicklistOptions = picklistOptions;
        }

        public PicklistOptionSet(IEnumerable<PicklistOption> picklistOptions, bool isSharedOptionSet, string schemaName,
            string displayName)
        {
            PicklistOptions = picklistOptions;
            IsSharedOptionSet = isSharedOptionSet;
            DisplayName = displayName;
            SchemaName = schemaName;
        }

        public IEnumerable<PicklistOption> PicklistOptions { get; set; }
        public bool IsSharedOptionSet { get; set; }
        public string SchemaName { get; set; }
        public string DisplayName { get; set; }
        public string MetadataId { get; set; }

        public string SchemaNameQualified
        {
            get
            {
                return SchemaName ?? DisplayName;
            }
        }
    }
}