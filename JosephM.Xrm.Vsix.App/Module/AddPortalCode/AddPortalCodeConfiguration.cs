using JosephM.Record.Extentions;
using JosephM.Record.IService;
using JosephM.Record.Query;
using JosephM.Xrm.Schema;
using System;
using System.Collections.Generic;
using System.Linq;

namespace JosephM.Xrm.Vsix.Module.AddPortalCode
{
    public class AddPortalCodeConfiguration
    {
        public string RecordType { get; set; }
        public string WebSiteField { get; set; }
        public IEnumerable<ExportFieldConfigration> FieldsToExport { get; set; }
        public IEnumerable<Condition> Conditions { get; set; }

        public class ExportFieldConfigration
        {
            public string FieldName { get; set; }
            public string Extention { get; set; }
        }

        public static IEnumerable<IRecord> GetRecordsForConfig(string recordType, IRecordService service, string websiteId)
        {
            var config = GetConfigFor(recordType);
            var query = new QueryDefinition(recordType);
            if (config.Conditions != null)
                query.RootFilter.Conditions.AddRange(config.Conditions);
            service.AddJoinCondition(query, config.WebSiteField, ConditionType.Equal, websiteId);
            return service.RetreiveAll(query);
        }

        private static AddPortalCodeConfiguration GetConfigFor(string recordType)
        {
            var configs = GetExportConfigs();
            if (configs.Any(c => c.RecordType == recordType))
                return configs.First(c => c.RecordType == recordType);
            throw new Exception("No config defined for type " + recordType);
        }

        public static IEnumerable<AddPortalCodeConfiguration> GetExportConfigs()
        {
            return new[]
            {
                    new AddPortalCodeConfiguration
                    {
                        RecordType = Entities.adx_webpage,
                        Conditions = new [] { new Condition(Fields.adx_webpage_.adx_rootwebpageid, ConditionType.NotNull) },
                        WebSiteField = Fields.adx_webpage_.adx_websiteid,
                        FieldsToExport = new []
                        {
                            new AddPortalCodeConfiguration.ExportFieldConfigration
                            {
                                FieldName = Fields.adx_webpage_.adx_copy,
                                Extention = "html"
                            },
                            new AddPortalCodeConfiguration.ExportFieldConfigration
                            {
                                FieldName = Fields.adx_webpage_.adx_customjavascript,
                                Extention = "js"
                            },
                            new AddPortalCodeConfiguration.ExportFieldConfigration
                            {
                                FieldName = Fields.adx_webpage_.adx_customcss,
                                Extention = "css"
                            }
                        }
                    },
                    new AddPortalCodeConfiguration
                    {
                        RecordType = Entities.adx_webformstep,
                        WebSiteField = Fields.adx_webformstep_.adx_webform + "." + Fields.adx_webform_.adx_websiteid,
                        FieldsToExport = new []
                        {
                            new AddPortalCodeConfiguration.ExportFieldConfigration
                            {
                                FieldName = Fields.adx_webformstep_.adx_registerstartupscript,
                                Extention = "js"
                            }
                        }
                    },
                    new AddPortalCodeConfiguration
                    {
                        RecordType = Entities.adx_webtemplate,
                        WebSiteField = Fields.adx_webtemplate_.adx_websiteid,
                        FieldsToExport = new []
                        {
                            new AddPortalCodeConfiguration.ExportFieldConfigration
                            {
                                FieldName = Fields.adx_webtemplate_.adx_source,
                                Extention = "html"
                            }
                        }
                    },
                    new AddPortalCodeConfiguration
                    {
                        RecordType = Entities.adx_entityform,
                        WebSiteField = Fields.adx_entityform_.adx_websiteid,
                        FieldsToExport = new []
                        {
                            new AddPortalCodeConfiguration.ExportFieldConfigration
                            {
                                FieldName = Fields.adx_entityform_.adx_registerstartupscript,
                                Extention = "js"
                            }
                        }
                    },
                    new AddPortalCodeConfiguration
                    {
                        RecordType = Entities.adx_entitylist,
                        WebSiteField = Fields.adx_entitylist_.adx_websiteid,
                        FieldsToExport = new []
                        {
                            new AddPortalCodeConfiguration.ExportFieldConfigration
                            {
                                FieldName = Fields.adx_entitylist_.adx_registerstartupscript,
                                Extention = "js"
                            }
                        }
                    },
                    new AddPortalCodeConfiguration
                    {
                        RecordType = Entities.adx_contentsnippet,
                        WebSiteField = Fields.adx_contentsnippet_.adx_websiteid,
                        FieldsToExport = new []
                        {
                            new AddPortalCodeConfiguration.ExportFieldConfigration
                            {
                                FieldName = Fields.adx_contentsnippet_.adx_value,
                                Extention = "html"
                            }
                        }
                    },
                    new AddPortalCodeConfiguration
                    {
                        RecordType = Entities.adx_webfile,
                        WebSiteField = Fields.adx_webfile_.adx_websiteid
                    }
                };
        }
    }
}