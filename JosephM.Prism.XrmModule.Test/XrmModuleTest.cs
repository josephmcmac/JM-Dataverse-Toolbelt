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
using Microsoft.VisualStudio.TestTools.UnitTesting;

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

        public SavedXrmRecordConfiguration GetAltSavedXrmRecordConfiguration()
        {
            var altConnection = GetSavedXrmRecordConfiguration();
            var altOrgName = "CRMAuto";
            altConnection.OrganizationUniqueName = altOrgName;
            if (!altConnection.Validate().IsValid)
                Assert.Fail($"Could not connect to alt organisation named {altOrgName} for camparison ");
            return altConnection;
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
            DeleteAll(Entities.adx_webform);
            DeleteAll(Entities.adx_webformstep);
            DeleteAll(Entities.adx_webformmetadata);

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

            var webForm = CreateTestRecord(Entities.adx_webform, new Dictionary<string, object>
            {
                { Fields.adx_webform_.adx_name, "IScriptWebForm" }
            });

            var webFormStep = CreateTestRecord(Entities.adx_webformstep, new Dictionary<string, object>
            {
                { Fields.adx_webformstep_.adx_name, "IScriptWebFormStep1" },
                { Fields.adx_webformstep_.adx_webform, webForm.ToEntityReference() }
            });

            var webFormStep2 = CreateTestRecord(Entities.adx_webformstep, new Dictionary<string, object>
            {
                { Fields.adx_webformstep_.adx_name, "IScriptWebFormStep2" },
                { Fields.adx_webformstep_.adx_webform, webForm.ToEntityReference() }
            });

            var webFormMetadata1 = CreateTestRecord(Entities.adx_webformmetadata, new Dictionary<string, object>
            {
                { Fields.adx_webformmetadata_.adx_webformstep, webFormStep.ToEntityReference() },
                { Fields.adx_webformmetadata_.adx_type, new OptionSetValue(OptionSets.WebFormMetadata.Type.Attribute) },
                { Fields.adx_webformmetadata_.adx_attributelogicalname, "foo" },
            });

            var webFormMetadata2 = CreateTestRecord(Entities.adx_webformmetadata, new Dictionary<string, object>
            {
                { Fields.adx_webformmetadata_.adx_webformstep, webFormStep.ToEntityReference() },
                { Fields.adx_webformmetadata_.adx_type, new OptionSetValue(OptionSets.WebFormMetadata.Type.Attribute) },
                { Fields.adx_webformmetadata_.adx_attributelogicalname, "bar" },
            });

            var webFormMetadata3 = CreateTestRecord(Entities.adx_webformmetadata, new Dictionary<string, object>
            {
                { Fields.adx_webformmetadata_.adx_attributelogicalname, null },
                { Fields.adx_webformmetadata_.adx_webformstep, webFormStep.ToEntityReference() },
                { Fields.adx_webformmetadata_.adx_type, new OptionSetValue(OptionSets.WebFormMetadata.Type.Notes) }
            });

            var webFormMetadata4 = CreateTestRecord(Entities.adx_webformmetadata, new Dictionary<string, object>
            {
                { Fields.adx_webformmetadata_.adx_attributelogicalname, null },
                { Fields.adx_webformmetadata_.adx_webformstep, webFormStep2.ToEntityReference() },
                { Fields.adx_webformmetadata_.adx_type, new OptionSetValue(OptionSets.WebFormMetadata.Type.Notes) }
            });
        }
    }
}
