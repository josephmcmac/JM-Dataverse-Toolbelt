using JosephM.Record.Extentions;
using JosephM.Record.IService;
using JosephM.Record.Xrm.XrmRecord;
using JosephM.Xrm.Schema;
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
                    Type = Entities.adx_webpage,
                    ParentLookupField = Fields.adx_webpage_.adx_rootwebpageid,
                    ParentLookupType = Entities.adx_webpage,
                    UniqueChildFields = new [] { Fields.adx_webpage_.adx_webpagelanguageid },
                    BlockCreateChild = true
                },
            new XrmTypeConfigs()
                {
                    Type = Entities.adx_entityformmetadata,
                    ParentLookupField = Fields.adx_entityformmetadata_.adx_entityform,
                    ParentLookupType = Entities.adx_entityform,
                    UniqueChildFields = new [] { Fields.adx_entityformmetadata_.adx_type, Fields.adx_entityformmetadata_.adx_sectionname, Fields.adx_entityformmetadata_.adx_attributelogicalname, Fields.adx_entityformmetadata_.adx_tabname, Fields.adx_entityformmetadata_.adx_subgrid_name }
                },
            new XrmTypeConfigs()
                {
                    Type = Entities.adx_webpageaccesscontrolrule,
                    ParentLookupField = Fields.adx_webpageaccesscontrolrule_.adx_webpageid,
                    ParentLookupType = Entities.adx_webpage,
                },
            new XrmTypeConfigs()
                {
                    Type = Entities.productpricelevel,
                    ParentLookupField = Fields.productpricelevel_.pricelevelid,
                    ParentLookupType = Entities.pricelevel,
                    UniqueChildFields = new [] { Fields.productpricelevel_.productid, Fields.productpricelevel_.uomid }
                },
            new XrmTypeConfigs()
                {
                    Type = Entities.uom,
                    ParentLookupField = Fields.uom_.uomscheduleid,
                    ParentLookupType = Entities.uomschedule,
                    UniqueChildFields = new [] { Fields.uom_.baseuom, Fields.uom_.name }
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
                var parentComparisonFields = GetParentFieldsRequiredForComparison(type);
                if(parentComparisonFields != null)
                    fields.AddRange(parentComparisonFields.Select(pc => config.ParentLookupField + "." + pc));
            }
            fields.Add(xrmRecordService.GetPrimaryField(type));
            return fields;
        }

        public static IEnumerable<string> GetParentFieldsRequiredForComparison(string type)
        {
            var thisTypeConfig = GetFor(type);
            if (thisTypeConfig == null)
                return null;
            var thisTypesParentsConfig = GetFor(thisTypeConfig.ParentLookupType);
            if (thisTypesParentsConfig == null || thisTypeConfig.Type == thisTypeConfig.ParentLookupType)
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
