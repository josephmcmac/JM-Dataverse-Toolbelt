using JosephM.Application.Application;
using JosephM.Application.Modules;
using JosephM.Application.ViewModel.Extentions;
using JosephM.Application.ViewModel.RecordEntry.Form;
using JosephM.Core.AppConfig;
using JosephM.Core.Attributes;
using JosephM.Core.Extentions;
using JosephM.Core.Log;
using JosephM.Record.Service;
using JosephM.Record.Xrm.Mappers;
using JosephM.Record.Xrm.XrmRecord;
using JosephM.XrmModule.Crud;
using System;
using System.Configuration;
using System.Linq;

namespace JosephM.XrmModule.XrmConnection
{
    [MyDescription("Connect To A CRM Instance")]
    public class XrmConnectionModule : ActionModuleBase
    {

        public override void InitialiseModule()
        {
            AddSetting(MainOperationName, DialogCommand, OperationDescription);
        }

        public override string MainOperationName => "Connect To Crm";

        public override void DialogCommand()
        {
            NavigateTo<XrmConnectionDialog>();
        }

        public override void RegisterTypes()
        {
            var configManager = Resolve<ISettingsManager>();

            try
            {
                var xrmConfiguration = configManager.Resolve<XrmRecordConfiguration>();
                RefreshXrmServices(xrmConfiguration, ApplicationController);
            }
            catch (ConfigurationErrorsException ex)
            {
                ApplicationController.UserMessage(
                    string.Concat("Warning!! There was an error reading the crm connection from config\n",
                        ex.DisplayString()));
            }

            RegisterTypeForNavigation<XrmConnectionDialog>();

            AddConnectionFieldsAutocomplete();
        }

        private static IXrmRecordConfiguration LastXrmConfiguration { get; set; }

        public static void RefreshXrmServices(IXrmRecordConfiguration xrmConfiguration, IApplicationController controller, XrmRecordService xrmRecordService = null)
        {
            controller.RegisterInstance<IXrmRecordConfiguration>(xrmConfiguration);
            xrmRecordService = xrmRecordService ?? new XrmRecordService(xrmConfiguration, controller.ResolveType<LogController>(), formService: new XrmFormService());
            xrmRecordService.XrmRecordConfiguration = xrmConfiguration;
            controller.RegisterInstance(xrmRecordService);
            LastXrmConfiguration = xrmConfiguration;
            if (xrmConfiguration.OrganizationUniqueName == null)
                controller.AddNotification("XRMCONNECTION", "No Active Connection");
            else if (controller.RunThreadsAsynch)
            {
                controller.DoOnAsyncThread(() =>
                {
                    try
                    {
                        controller.AddNotification("XRMCONNECTION", $"Connecting To '{xrmConfiguration}'", isLoading: true);
                        var verify = xrmRecordService.VerifyConnection();
                        if (LastXrmConfiguration != xrmConfiguration)
                            return;
                        if (verify.IsValid)
                        {
                            controller.AddNotification("XRMCONNECTION", string.Format("Connected To '{0}'", xrmConfiguration));
                            var preLoadRecordTypes = xrmRecordService.GetAllRecordTypes();
                        }
                        else
                        {
                            controller.AddNotification("XRMCONNECTION", string.Format("Error Connecting To '{0}'", xrmConfiguration));
                        }
                    }
                    catch (Exception ex)
                    {
                        if (LastXrmConfiguration != xrmConfiguration)
                            return;
                        controller.AddNotification("XRMCONNECTION", ex.Message);
                        controller.ThrowException(ex);
                    }
                });
            }
        }

        private void AddConnectionFieldsAutocomplete()
        {
            //existing values + the standard online regional endpoints
            var onlineDiscoveryServices = new[]
            {
                new AutocompleteOption("North America", "https://disco.crm.dynamics.com/XRMServices/2011/Discovery.svc"),
                new AutocompleteOption("North America 2", "https://disco.crm9.dynamics.com/XRMServices/2011/Discovery.svc"),
                new AutocompleteOption("EMEA", "https://disco.crm4.dynamics.com/XRMServices/2011/Discovery.svc"),
                new AutocompleteOption("Asia Pacific Area", "https://disco.crm5.dynamics.com/XRMServices/2011/Discovery.svc"),
                new AutocompleteOption("Oceania", "https://disco.crm6.dynamics.com/XRMServices/2011/Discovery.svc"),
                new AutocompleteOption("Japan", "https://disco.crm7.dynamics.com/XRMServices/2011/Discovery.svc"),
                new AutocompleteOption("South America", "https://disco.crm2.dynamics.com/XRMServices/2011/Discovery.svc"),
                new AutocompleteOption("India", "https://disco.crm8.dynamics.com/XRMServices/2011/Discovery.svc"),
                new AutocompleteOption("Canada", "https://disco.crm3.dynamics.com/XRMServices/2011/Discovery.svc"),
                new AutocompleteOption("United Kingdom", "https://disco.crm11.dynamics.com/XRMServices/2011/Discovery.svc"),
            };
            this.AddAutocompleteFunction(new AutocompleteFunction((recordForm) =>
            {
                return onlineDiscoveryServices;
            }, displayInGrid: false, displayNames: true), typeof(XrmRecordConfiguration), nameof(XrmRecordConfiguration.DiscoveryServiceAddress));

            //get the organisations based on details entered
            this.AddAutocompleteFunction(new AutocompleteFunction((recordForm) =>
            {
                var objectRecord = recordForm.GetRecord() as ObjectRecord;
                if (objectRecord == null)
                    return null;
                var thisConnectionEntered = objectRecord.Instance as XrmRecordConfiguration;
                if (thisConnectionEntered == null)
                    return null;
                var xrmRecordConfiguration = new XrmRecordConfigurationInterfaceMapper().Map(thisConnectionEntered);
                var xrmConfiguration = new XrmConfigurationMapper().Map(xrmRecordConfiguration);
                var xrmConnection = new Xrm.XrmConnection(xrmConfiguration);
                return xrmConnection
                    .GetActiveOrganisations()
                    .Select(org => new AutocompleteOption(org.FriendlyName, org.UniqueName));
            }, displayInGrid: false, autosearch: false, displayNames: true), typeof(XrmRecordConfiguration), nameof(XrmRecordConfiguration.OrganizationUniqueName));
        }
    }
}