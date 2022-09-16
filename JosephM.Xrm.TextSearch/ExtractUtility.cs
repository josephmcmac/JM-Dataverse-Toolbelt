using JosephM.Application.ViewModel.SettingTypes;
using JosephM.Core.Extentions;
using JosephM.Core.FieldType;
using JosephM.Xrm.Schema;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace JosephM.Xrm.TextSearch
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
                Entities.solutioncomponent, Entities.workflowdependency, Entities.audit, Entities.calendar, Entities.importjob, Entities.plugintypestatistic, "cdi_excludedemail", Entities.postfollow, Entities.sdkmessageprocessingstepsecureconfig, Entities.activitypointer, Entities.attributemap,
                Entities.organizationui, Entities.lookupmapping, Entities.unresolvedaddress, Entities.subscription, Entities.rollupfield, Entities.picklistmapping,
                Entities.importentitymapping, Entities.importlog, Entities.workflowlog, Entities.ribboncommand, Entities.leadaddress, Entities.invaliddependency,
                Entities.fieldpermission, Entities.stringmap, Entities.systemform, Entities.customerrelationship, Entities.timezonerule,
                Entities.partnerapplication, Entities.calendarrule, Entities.customeropportunityrole, Entities.multientitysearch, Entities.wizardpage,
                Entities.authorizationserver, Entities.complexcontrol, Entities.workflowwaitsubscription, Entities.sdkmessagerequestfield,
                Entities.untrackedemail, Entities.mailboxtrackingfolder, Entities.sharepointdocument, Entities.pluginassembly, Entities.sdkmessageprocessingstep, Entities.sdkmessageprocessingstepimage, Entities.plugintracelog,
                Entities.solutioncomponent, Entities.msdyn_solutioncomponentsummary, Entities.msdyn_componentlayer, Entities.msdyn_componentlayerdatasource, Entities.msdyn_solutioncomponentdatasource, Entities.msdyn_solutionhistory,
                Entities.msdyn_nonrelationalds, Entities.datalakeworkspace, Entities.datalakeworkspacepermission, Entities.msdyn_casesuggestion, Entities.msdyn_knowledgearticlesuggestion, Entities.virtualresourcegroupresource, Entities.msdyn_solutioncomponentcountsummary, Entities.msdyn_solutioncomponentcountdatasource, "aaduser"
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
                },
                new RecordFieldSetting()
                {
                    RecordField = new RecordField("description", "description"),
                    RecordType = new RecordType("annotation", "annotation")
                },
                new RecordFieldSetting()
                {
                    RecordField = new RecordField("incident", "incident"),
                    RecordType = new RecordType("description", "description")
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