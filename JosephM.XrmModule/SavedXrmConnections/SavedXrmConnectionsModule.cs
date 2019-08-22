using JosephM.Application.Application;
using JosephM.Application.Desktop.Module.Settings;
using JosephM.Application.ViewModel.Dialog;
using JosephM.Application.ViewModel.Extentions;
using JosephM.Application.ViewModel.Grid;
using JosephM.Application.ViewModel.RecordEntry.Form;
using JosephM.Application.ViewModel.RecordEntry.Metadata;
using JosephM.Core.AppConfig;
using JosephM.Core.Attributes;
using JosephM.Core.Extentions;
using JosephM.Core.Log;
using JosephM.Record.Service;
using JosephM.Record.Xrm.Mappers;
using JosephM.Record.Xrm.XrmRecord;
using JosephM.XrmModule.Crud;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;

namespace JosephM.XrmModule.SavedXrmConnections
{
    [MyDescription("Multiple Saved Connections To CRM Instances")]
    public class SavedXrmConnectionsModule : SettingsModule<SavedXrmConnectionsDialog, ISavedXrmConnections, SavedXrmConnections>
    {
        public override string MainOperationName => "Saved Connections";

        public override void InitialiseModule()
        {
            base.InitialiseModule();
        }

        public override void RegisterTypes()
        {
            var configManager = Resolve<ISettingsManager>();
            configManager.ProcessNamespaceChange(GetType().Namespace, "JosephM.Prism.XrmModule.SavedXrmConnections");
            base.RegisterTypes();

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
            AddWebBrowseGridFunction();
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
                RefreshConnectionNotification(controller, "No Active Connection");
            else if (controller.RunThreadsAsynch)
            {
                controller.DoOnAsyncThread(() =>
                {
                    try
                    {
                        RefreshConnectionNotification(controller, $"Connecting To '{xrmConfiguration}'", isLoading: true);
                        var verify = xrmRecordService.VerifyConnection();
                        if (LastXrmConfiguration != xrmConfiguration)
                            return;
                        if (verify.IsValid)
                        {
                            RefreshConnectionNotification(controller, string.Format("Connected To '{0}'", xrmConfiguration));
                            var preLoadRecordTypes = xrmRecordService.GetAllRecordTypes();
                        }
                        else
                        {
                            RefreshConnectionNotification(controller, string.Format("Error Connecting To '{0}'", xrmConfiguration));
                        }
                    }
                    catch (Exception ex)
                    {
                        if (LastXrmConfiguration != xrmConfiguration)
                            return;
                        RefreshConnectionNotification(controller, ex.Message);
                        controller.ThrowException(ex);
                    }
                });
            }
        }

        private static void RefreshConnectionNotification(IApplicationController controller, string message, bool isLoading = false)
        {
            var actions = new Dictionary<string, Action>();
            actions.Add("Create New", () =>
            {
                var dialog = new AppXrmConnectionEntryDialog(controller.ResolveType<IDialogController>());
                controller.NavigateTo(dialog);
            });
            var savedConnections = controller.ResolveType<ISavedXrmConnections>();
            if(savedConnections.Connections != null)
            {
                foreach (var connection in savedConnections.Connections.OrderBy(c => c.Name).ToArray())
                {
                    if (!string.IsNullOrWhiteSpace(connection.Name)
                        && !connection.Active
                        && !actions.ContainsKey(connection.Name))
                    {
                        actions.Add(connection.Name, () =>
                        {
                            controller.DoOnAsyncThread(() =>
                            {
                                var appSettingsManager = controller.ResolveType(typeof(ISettingsManager)) as ISettingsManager;
                                var recordconfig = new ObjectMapping.ClassMapperFor<SavedXrmRecordConfiguration, XrmRecordConfiguration>().Map(connection);
                                appSettingsManager.SaveSettingsObject(recordconfig);
                                savedConnections = controller.ResolveType<ISavedXrmConnections>();
                                foreach (var item in savedConnections.Connections.OrderBy(c => c.Name).ToArray())
                                {
                                    item.Active = item.Name == connection.Name;
                                }
                                appSettingsManager.SaveSettingsObject(savedConnections);
                                RefreshXrmServices(connection, controller);
                            });
                        });
                    }
                }       
            }
            controller.AddNotification("XRMCONNECTION", message, isLoading: isLoading, actions: actions);
        }

