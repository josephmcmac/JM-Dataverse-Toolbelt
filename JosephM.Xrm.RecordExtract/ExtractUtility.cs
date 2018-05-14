using JosephM.Application.ViewModel.SettingTypes;
using JosephM.Core.Extentions;
using JosephM.Core.FieldType;
using JosephM.Xrm.Schema;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace JosephM.Xrm.RecordExtract
{
    public static class ExtractUtility
    {
        public static int TextSearchSetSize { get { return 5000; } }

        public static IEnumerable<RecordFieldSetting> GetSystemHtmlFields()
        {
            return new[]
            {
                new RecordFieldSetting()
                {
                    RecordField = new RecordField(Fields.email_.description, Fields.email_.description),
                    RecordType = new RecordType(Entities.email, Entities.email)
                },
                new RecordFieldSetting()
                {
                    RecordField = new RecordField(Fields.knowledgearticle_.content, Fields.knowledgearticle_.content),
                    RecordType = new RecordType(Entities.knowledgearticle, Entities.knowledgearticle)
                }
            };
        }

        public static string CheckStripFormatting(string value, string recordType, string field)
        {
            if (recordType == "post" && field == "text")
            {
                return StripPostLinks(value.StripXmlTags());
            }
            return value;
        }

        private static string StripPostLinks(string value)
        {
            var temp = Regex.Replace(value,
                @"@\[\d,[A-F0-9]{8}(?:-[A-F0-9]{4}){3}-[A-F0-9]{12},\""",
                "",
                RegexOptions.IgnoreCase);
            return Regex.Replace(temp,
                @"\""\]",
                "",
                RegexOptions.IgnoreCase);
        }

        public static IEnumerable<string> GetSystemRecordTypesToExclude()
        {
            return new[]
            {
                "solutioncomponent", "workflowdependency", "audit", "calendar", "importjob", "plugintypestatistic", "cdi_excludedemail", "postfollow", "sdkmessageprocessingstepsecureconfig", "activitypointer", "attributemap",
                "organizationui", "lookupmapping", "unresolvedaddress", "subscription", "rollupfield", "picklistmapping",
                "importentitymapping", "importlog", "workflowlog", "ribboncommand", "leadaddress", "invaliddependency",
                "fieldpermission", "stringmap", "systemform", "customerrelationship", "timezonerule",
                "partnerapplication", "calendarrule", "customeropportunityrole", "multientitysearch", "wizardpage",
                "authorizationserver", "complexcontrol", "workflowwaitsubscription", "sdkmessagerequestfield",
                "untrackedemail", "mailboxtrackingfolder", "sharepointdocument", "pluginassembly", "sdkmessageprocessingstep", "sdkmessageprocessingstepimage", "plugintracelog"
            };
        }

        public static IEnumerable<string> GetSystemFieldsToExclude()
        {
            return new[]
            {
                "articlexml", "organization", "ahrc_sendforapproval", "ahrc_sentfrommediarelease", "readreceiptrequested", "directioncode",
                "deliveryreceiptrequested", "correlationmethod", "compressed", "activitytypecode", "messageid",
                "importsequencenumber", "timezoneruleversionnumber", "transactioncurrencyid", "exchangerate",
                "participatesinworkflow", "ahrc_drupalkey", "mimetype", "isdocument", "filesize", "documentbody",
                "isbilled", "isregularactivity", "isworkflowcreated", "leftvoicemail", "instancetypecode", "cdi_gender",
                "address1_line1", "address1_line2", "address1_line3", "address1_city", "address1_county",
                "address1_stateprovince", "address1_country", "address1_postalcode", "address2_line1", "address2_line2",
                "address2_line3", "address2_city", "address2_county", "address2_stateprovince", "address2_country",
                "address2_postalcode", "objecttypecode"
            };
        }

        public static IEnumerable<string> GetSystemRelationshipsToExclude()
        {
            return new[]
            {
                "listcontact_association",
                "listaccount_association",
                "listlead_association"
            };
        }

        public static IEnumerable<RecordFieldSetting> GetSystemTextSearchSetFields()
        {
            return new[]
            {
                new RecordFieldSetting()
                {
                    RecordField = new RecordField("description", "description"),
                    RecordType = new RecordType("email", "email")
                },
                new RecordFieldSetting()
                {
                    RecordField = new RecordField("notetext", "notetext"),
                    RecordType = new RecordType("annotation", "annotation")
                }
            };
        }

        public static IEnumerable<string> GetStringValuesToExclude()
        {
            return new[]
            {
                "Default Value"
            };
        }
    }
}