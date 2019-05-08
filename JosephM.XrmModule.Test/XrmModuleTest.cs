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
using System.Linq;

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

        public void RecreatePortalData(bool createSecondDuplicateSite = false, XrmRecordService useRecordService = null)
        {
            useRecordService = useRecordService ?? XrmRecordService;

            DeleteAllPortalData(useRecordService: useRecordService);

            var website1 = CreateTestRecord(Entities.adx_website, new Dictionary<string, object>
            {
                { Fields.adx_website_.adx_name, "Fake Site 1" }
            }, useService: useRecordService.XrmService);
            CreateWebsiteRecords(website1, useRecordService: useRecordService);

            if (createSecondDuplicateSite)
            {
                var website2 = CreateTestRecord(Entities.adx_website, new Dictionary<string, object>
                {
                    { Fields.adx_website_.adx_name, "Fake Site 2" }
                }, useService: useRecordService.XrmService);
                CreateWebsiteRecords(website2, useRecordService: useRecordService);
            }
        }

        public void DeleteAllPortalData(IEnumerable<string> dontDeleteTypes = null, XrmRecordService useRecordService = null)
        {
            var typesToDelete = new[]
            {
                Entities.adx_websitelanguage,
                Entities.adx_webrole,
                Entities.adx_webpage,
                Entities.adx_webpageaccesscontrolrule,
                Entities.adx_entityform,
                Entities.adx_entityformmetadata,
                Entities.adx_entitylist,
                Entities.adx_webform,
                Entities.adx_webformstep,
                Entities.adx_webformmetadata,
                Entities.adx_weblink,
                Entities.adx_weblinkset,
                Entities.adx_website,
                Entities.adx_webfile,
                Entities.adx_webtemplate,
                Entities.adx_contentsnippet,
                Entities.adx_entitypermission,
                Entities.adx_pagetemplate,
                Entities.adx_publishingstate,
                Entities.adx_sitesetting,
                Entities.adx_sitemarker,
                Entities.adx_contentaccesslevel,
            };
            if(dontDeleteTypes != null)
            {
                typesToDelete = typesToDelete.Except(dontDeleteTypes).ToArray();
            }
            foreach(var type in typesToDelete)
            {
                DeleteAll(type, serviceToUse: useRecordService.XrmService);
            }
        }

        private void CreateWebsiteRecords(Entity website, XrmRecordService useRecordService = null)
        {
            useRecordService = useRecordService ?? XrmRecordService;

            var webFile = CreateTestRecord(Entities.adx_webfile, new Dictionary<string, object>
            {
                { Fields.adx_webfile_.adx_websiteid, website.ToEntityReference() },
                { Fields.adx_webfile_.adx_name, "Fake Web File.css" },
            }, useService: useRecordService.XrmService);

            var file = Path.Combine(GetSolutionRootFolder().FullName, "SolutionItems", "TestFiles", "WEB FILE", "TESTDEPLOYINTO.css");
            var contentBytes = File.ReadAllBytes(file);
            var contentBase64String = Convert.ToBase64String(contentBytes);
            var webFileAttachment = CreateTestRecord(Entities.annotation, new Dictionary<string, object>
            {
                { Fields.annotation_.subject, "Fake Web File.css" },
                { Fields.annotation_.objectid, webFile.ToEntityReference() },
                { Fields.annotation_.documentbody, contentBase64String },
            }, useService: useRecordService.XrmService);

            var webFile2 = CreateTestRecord(Entities.adx_webfile, new Dictionary<string, object>
            {
                { Fields.adx_webfile_.adx_websiteid, website.ToEntityReference() },
                { Fields.adx_webfile_.adx_name, "Fake Web File 2.css" },
            }, useService: useRecordService.XrmService);

            var webTemplate1 = CreateTestRecord(Entities.adx_webtemplate, new Dictionary<string, object>
            {
                { Fields.adx_webtemplate_.adx_name, "Fake Web Template 1" },
                { Fields.adx_webtemplate_.adx_websiteid, website.ToEntityReference() },
                { Fields.adx_webtemplate_.adx_source, "<html><body><div>Web Template 1</div></body></html>" }
            }, useService: useRecordService.XrmService);

            var webTemplate2 = CreateTestRecord(Entities.adx_webtemplate, new Dictionary<string, object>
            {
                { Fields.adx_webtemplate_.adx_name, "Fake Web Template 2" },
                { Fields.adx_webtemplate_.adx_websiteid, website.ToEntityReference() },
                { Fields.adx_webtemplate_.adx_source, "<html><body><div>Web Template 2</div></body></html>" }
            }, useService: useRecordService.XrmService);

            var webTemplate3 = CreateTestRecord(Entities.adx_webtemplate, new Dictionary<string, object>
            {
                { Fields.adx_webtemplate_.adx_name, "Fake Web Template 3" },
                { Fields.adx_webtemplate_.adx_websiteid, website.ToEntityReference() },
            }, useService: useRecordService.XrmService);

            var weblinkSet1 = CreateTestRecord(Entities.adx_weblinkset, new Dictionary<string, object>
            {
                { Fields.adx_weblinkset_.adx_websiteid, website.ToEntityReference() },
                { Fields.adx_weblinkset_.adx_name, "Fake Link Set 1" }
            }, useService: useRecordService.XrmService);

            var weblinkSet2 = CreateTestRecord(Entities.adx_weblinkset, new Dictionary<string, object>
            {
                { Fields.adx_weblinkset_.adx_websiteid, website.ToEntityReference() },
                { Fields.adx_weblinkset_.adx_name, "Fake Link Set 2" }
            }, useService: useRecordService.XrmService);

            var weblink1a = CreateTestRecord(Entities.adx_weblink, new Dictionary<string, object>
            {
                { Fields.adx_weblink_.adx_name, "Fake Link a" },
                { Fields.adx_weblink_.adx_weblinksetid, weblinkSet1.ToEntityReference() }
            }, useService: useRecordService.XrmService);

            var weblink1b = CreateTestRecord(Entities.adx_weblink, new Dictionary<string, object>
            {
                { Fields.adx_weblink_.adx_name, "Fake Link b" },
                { Fields.adx_weblink_.adx_weblinksetid, weblinkSet1.ToEntityReference() }
            }, useService: useRecordService.XrmService);

            var weblink2b = CreateTestRecord(Entities.adx_weblink, new Dictionary<string, object>
            {
                { Fields.adx_weblink_.adx_name, "Fake Link b" },
                { Fields.adx_weblink_.adx_weblinksetid, weblinkSet2.ToEntityReference() }
            }, useService: useRecordService.XrmService);

            var weblink2c = CreateTestRecord(Entities.adx_weblink, new Dictionary<string, object>
            {
                { Fields.adx_weblink_.adx_name, "Fake Link c" },
                { Fields.adx_weblink_.adx_weblinksetid, weblinkSet2.ToEntityReference() }
            }, useService: useRecordService.XrmService);

            var weblink2bc = CreateTestRecord(Entities.adx_weblink, new Dictionary<string, object>
            {
                { Fields.adx_weblink_.adx_name, "Fake Link bc" },
                { Fields.adx_weblink_.adx_parentweblinkid, weblink2c.ToEntityReference() }
            }, useService: useRecordService.XrmService);

            var websiteLanguage = CreateTestRecord(Entities.adx_websitelanguage, new Dictionary<string, object>
            {
                { Fields.adx_websitelanguage_.adx_name, "English" }
            }, useService: useRecordService.XrmService);

            var webRole = CreateTestRecord(Entities.adx_webrole, new Dictionary<string, object>
            {
                { Fields.adx_webrole_.adx_websiteid, website.ToEntityReference() },
                { Fields.adx_webrole_.adx_name, "TestScriptRole" }
            }, useService: useRecordService.XrmService);

            var webPage = CreateTestRecord(Entities.adx_webpage, new Dictionary<string, object>
            {
                { Fields.adx_webpage_.adx_name, "IScriptWebPage" },
                { Fields.adx_webpage_.adx_websiteid, website.ToEntityReference() },
                { Fields.adx_webpage_.adx_copy, "<div>Page Copy Parent</div>" }
            }, useService: useRecordService.XrmService);
            var childWebPage = CreateTestRecord(Entities.adx_webpage, new Dictionary<string, object>
            {
                { Fields.adx_webpage_.adx_name, "IScriptWebPage" },
                { Fields.adx_webpage_.adx_websiteid, website.ToEntityReference() },
                { Fields.adx_webpage_.adx_rootwebpageid, webPage.ToEntityReference() },
                { Fields.adx_webpage_.adx_webpagelanguageid, websiteLanguage.ToEntityReference() },
                { Fields.adx_webpage_.adx_copy, "<div>Page Copy</div>" },
                { Fields.adx_webpage_.adx_customcss, ".class { color : white }" },
                { Fields.adx_webpage_.adx_customjavascript, "var blah = 'javascript'" },
            }, useService: useRecordService.XrmService);
            var webpageAccessControlRule = CreateTestRecord(Entities.adx_webpageaccesscontrolrule, new Dictionary<string, object>
            {
                { Fields.adx_webpageaccesscontrolrule_.adx_name, "IScriptWebPage" },
                { Fields.adx_webpageaccesscontrolrule_.adx_webpageid, childWebPage.ToEntityReference() },
            }, useService: useRecordService.XrmService);

            var entityForm = CreateTestRecord(Entities.adx_entityform, new Dictionary<string, object>
            {
                { Fields.adx_entityform_.adx_name, "IScriptEntityForm" },
                { Fields.adx_entityform_.adx_websiteid, website.ToEntityReference() },
                { Fields.adx_entityform_.adx_registerstartupscript, "var blah = 'entityform'" }
            }, useService: useRecordService.XrmService);

            var entityFormMetadata1 = CreateTestRecord(Entities.adx_entityformmetadata, new Dictionary<string, object>
            {
                { Fields.adx_entityform_.adx_name, null },
                { Fields.adx_entityformmetadata_.adx_entityform, entityForm.ToEntityReference() },
                { Fields.adx_entityformmetadata_.adx_type, new OptionSetValue(OptionSets.EntityFormMetadata.Type.Attribute) },
                { Fields.adx_entityformmetadata_.adx_attributelogicalname, "foo" },
            }, useService: useRecordService.XrmService);

            var entityFormMetadata2 = CreateTestRecord(Entities.adx_entityformmetadata, new Dictionary<string, object>
            {
                { Fields.adx_entityform_.adx_name, null },
                { Fields.adx_entityformmetadata_.adx_entityform, entityForm.ToEntityReference() },
                { Fields.adx_entityformmetadata_.adx_type, new OptionSetValue(OptionSets.EntityFormMetadata.Type.Attribute) },
                { Fields.adx_entityformmetadata_.adx_attributelogicalname, "bar" },
            }, useService: useRecordService.XrmService);

            var entityFormMetadata3 = CreateTestRecord(Entities.adx_entityformmetadata, new Dictionary<string, object>
            {
                { Fields.adx_entityform_.adx_name, null },
                { Fields.adx_entityformmetadata_.adx_entityform, entityForm.ToEntityReference() },
                { Fields.adx_entityformmetadata_.adx_type, new OptionSetValue(OptionSets.EntityFormMetadata.Type.Notes) }
            }, useService: useRecordService.XrmService);

            var entityList = CreateTestRecord(Entities.adx_entitylist, new Dictionary<string, object>
            {
                { Fields.adx_entitylist_.adx_name, "IScriptEntityList" },
                { Fields.adx_entitylist_.adx_websiteid, website.ToEntityReference() },
                { Fields.adx_entitylist_.adx_registerstartupscript, "var blah = 'entityform'" }
            }, useService: useRecordService.XrmService);

            var webForm = CreateTestRecord(Entities.adx_webform, new Dictionary<string, object>
            {
                { Fields.adx_webform_.adx_name, "IScriptWebForm" },
                { Fields.adx_webform_.adx_websiteid, website.ToEntityReference() },
            }, useService: useRecordService.XrmService);

            var webFormStep = CreateTestRecord(Entities.adx_webformstep, new Dictionary<string, object>
            {
                { Fields.adx_webformstep_.adx_name, "IScriptWebFormStep1" },
                { Fields.adx_webformstep_.adx_webform, webForm.ToEntityReference() },
                { Fields.adx_webformstep_.adx_registerstartupscript, "var blah = 'web form step'" }
            }, useService: useRecordService.XrmService);

            var webFormStep2 = CreateTestRecord(Entities.adx_webformstep, new Dictionary<string, object>
            {
                { Fields.adx_webformstep_.adx_name, "IScriptWebFormStep2" },
                { Fields.adx_webformstep_.adx_webform, webForm.ToEntityReference() },
                { Fields.adx_webformstep_.adx_registerstartupscript, "var blah = 'web form step 2'" }
            }, useService: useRecordService.XrmService);

            var webFormMetadata1 = CreateTestRecord(Entities.adx_webformmetadata, new Dictionary<string, object>
            {
                { Fields.adx_webformmetadata_.adx_webformstep, webFormStep.ToEntityReference() },
                { Fields.adx_webformmetadata_.adx_type, new OptionSetValue(OptionSets.WebFormMetadata.Type.Attribute) },
                { Fields.adx_webformmetadata_.adx_attributelogicalname, "foo" },
            }, useService: useRecordService.XrmService);

            var webFormMetadata2 = CreateTestRecord(Entities.adx_webformmetadata, new Dictionary<string, object>
            {
                { Fields.adx_webformmetadata_.adx_webformstep, webFormStep.ToEntityReference() },
                { Fields.adx_webformmetadata_.adx_type, new OptionSetValue(OptionSets.WebFormMetadata.Type.Attribute) },
                { Fields.adx_webformmetadata_.adx_attributelogicalname, "bar" },
            }, useService: useRecordService.XrmService);

            var webFormMetadata3 = CreateTestRecord(Entities.adx_webformmetadata, new Dictionary<string, object>
            {
                { Fields.adx_webformmetadata_.adx_attributelogicalname, null },
                { Fields.adx_webformmetadata_.adx_webformstep, webFormStep.ToEntityReference() },
                { Fields.adx_webformmetadata_.adx_type, new OptionSetValue(OptionSets.WebFormMetadata.Type.Notes) }
            }, useService: useRecordService.XrmService);

            var webFormMetadata4 = CreateTestRecord(Entities.adx_webformmetadata, new Dictionary<string, object>
            {
                { Fields.adx_webformmetadata_.adx_attributelogicalname, null },
                { Fields.adx_webformmetadata_.adx_webformstep, webFormStep2.ToEntityReference() },
                { Fields.adx_webformmetadata_.adx_type, new OptionSetValue(OptionSets.WebFormMetadata.Type.Notes) }
            }, useService: useRecordService.XrmService);

            var contentSnippet = CreateTestRecord(Entities.adx_contentsnippet, new Dictionary<string, object>
            {
                { Fields.adx_contentsnippet_.adx_websiteid, website.ToEntityReference() },
                { Fields.adx_contentsnippet_.adx_name, "IContentSnippet" },
            }, useService: useRecordService.XrmService);

            var entityPermission = CreateTestRecord(Entities.adx_entitypermission, new Dictionary<string, object>
            {
                { Fields.adx_entitypermission_.adx_websiteid, website.ToEntityReference() },
                { Fields.adx_entitypermission_.adx_name, "IEntityPermission" },
            }, useService: useRecordService.XrmService);

            var pageTemplate = CreateTestRecord(Entities.adx_pagetemplate, new Dictionary<string, object>
            {
                { Fields.adx_pagetemplate_.adx_websiteid, website.ToEntityReference() },
                { Fields.adx_pagetemplate_.adx_name, "IPageTemplate" },
            }, useService: useRecordService.XrmService);

            var publishingState = CreateTestRecord(Entities.adx_publishingstate, new Dictionary<string, object>
            {
                { Fields.adx_publishingstate_.adx_websiteid, website.ToEntityReference() },
                { Fields.adx_publishingstate_.adx_name, "IPublishingState" },
            }, useService: useRecordService.XrmService);

            var siteSetting = CreateTestRecord(Entities.adx_sitesetting, new Dictionary<string, object>
            {
                { Fields.adx_sitesetting_.adx_websiteid, website.ToEntityReference() },
                { Fields.adx_sitesetting_.adx_name, "ISiteSetting" },
            }, useService: useRecordService.XrmService);

            var sitemarker = CreateTestRecord(Entities.adx_sitemarker, new Dictionary<string, object>
            {
                { Fields.adx_sitemarker_.adx_websiteid, website.ToEntityReference() },
                { Fields.adx_sitemarker_.adx_name, "ISiteMarker" },
            }, useService: useRecordService.XrmService);

            var contentAccessLevel = useRecordService.XrmService.GetFirst(Entities.adx_contentaccesslevel, Fields.adx_contentaccesslevel_.adx_name, "IContentAccessLevel");
            if (contentAccessLevel == null)
            {
                contentAccessLevel = CreateTestRecord(Entities.adx_contentaccesslevel, new Dictionary<string, object>
                {
                    { Fields.adx_contentaccesslevel_.adx_name, "IContentAccessLevel" },
                }, useService: useRecordService.XrmService);
            }

            useRecordService.XrmService.Associate(Relationships.adx_contentaccesslevel_.adx_WebRoleContentAccessLevel.Name, Fields.adx_contentaccesslevel_.adx_contentaccesslevelid, contentAccessLevel.Id, Fields.adx_webrole_.adx_webroleid, webRole.Id);
            useRecordService.XrmService.Associate(Relationships.adx_entitypermission_.adx_entitypermission_webrole.Name, Fields.adx_entitypermission_.adx_entitypermissionid, entityPermission.Id, Fields.adx_webrole_.adx_webroleid, webRole.Id);
            useRecordService.XrmService.Associate(Relationships.adx_publishingstate_.adx_accesscontrolrule_publishingstate.Name, Fields.adx_publishingstate_.adx_publishingstateid, publishingState.Id, Fields.adx_webpageaccesscontrolrule_.adx_webpageaccesscontrolruleid, webpageAccessControlRule.Id);
            useRecordService.XrmService.Associate(Relationships.adx_webpageaccesscontrolrule_.adx_webpageaccesscontrolrule_webrole.Name, Fields.adx_webpageaccesscontrolrule_.adx_webpageaccesscontrolruleid, webpageAccessControlRule.Id, Fields.adx_webrole_.adx_webroleid, webRole.Id);
        }
    }
}