        private void AddConnectionFieldsAutocomplete()
        {
            //existing values for these
            var propertiesForAutocompleteExistingValues = new[]
            {
                new KeyValuePair<string, double>(nameof(SavedXrmRecordConfiguration.Domain), 150),
                new KeyValuePair<string, double>(nameof(SavedXrmRecordConfiguration.Username), 400),
            };
            foreach (var prop in propertiesForAutocompleteExistingValues)
            {
                Func<RecordEntryViewModelBase, IEnumerable<AutocompleteOption>> getExistingValues = (recordForm) =>
                {
                    var parentForm = recordForm.ParentForm;
                    if (parentForm != null)
                    {
                        var objectRecord = parentForm.GetRecord() as ObjectRecord;
                        if (objectRecord != null)
                        {
                            var instance = objectRecord.Instance as ISavedXrmConnections;
                            if (instance != null && instance.Connections != null)
                            {
                                return instance
                                   .Connections
                                   .Select(pt => (string)pt.GetPropertyValue(prop.Key))
                                   .Where(g => !string.IsNullOrWhiteSpace(g))
                                   .Distinct()
                                   .Select(s => new AutocompleteOption(s))
                                   .ToArray();
                            }
                        }
                    }
                    var savedConnections = ApplicationController.ResolveType<ISavedXrmConnections>();
                    if (savedConnections != null && savedConnections.Connections != null)
                    {
                        return savedConnections
                           .Connections
                           .Select(pt => (string)pt.GetPropertyValue(prop.Key))
                           .Where(g => !string.IsNullOrWhiteSpace(g))
                           .Distinct()
                           .Select(s => new AutocompleteOption(s))
                           .ToArray();
                    }
                    return new AutocompleteOption[0];
                };
                this.AddAutocompleteFunction(new AutocompleteFunction(getExistingValues, gridWidth: prop.Value
                    , isValidForFormFunction: (f) => getExistingValues(f).Any(), displayInGrid: false), typeof(SavedXrmRecordConfiguration), prop.Key);
            }

            //existing values + the standard online regional endpoints
            var onlineDiscoveryServices = new[]
            {
                new DiscoveryAutocomplete("North America", "https://disco.crm.dynamics.com/XRMServices/2011/Discovery.svc"),
                new DiscoveryAutocomplete("North America 2", "https://disco.crm9.dynamics.com/XRMServices/2011/Discovery.svc"),
                new DiscoveryAutocomplete("EMEA", "https://disco.crm4.dynamics.com/XRMServices/2011/Discovery.svc"),
                new DiscoveryAutocomplete("Asia Pacific Area", "https://disco.crm5.dynamics.com/XRMServices/2011/Discovery.svc"),
                new DiscoveryAutocomplete("Oceania", "https://disco.crm6.dynamics.com/XRMServices/2011/Discovery.svc"),
                new DiscoveryAutocomplete("Japan", "https://disco.crm7.dynamics.com/XRMServices/2011/Discovery.svc"),
                new DiscoveryAutocomplete("South America", "https://disco.crm2.dynamics.com/XRMServices/2011/Discovery.svc"),
                new DiscoveryAutocomplete("India", "https://disco.crm8.dynamics.com/XRMServices/2011/Discovery.svc"),
                new DiscoveryAutocomplete("Canada", "https://disco.crm3.dynamics.com/XRMServices/2011/Discovery.svc"),
                new DiscoveryAutocomplete("United Kingdom", "https://disco.crm11.dynamics.com/XRMServices/2011/Discovery.svc"),
            };
            this.AddAutocompleteFunction(new AutocompleteFunction((recordForm) =>
            {
                var parentForm = recordForm.ParentForm;
                if (parentForm == null)
                    return onlineDiscoveryServices;
                var objectRecord = parentForm.GetRecord() as ObjectRecord;
                if (objectRecord == null)
                    return onlineDiscoveryServices;
                var instance = objectRecord.Instance as ISavedXrmConnections;
                var otherSavedConnections = instance == null || instance.Connections == null
                    ? new string[0]
                    : instance
                    .Connections
                    .Select(pt => (string)pt.GetPropertyValue(nameof(SavedXrmRecordConfiguration.DiscoveryServiceAddress)))
                    .Where(g => !string.IsNullOrWhiteSpace(g) && !onlineDiscoveryServices.Any(sv => sv.Value.ToLower() == g.ToLower()));
                return onlineDiscoveryServices
                    .Union(otherSavedConnections.Select(s => new DiscoveryAutocomplete(" ", s)));
            }, typeof(DiscoveryAutocomplete), nameof(DiscoveryAutocomplete.Value), new[] { new GridFieldMetadata(nameof(DiscoveryAutocomplete.Name), 120), new GridFieldMetadata(nameof(DiscoveryAutocomplete.Value), 450) }, sortField: nameof(DiscoveryAutocomplete.Name), displayInGrid: false), typeof(SavedXrmRecordConfiguration), nameof(SavedXrmRecordConfiguration.DiscoveryServiceAddress));

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
                    .GetActiveOrganisations(); ;
            }, typeof(Xrm.Organisation), nameof(Xrm.Organisation.UniqueName), new[] { new GridFieldMetadata(nameof(Xrm.Organisation.UniqueName), 100), new GridFieldMetadata(nameof(Xrm.Organisation.FriendlyName), 400) }, sortField: nameof(Xrm.Organisation.FriendlyName), displayInGrid: false, autosearch: false), typeof(SavedXrmRecordConfiguration), nameof(SavedXrmRecordConfiguration.OrganizationUniqueName));
        }

        private void AddWebBrowseGridFunction()
        {
            var customGridFunction = new CustomGridFunction("WEB", "Open In Web", (g) =>
            {
                if (g.SelectedRows.Count() != 1)
                {
                    g.ApplicationController.UserMessage("Please Select One Row To Browse The Connection");
                }
                else
                {
                    var selectedRow = g.SelectedRows.First();
                    var instance = ((ObjectRecord)selectedRow.Record).Instance as SavedXrmRecordConfiguration;
                    if (instance != null)
                    {
                        var xrmRecordService = new XrmRecordService(instance);
                        Process.Start(xrmRecordService.WebUrl);
                    }
                }
            }, (g) => g.GridRecords != null && g.GridRecords.Any());
            this.AddCustomGridFunction(customGridFunction, typeof(SavedXrmRecordConfiguration));
        }
    }
}