using JosephM.Application.Application;
using JosephM.Application.Modules;
using JosephM.Core.AppConfig;
using JosephM.Core.Attributes;
using JosephM.Core.Extentions;
using JosephM.Core.Log;
using JosephM.Record.Xrm.XrmRecord;
using JosephM.XrmModule.Crud;
using System;
using System.Configuration;

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
    }
}