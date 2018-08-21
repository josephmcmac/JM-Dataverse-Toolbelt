using JosephM.Application.Application;
using JosephM.Application.Modules;
using JosephM.Application.Desktop.Test;
using JosephM.Core.AppConfig;
using JosephM.ObjectMapping;
using JosephM.Record.Xrm.Test;
using JosephM.Record.Xrm.XrmRecord;
using JosephM.XrmModule.Crud;
using JosephM.XrmModule.SavedXrmConnections;
using JosephM.XrmModule.XrmConnection;
using Microsoft.Xrm.Sdk.Client;
using JosephM.Xrm.Schema;
using System.Collections.Generic;
using Microsoft.Xrm.Sdk;
using JosephM.XrmModule.AppConnection;

namespace JosephM.XrmModule.Test
{
    public class XrmModuleTest : XrmRecordTest
    {
        protected XrmModuleTest()
            : base()
        {
            XrmRecordService.SetFormService(new XrmFormService());
        }

        protected virtual TestApplication CreateAndLoadTestApplication<TModule>(ApplicationControllerBase applicationController = null, ISettingsManager settingsManager = null, bool loadXrmConnection = true, bool addSavedConnectionAppConnectionModule = true)
            where TModule : ModuleBase, new()
        {
            var testApplication = TestApplication.CreateTestApplication(applicationController, settingsManager);
            testApplication.AddModule<TModule>();
            if(addSavedConnectionAppConnectionModule)
                testApplication.AddModule<SavedConnectionAppConnectionModule>();
            if (loadXrmConnection)
            {
                XrmConnectionModule.RefreshXrmServices(GetXrmRecordConfiguration(), testApplication.Controller);
                testApplication.Controller.RegisterInstance<ISavedXrmConnections>(new SavedXrmConnections.SavedXrmConnections
                {
                    Connections = new[] { GetSavedXrmRecordConfiguration() }
                });
            }
            return testApplication;
        }

        public SavedXrmRecordConfiguration GetSavedXrmRecordConfiguration()
        {
            var xrmConfig = GetSavedTestEncryptedXrmConfiguration();
            var enumMapper = new EnumMapper<XrmRecordAuthenticationProviderType, AuthenticationProviderType>();
            var savedConfig = new SavedXrmRecordConfiguration()
            {
                Active = true,
                AuthenticationProviderType = enumMapper.Map(xrmConfig.AuthenticationProviderType),
                DiscoveryServiceAddress = xrmConfig.DiscoveryServiceAddress,
                Domain = xrmConfig.Domain,
                OrganizationUniqueName = OverrideOrganisation ?? xrmConfig.OrganizationUniqueName,
                Password = xrmConfig.Password,
                Username = xrmConfig.Username,
                Name = "TESTSCRIPTCONNECTION"
            };
            return savedConfig;
        }

        public XrmRecordConfiguration GetXrmRecordConfiguration()
        {
            var saved = GetSavedXrmRecordConfiguration();
            return new XrmRecordConfiguration()
            {
                AuthenticationProviderType = saved.AuthenticationProviderType,
                DiscoveryServiceAddress = saved.DiscoveryServiceAddress,
                Domain = saved.Domain,
                OrganizationUniqueName = OverrideOrganisation ?? saved.OrganizationUniqueName,
                Password = saved.Password,
                Username = saved.Username,
                Name = "TESTSCRIPTCONNECTION"
            };
        }

        public void RecreatePortalData()
        {
            DeleteAll(Entities.adx_websitelanguage);
            DeleteAll(Entities.adx_webrole);
            DeleteAll(Entities.adx_webpage);
            DeleteAll(Entities.adx_webpageaccesscontrolrule);
            DeleteAll(Entities.adx_entityform);
            DeleteAll(Entities.adx_entityformmetadata);

            var websiteLanguage = CreateTestRecord(Entities.adx_websitelanguage, new Dictionary<string, object>
            {
                { Fields.adx_websitelanguage_.adx_name, "English" }
            });

            var webRole = CreateTestRecord(Entities.adx_webrole, new Dictionary<string, object>
            {
                { Fields.adx_webrole_.adx_name, "TestScriptRole" }
            });

            var webPage = CreateTestRecord(Entities.adx_webpage, new Dictionary<string, object>
            {
                { Fields.adx_webpage_.adx_name, "IScriptWebPage" }
            });
            var childWebPage = CreateTestRecord(Entities.adx_webpage, new Dictionary<string, object>
            {
                { Fields.adx_webpage_.adx_name, "IScriptWebPage" },
                { Fields.adx_webpage_.adx_rootwebpageid, webPage.ToEntityReference() },
                { Fields.adx_webpage_.adx_webpagelanguageid, websiteLanguage.ToEntityReference() }
            });
            var webpageAccessControlRule = CreateTestRecord(Entities.adx_webpageaccesscontrolrule, new Dictionary<string, object>
            {
                { Fields.adx_webpageaccesscontrolrule_.adx_name, "IScriptWebPage" },
                { Fields.adx_webpageaccesscontrolrule_.adx_webpageid, childWebPage.ToEntityReference() },
            });

            XrmService.Associate(Relationships.adx_webrole_.adx_webpageaccesscontrolrule_webrole.Name, Fields.adx_webpageaccesscontrolrule_.adx_webpageaccesscontrolruleid, webpageAccessControlRule.Id, Fields.adx_webrole_.adx_webroleid, webRole.Id);

            var entityForm = CreateTestRecord(Entities.adx_entityform, new Dictionary<string, object>
            {
                { Fields.adx_entityform_.adx_name, "IScriptEntityForm" }
            });

            var entityFormMetadata1 = CreateTestRecord(Entities.adx_entityformmetadata, new Dictionary<string, object>
            {
                { Fields.adx_entityform_.adx_name, null },
                { Fields.adx_entityformmetadata_.adx_entityform, entityForm.ToEntityReference() },
                { Fields.adx_entityformmetadata_.adx_type, new OptionSetValue(OptionSets.EntityFormMetadata.Type.Attribute) },
                { Fields.adx_entityformmetadata_.adx_attributelogicalname, "foo" },
            });

            var entityFormMetadata2 = CreateTestRecord(Entities.adx_entityformmetadata, new Dictionary<string, object>
            {
                { Fields.adx_entityform_.adx_name, null },
                { Fields.adx_entityformmetadata_.adx_entityform, entityForm.ToEntityReference() },
                { Fields.adx_entityformmetadata_.adx_type, new OptionSetValue(OptionSets.EntityFormMetadata.Type.Attribute) },
                { Fields.adx_entityformmetadata_.adx_attributelogicalname, "bar" },
            });

            var entityFormMetadata3 = CreateTestRecord(Entities.adx_entityformmetadata, new Dictionary<string, object>
            {
                { Fields.adx_entityform_.adx_name, null },
                { Fields.adx_entityformmetadata_.adx_entityform, entityForm.ToEntityReference() },
                { Fields.adx_entityformmetadata_.adx_type, new OptionSetValue(OptionSets.EntityFormMetadata.Type.Notes) }
            });
        }
    }
}
