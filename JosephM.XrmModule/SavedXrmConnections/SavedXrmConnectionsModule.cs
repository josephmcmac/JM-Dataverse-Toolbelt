using JosephM.Application.Application;
using JosephM.Application.Desktop.Module.Settings;
using JosephM.Application.ViewModel.Dialog;
using JosephM.Application.ViewModel.Extentions;
using JosephM.Application.ViewModel.Grid;
using JosephM.Application.ViewModel.RecordEntry.Form;
using JosephM.Core.AppConfig;
using JosephM.Core.Attributes;
using JosephM.Core.Extentions;
using JosephM.Core.FieldType;
using JosephM.Record.Service;
using JosephM.Record.Xrm.XrmRecord;
using JosephM.Xrm;
using JosephM.XrmModule.Crud;
using JosephM.XrmModule.ToolingConnector;
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
            RegisterInstance<IOrganizationConnectionFactory>(new ToolingOrganizationConnectionFactory(ApplicationController));
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
            OpenToolingConnectorOnConfigurationForm();
        }

        private static IXrmRecordConfiguration LastXrmConfiguration { get; set; }

        public static void RefreshXrmServices(IXrmRecordConfiguration xrmConfiguration, IApplicationController controller, XrmRecordService xrmRecordService = null)
        {
            controller.RegisterInstance<IXrmRecordConfiguration>(xrmConfiguration);
            var serviceFactory = controller.ResolveType<IOrganizationConnectionFactory>();
            xrmRecordService = xrmRecordService ?? new XrmRecordService(xrmConfiguration, serviceFactory, formService: new XrmFormService());
            xrmRecordService.XrmRecordConfiguration = xrmConfiguration;
            controller.RegisterInstance(xrmRecordService);
            LastXrmConfiguration = xrmConfiguration;

            var spawnConnectAsynch = xrmConfiguration.ConnectionType == XrmRecordConfigurationConnectionType.ClientSecret;

            if (!xrmRecordService.XrmRecordConfiguration.ConnectionType.HasValue)
            {
                RefreshConnectionNotification(controller, "No Active Connection");
            }
            else if (xrmConfiguration.ConnectionType == XrmRecordConfigurationConnectionType.XrmTooling)
            {
                RefreshConnectionNotification(controller, string.Format("Connected To '{0}'", xrmConfiguration));
            }
            else if (controller.RunThreadsAsynch && spawnConnectAsynch)
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
            //get the organisations based on details entered
            //this.AddAutocompleteFunction(new AutocompleteFunction((recordForm) =>
            //{
            //    var objectRecord = recordForm.GetRecord() as ObjectRecord;
            //    if (objectRecord == null)
            //        return null;
            //    var thisConnectionEntered = objectRecord.Instance as XrmRecordConfiguration;
            //    if (thisConnectionEntered == null)
            //        return null;
            //    var xrmRecordConfiguration = new XrmRecordConfigurationInterfaceMapper().Map(thisConnectionEntered);
            //    var xrmConfiguration = new XrmConfigurationMapper().Map(xrmRecordConfiguration);
            //    var xrmConnection = new Xrm.XrmConnection(xrmConfiguration);
            //    return xrmConnection
            //        .GetActiveOrganisations();
            //}, typeof(Xrm.Organisation), nameof(Xrm.Organisation.UniqueName), new[] { new GridFieldMetadata(nameof(Xrm.Organisation.UniqueName), 100), new GridFieldMetadata(nameof(Xrm.Organisation.FriendlyName), 400) }, sortField: nameof(Xrm.Organisation.FriendlyName), displayInGrid: false, autosearch: false), typeof(SavedXrmRecordConfiguration), nameof(SavedXrmRecordConfiguration.OrganizationUniqueName));
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
                    ApplicationController.DoOnAsyncThread(() =>
                    {
                        var selectedRow = g.SelectedRows.First();
                        var instance = ((ObjectRecord)selectedRow.Record).Instance as SavedXrmRecordConfiguration;
                        if (instance != null)
                        {
                            var serviceFactory = ApplicationController.ResolveType<IOrganizationConnectionFactory>();
                            var xrmRecordService = new XrmRecordService(instance, serviceFactory);
                            Process.Start(xrmRecordService.WebUrl);
                        }
                    });
                }
            }, (g) => g.GridRecords != null && g.GridRecords.Any());
            this.AddCustomGridFunction(customGridFunction, typeof(SavedXrmRecordConfiguration));
        }

        private void OpenToolingConnectorOnConfigurationForm()
        {
            var customFunction = new OnChangeFunction((RecordEntryViewModelBase revm, string changedField) =>
            {
                try
                {
                    switch (changedField)
                    {
                        case nameof(SavedXrmRecordConfiguration.ConnectionType):
                            {
                                if (!(revm is GridRowViewModel))
                                {
                                    var connectionTypeViewModel = revm.GetPicklistFieldFieldViewModel(nameof(SavedXrmRecordConfiguration.ConnectionType));
                                    if (connectionTypeViewModel.Value == PicklistOption.EnumToPicklistOption(XrmRecordConfigurationConnectionType.XrmTooling))
                                    {
                                        var connectionIdViewModel = revm.GetStringFieldFieldViewModel(nameof(SavedXrmRecordConfiguration.ToolingConnectionId));
                                        var webUrlViewModel = revm.GetStringFieldFieldViewModel(nameof(SavedXrmRecordConfiguration.WebUrl));
                                        if (string.IsNullOrWhiteSpace(connectionIdViewModel.Value))
                                        {
                                            try
                                            {
                                                connectionIdViewModel.Value = $"{ApplicationController.ApplicationName}_{Guid.NewGuid()}";
                                                var objectRecord = revm.GetRecord() as ObjectRecord;
                                                if (objectRecord == null)
                                                {
                                                    throw new Exception($"Expected Form Record Of Type {nameof(ObjectRecord)}. Actual Type Is {revm.GetRecord().GetType().Name}");
                                                }
                                                var xrmConfiguration = objectRecord.Instance as SavedXrmRecordConfiguration;
                                                if (xrmConfiguration == null)
                                                {
                                                    throw new Exception($"Expected Form Object Of Type {nameof(SavedXrmRecordConfiguration)}. Actual Type Is {objectRecord.Instance.GetType().Name}");
                                                }
                                                var serviceFactory = ApplicationController.ResolveType<IOrganizationConnectionFactory>();
                                                var xrmRecordService = new XrmRecordService(xrmConfiguration, serviceFactory);
                                                var verifyConnection = xrmRecordService.VerifyConnection();
                                                if (!verifyConnection.IsValid)
                                                {
                                                    throw new Exception(verifyConnection.GetErrorString());
                                                }
                                                webUrlViewModel.Value = xrmRecordService.WebUrl;
                                            }
                                            catch (Exception ex)
                                            {
                                                connectionTypeViewModel.Value = null;
                                                connectionIdViewModel.Value = null;
                                                webUrlViewModel.Value = null;
                                                revm.ApplicationController.ThrowException(ex);
                                            }
                                        }
                                    }
                                }
                                break;
                            }
                    }
                }
                catch (Exception ex)
                {
                    revm.ApplicationController.ThrowException(ex);
                }
            });
            this.AddOnChangeFunction(customFunction, typeof(SavedXrmRecordConfiguration));
        }
    }
}