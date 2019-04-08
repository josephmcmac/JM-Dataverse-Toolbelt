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
using System;
using System.IO;
using JosephM.Core.FieldType;

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
            var xrmConfig = XrmConfiguration;
            var enumMapper = new EnumMapper<XrmRecordAuthenticationProviderType, AuthenticationProviderType>();
            var savedConfig = new SavedXrmRecordConfiguration()
            {
                Active = true,
                AuthenticationProviderType = enumMapper.Map(xrmConfig.AuthenticationProviderType),
                DiscoveryServiceAddress = xrmConfig.DiscoveryServiceAddress,
                Domain = xrmConfig.Domain,
                OrganizationUniqueName = OverrideOrganisation ?? xrmConfig.OrganizationUniqueName,
                Password = new Password(xrmConfig.Password, false, true),
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

        public void RecreatePortalData(bool createSecondDuplicateSite = false)
        {
            DeleteAll(Entities.adx_websitelanguage);
            DeleteAll(Entities.adx_webrole);
            DeleteAll(Entities.adx_webpage);
            DeleteAll(Entities.adx_webpageaccesscontrolrule);
            DeleteAll(Entities.adx_entityform);
            DeleteAll(Entities.adx_entityformmetadata);
            DeleteAll(Entities.adx_entitylist);
            DeleteAll(Entities.adx_webform);
            DeleteAll(Entities.adx_webformstep);
            DeleteAll(Entities.adx_webformmetadata);
            DeleteAll(Entities.adx_weblink);
            DeleteAll(Entities.adx_weblinkset);
            DeleteAll(Entities.adx_website);
            DeleteAll(Entities.adx_webfile);
            DeleteAll(Entities.adx_webtemplate);

            DeleteAll(Entities.adx_contentsnippet);
            DeleteAll(Entities.adx_entitypermission);
            DeleteAll(Entities.adx_pagetemplate);
            DeleteAll(Entities.adx_publishingstate);
            DeleteAll(Entities.adx_sitesetting);
            DeleteAll(Entities.adx_sitemarker);
            DeleteAll(Entities.adx_contentaccesslevel);

            var website1 = CreateTestRecord(Entities.adx_website, new Dictionary<string, object>
            {
                { Fields.adx_website_.adx_name, "Fake Site 1" }
            });
            CreateWebsiteRecords(website1);

            if (createSecondDuplicateSite)
            {
                var website2 = CreateTestRecord(Entities.adx_website, new Dictionary<string, object>
                {
                    { Fields.adx_website_.adx_name, "Fake Site 2" }
                });
                CreateWebsiteRecords(website2);
            }
        }

        private void CreateWebsiteRecords(Entity website)
        {
            var webFile = CreateTestRecord(Entities.adx_webfile, new Dictionary<string, object>
            {
                { Fields.adx_webfile_.adx_name, "Fake Web File.css" },
                { Fields.adx_webfile_.adx_websiteid, website.ToEntityReference() }
            });

            var file = Path.Combine(GetSolutionRootFolder().FullName, "SolutionItems", "TestFiles", "WEB FILE", "TESTDEPLOYINTO.css");
            var contentBytes = File.ReadAllBytes(file);
            var contentBase64String = Convert.ToBase64String(contentBytes);
            var webFileAttachment = CreateTestRecord(Entities.annotation, new Dictionary<string, object>
            {
                { Fields.annotation_.subject, "Fake Web File.css" },
                { Fields.annotation_.objectid, webFile.ToEntityReference() },
                { Fields.annotation_.documentbody, contentBase64String },
            });

            var webFile2 = CreateTestRecord(Entities.adx_webfile, new Dictionary<string, object>
            {
                { Fields.adx_webfile_.adx_name, "Fake Web File 2.css" },
                { Fields.adx_webfile_.adx_websiteid, website.ToEntityReference() }
            });

            var webTemplate1 = CreateTestRecord(Entities.adx_webtemplate, new Dictionary<string, object>
            {
                { Fields.adx_webtemplate_.adx_name, "Fake Web Template 1" },
                { Fields.adx_webtemplate_.adx_websiteid, website.ToEntityReference() },
                { Fields.adx_webtemplate_.adx_source, "<html><body><div>Web Template 1</div></body></html>" }
            });

            var webTemplate2 = CreateTestRecord(Entities.adx_webtemplate, new Dictionary<string, object>
            {
                { Fields.adx_webtemplate_.adx_name, "Fake Web Template 2" },
                { Fields.adx_webtemplate_.adx_websiteid, website.ToEntityReference() },
                { Fields.adx_webtemplate_.adx_source, "<html><body><div>Web Template 2</div></body></html>" }
            });

            var webTemplate3 = CreateTestRecord(Entities.adx_webtemplate, new Dictionary<string, object>
            {
                { Fields.adx_webtemplate_.adx_name, "Fake Web Template 3" },
                { Fields.adx_webtemplate_.adx_websiteid, website.ToEntityReference() },
            });

            var weblinkSet1 = CreateTestRecord(Entities.adx_weblinkset, new Dictionary<string, object>
            {
                { Fields.adx_weblinkset_.adx_name, "Fake Link Set 1" }
            });

            var weblinkSet2 = CreateTestRecord(Entities.adx_weblinkset, new Dictionary<string, object>
            {
                { Fields.adx_weblinkset_.adx_name, "Fake Link Set 2" }
            });

            var weblink1a = CreateTestRecord(Entities.adx_weblink, new Dictionary<string, object>
            {
                { Fields.adx_weblink_.adx_name, "Fake Link a" },
                { Fields.adx_weblink_.adx_weblinksetid, weblinkSet1.ToEntityReference() }
            });

            var weblink1b = CreateTestRecord(Entities.adx_weblink, new Dictionary<string, object>
            {
                { Fields.adx_weblink_.adx_name, "Fake Link b" },
                { Fields.adx_weblink_.adx_weblinksetid, weblinkSet1.ToEntityReference() }
            });

            var weblink2b = CreateTestRecord(Entities.adx_weblink, new Dictionary<string, object>
            {
                { Fields.adx_weblink_.adx_name, "Fake Link b" },
                { Fields.adx_weblink_.adx_weblinksetid, weblinkSet2.ToEntityReference() }
            });

            var weblink2c = CreateTestRecord(Entities.adx_weblink, new Dictionary<string, object>
            {
                { Fields.adx_weblink_.adx_name, "Fake Link c" },
                { Fields.adx_weblink_.adx_weblinksetid, weblinkSet2.ToEntityReference() }
            });

            var weblink2bc = CreateTestRecord(Entities.adx_weblink, new Dictionary<string, object>
            {
                { Fields.adx_weblink_.adx_name, "Fake Link bc" },
                { Fields.adx_weblink_.adx_parentweblinkid, weblink2c.ToEntityReference() }
            });

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
                { Fields.adx_webpage_.adx_name, "IScriptWebPage" },
                { Fields.adx_webpage_.adx_websiteid, website.ToEntityReference() },
                { Fields.adx_webpage_.adx_copy, "<div>Page Copy Parent</div>" }
            });
            var childWebPage = CreateTestRecord(Entities.adx_webpage, new Dictionary<string, object>
            {
                { Fields.adx_webpage_.adx_name, "IScriptWebPage" },
                { Fields.adx_webpage_.adx_websiteid, website.ToEntityReference() },
                { Fields.adx_webpage_.adx_rootwebpageid, webPage.ToEntityReference() },
                { Fields.adx_webpage_.adx_webpagelanguageid, websiteLanguage.ToEntityReference() },
                { Fields.adx_webpage_.adx_copy, "<div>Page Copy</div>" },
                { Fields.adx_webpage_.adx_customcss, ".class { color : white }" },
                { Fields.adx_webpage_.adx_customjavascript, "var blah = 'javascript'" },
            });
            var webpageAccessControlRule = CreateTestRecord(Entities.adx_webpageaccesscontrolrule, new Dictionary<string, object>
            {
                { Fields.adx_webpageaccesscontrolrule_.adx_name, "IScriptWebPage" },
                { Fields.adx_webpageaccesscontrolrule_.adx_webpageid, childWebPage.ToEntityReference() },
            });

            var entityForm = CreateTestRecord(Entities.adx_entityform, new Dictionary<string, object>
            {
                { Fields.adx_entityform_.adx_name, "IScriptEntityForm" },
                { Fields.adx_entityform_.adx_websiteid, website.ToEntityReference() },
                { Fields.adx_entityform_.adx_registerstartupscript, "var blah = 'entityform'" }
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

            var entityList = CreateTestRecord(Entities.adx_entitylist, new Dictionary<string, object>
            {
                { Fields.adx_entitylist_.adx_name, "IScriptEntityList" },
                { Fields.adx_entitylist_.adx_websiteid, website.ToEntityReference() },
                { Fields.adx_entitylist_.adx_registerstartupscript, "var blah = 'entityform'" }
            });

            var webForm = CreateTestRecord(Entities.adx_webform, new Dictionary<string, object>
            {
                { Fields.adx_webform_.adx_name, "IScriptWebForm" },
                { Fields.adx_webform_.adx_websiteid, website.ToEntityReference() },
            });

            var webFormStep = CreateTestRecord(Entities.adx_webformstep, new Dictionary<string, object>
            {
                { Fields.adx_webformstep_.adx_name, "IScriptWebFormStep1" },
                { Fields.adx_webformstep_.adx_webform, webForm.ToEntityReference() },
                { Fields.adx_webformstep_.adx_registerstartupscript, "var blah = 'web form step'" }
            });

            var webFormStep2 = CreateTestRecord(Entities.adx_webformstep, new Dictionary<string, object>
            {
                { Fields.adx_webformstep_.adx_name, "IScriptWebFormStep2" },
                { Fields.adx_webformstep_.adx_webform, webForm.ToEntityReference() },
                { Fields.adx_webformstep_.adx_registerstartupscript, "var blah = 'web form step 2'" }
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

            var contentSnippet = CreateTestRecord(Entities.adx_contentsnippet, new Dictionary<string, object>
            {
                { Fields.adx_contentsnippet_.adx_name, "IContentSnippet" },
            });

            var entityPermission = CreateTestRecord(Entities.adx_entitypermission, new Dictionary<string, object>
            {
                { Fields.adx_entitypermission_.adx_name, "IEntityPermission" },
            });

            var pageTemplate = CreateTestRecord(Entities.adx_pagetemplate, new Dictionary<string, object>
            {
                { Fields.adx_pagetemplate_.adx_name, "IPageTemplate" },
            });

            var publishingState = CreateTestRecord(Entities.adx_publishingstate, new Dictionary<string, object>
            {
                { Fields.adx_publishingstate_.adx_name, "IPublishingState" },
            });

            var siteSetting = CreateTestRecord(Entities.adx_sitesetting, new Dictionary<string, object>
            {
                { Fields.adx_sitesetting_.adx_name, "ISiteSetting" },
            });

            var sitemarker = CreateTestRecord(Entities.adx_sitemarker, new Dictionary<string, object>
            {
                { Fields.adx_sitemarker_.adx_name, "ISiteMarker" },
            });

            var contentAccessLevel = CreateTestRecord(Entities.adx_contentaccesslevel, new Dictionary<string, object>
            {
                { Fields.adx_contentaccesslevel_.adx_name, "IContentAccessLevel" },
            });

            XrmService.Associate(Relationships.adx_contentaccesslevel_.adx_WebRoleContentAccessLevel.Name, Fields.adx_contentaccesslevel_.adx_contentaccesslevelid, contentAccessLevel.Id, Fields.adx_webrole_.adx_webroleid, webRole.Id);
            XrmService.Associate(Relationships.adx_entitypermission_.adx_entitypermission_webrole.Name, Fields.adx_entitypermission_.adx_entitypermissionid, entityPermission.Id, Fields.adx_webrole_.adx_webroleid, webRole.Id);
            XrmService.Associate(Relationships.adx_publishingstate_.adx_accesscontrolrule_publishingstate.Name, Fields.adx_publishingstate_.adx_publishingstateid, publishingState.Id, Fields.adx_webpageaccesscontrolrule_.adx_webpageaccesscontrolruleid, webpageAccessControlRule.Id);
            XrmService.Associate(Relationships.adx_webpageaccesscontrolrule_.adx_webpageaccesscontrolrule_webrole.Name, Fields.adx_webpageaccesscontrolrule_.adx_webpageaccesscontrolruleid, webpageAccessControlRule.Id, Fields.adx_webrole_.adx_webroleid, webRole.Id);
        }
    }
}
