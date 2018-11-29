using JosephM.Record.Query;
using JosephM.Xrm.Schema;
using System.Collections.Generic;

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
                        RecordType = Entities.adx_webfile,
                        WebSiteField = Fields.adx_webfile_.adx_websiteid
                    }
                };
        }
    }
}