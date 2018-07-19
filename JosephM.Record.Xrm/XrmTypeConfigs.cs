using JosephM.Record.Extentions;
using JosephM.Record.IService;
using JosephM.Record.Xrm.XrmRecord;
using System.Collections.Generic;
using System.Linq;

namespace JosephM.Record.Xrm
{
    /// <summary>
    /// This class is for configuring types which have root/parent/child type relationships
    /// so when they are migrated between environments they correctly resolve targets
    /// despite names not necessarily being populated or unique
    /// </summary>
    public class XrmTypeConfigs
    {
        public string Type { get; set; }
        public string ParentLookupType { get; set; }
        public string ParentLookupField { get; set; }
        public bool BlockCreateChild { get; set; }
        public IEnumerable<string> UniqueChildFields { get; set; }
        public static IEnumerable<XrmTypeConfigs> _configs = new[]
        {
            new XrmTypeConfigs()
                {
                    Type = "adx_webpage",
                    ParentLookupField = "adx_rootwebpageid",
                    ParentLookupType = "adx_webpage",
                    UniqueChildFields = new [] { "adx_webpagelanguageid" },
                    BlockCreateChild = true
                },
            new XrmTypeConfigs()
                {
                    Type = "adx_entityformmetadata",
                    ParentLookupField = "adx_entityform",
                    ParentLookupType = "adx_entityform",
                    UniqueChildFields = new [] { "adx_type", "adx_sectionname", "adx_attributelogicalname", "adx_tabname", "adx_subgrid_name" }
                },
            new XrmTypeConfigs()
                {
                    Type = "adx_webpageaccesscontrolrule",
                    ParentLookupField = "adx_webpageid",
                    ParentLookupType = "adx_webpage",
                },
        };

        public static XrmTypeConfigs GetFor(string type)
        {
            return _configs.Any(c => c.Type == type)
                ? _configs.First(c => c.Type == type)
                : null;
        }

        public static IEnumerable<string> GetComparisonFieldsFor(string type, XrmRecordService xrmRecordService)
        {
            var config = GetFor(type);
            var fields = new List<string>();
            if (config != null)
            {
                if (config.ParentLookupField != null)
                    fields.Add(config.ParentLookupField);
                if (config.UniqueChildFields != null)
                    fields.AddRange(config.UniqueChildFields);
            }
            fields.Add(xrmRecordService.GetPrimaryField(type));
            return fields;
        }
    }
}
