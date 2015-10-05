using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using JosephM.Core.FieldType;
using JosephM.Record.Metadata;
using JosephM.Xrm;

namespace JosephM.Record.Xrm.XrmRecord
{
    public class XrmPicklistSet : IPicklistSet
    {
        private XrmService XrmService { get; set; }
        public string SchemaName { get; set; }

        public XrmPicklistSet(string optionSetName, XrmService xrmService)
        {
            XrmService = xrmService;
            SchemaName = optionSetName;
        }

        public string DisplayName
        {
            get { return XrmService.GetSharedPicklistDisplayName(SchemaName); }
        }

        public IEnumerable<PicklistOption> PicklistOptions
        {
            get
            {
                return XrmService.GetSharedOptionSetKeyValues(SchemaName)
                    .Select(kv => new PicklistOption(kv.Key.ToString(CultureInfo.InvariantCulture), kv.Value))
                    .ToArray();
            }
        }
    }
}
