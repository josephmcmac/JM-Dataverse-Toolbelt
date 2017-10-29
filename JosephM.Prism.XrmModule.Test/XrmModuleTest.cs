using JosephM.Application.Application;
using JosephM.Application.Modules;
using JosephM.Application.ViewModel.Fakes;
using JosephM.Application.ViewModel.RecordEntry;
using JosephM.Application.ViewModel.RecordEntry.Form;
using JosephM.Core.AppConfig;
using JosephM.ObjectMapping;
using JosephM.Prism.Infrastructure.Module;
using JosephM.Prism.Infrastructure.Test;
using JosephM.Prism.XrmModule.SavedXrmConnections;
using JosephM.Prism.XrmModule.XrmConnection;
using JosephM.Record.Xrm.Test;
using JosephM.Record.Xrm.XrmRecord;
using Microsoft.Xrm.Sdk.Client;
using JosephM.Record.IService;
using JosephM.Prism.XrmModule.Crud;

namespace JosephM.Prism.XrmModule.Test
{
    public class XrmModuleTest : XrmRecordTest
    {
        protected XrmModuleTest()
            : base()
        {
            XrmRecordService.SetFormService(new XrmFormService());
        }

        protected virtual TestApplication CreateAndLoadTestApplication<TModule>(ApplicationControllerBase applicationController = null, ISettingsManager settingsManager = null)
            where TModule : ModuleBase, new()
        {
            var testApplication = TestApplication.CreateTestApplication(applicationController, settingsManager);
            testApplication.AddModule<TModule>();
            XrmConnectionModule.RefreshXrmServices(GetXrmRecordConfiguration(), testApplication.Controller);
            testApplication.Controller.RegisterInstance<ISavedXrmConnections>(new SavedXrmConnections.SavedXrmConnections
            {
                Connections = new[] { GetSavedXrmRecordConfiguration() }
            });
            return testApplication;
        }

        public FakeXrmApplicationController CreateFakeApplicationController()
        {
            var savedConfig = GetSavedXrmRecordConfiguration();
            var savedConfigs = new SavedXrmConnections.SavedXrmConnections()
            {
                Connections = new[] { savedConfig }
            };
            return new FakeXrmApplicationController(savedConfigs);
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
                Username = xrmConfig.Username
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
                Username = saved.Username
            };
        }

        public ObjectEntryViewModel CreateObjectEntryViewModel(object viewModelObject, IApplicationController applicationController)
        {
            return CreateObjectEntryViewModel(viewModelObject, applicationController, XrmRecordService);
        }

        public ObjectEntryViewModel CreateObjectEntryViewModel(object viewModelObject, IApplicationController applicationController, IRecordService lookupService)
        {
            var viewModel = new ObjectEntryViewModel(() => { }, () => { }, viewModelObject,
                FormController.CreateForObject(viewModelObject, applicationController, lookupService));
            viewModel.LoadFormSections();
            return viewModel;
        }

        public ObjectEntryViewModel CreateObjectEntryViewModel(object viewModelObject)
        {
            return CreateObjectEntryViewModel(viewModelObject, new FakeApplicationController());
        }
    }
}
