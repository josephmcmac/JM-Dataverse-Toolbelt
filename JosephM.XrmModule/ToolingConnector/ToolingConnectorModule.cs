using JosephM.Application.Modules;
using JosephM.Application.ViewModel.Extentions;
using JosephM.Application.ViewModel.RecordEntry.Form;
using JosephM.Record.Xrm.XrmRecord;
using JosephM.Xrm;
using System;
using JosephM.Core.AppConfig;
using JosephM.Record.Service;
using JosephM.XrmModule.SavedXrmConnections;
using JosephM.Application.ViewModel.Grid;

namespace JosephM.XrmModule.ToolingConnector
{
    public class ToolingConnectorModule : ModuleBase
    {
        public override void InitialiseModule()
        {
            
        }

        public override void RegisterTypes()
        {
            RegisterInstance<IOrganizationConnectionFactory>(new ToolingOrganizationConnectionFactory(ApplicationController));
            OpenToolingConnectorOnConfigurationForm();
        }

        private void OpenToolingConnectorOnConfigurationForm()
        {
            var customFunction = new OnChangeFunction((RecordEntryViewModelBase revm, string changedField) =>
            {
                try
                {
                    switch (changedField)
                    {
                        case nameof(SavedXrmRecordConfiguration.UseXrmToolingConnector):
                            {
                                if (!(revm is GridRowViewModel))
                                {
                                    var useToolingConnectorViewModel = revm.GetBooleanFieldFieldViewModel(nameof(SavedXrmRecordConfiguration.UseXrmToolingConnector));
                                    if (useToolingConnectorViewModel.Value ?? false)
                                    {
                                        var connectionIdViewModel = revm.GetStringFieldFieldViewModel(nameof(SavedXrmRecordConfiguration.ToolingConnectionId));
                                        if (string.IsNullOrWhiteSpace(connectionIdViewModel.Value))
                                        {
                                            try
                                            {
                                                connectionIdViewModel.Value = $"{ApplicationController.ApplicationName}_{Guid.NewGuid().ToString()}";
                                                var objectRecord = revm.GetRecord() as ObjectRecord;
                                                if (objectRecord == null)
                                                    throw new Exception($"Expected Form Record Of Type {nameof(ObjectRecord)}. Actual Type Is {revm.GetRecord().GetType().Name}");
                                                var xrmConfiguration = objectRecord.Instance as SavedXrmRecordConfiguration;
                                                if (xrmConfiguration == null)
                                                    throw new Exception($"Expected Form Object Of Type {nameof(SavedXrmRecordConfiguration)}. Actual Type Is {objectRecord.Instance.GetType().Name}");
                                                var serviceFactory = ApplicationController.ResolveType<IOrganizationConnectionFactory>();
                                                var xrmRecordService = new XrmRecordService(xrmConfiguration, serviceFactory);
                                                var verifyConnection = xrmRecordService.VerifyConnection();
                                                if (!verifyConnection.IsValid)
                                                {
                                                    throw new Exception(verifyConnection.GetErrorString());
                                                }
                                            }
                                            catch (Exception ex)
                                            {
                                                useToolingConnectorViewModel.Value = false;
                                                connectionIdViewModel.Value = null;
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
