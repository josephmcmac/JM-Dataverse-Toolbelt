#region

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

        public IEnumerable<PicklistOption> PicklistOptions { get; private set; }
        public bool IsSharedOptionSet { get; private set; }
        public string SchemaName { get; private set; }
        public string DisplayName { get; private set; }
    }
}