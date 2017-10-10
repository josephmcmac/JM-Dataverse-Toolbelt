#region

using JosephM.Application.Application;
using JosephM.Application.Modules;
using JosephM.Core.AppConfig;
using JosephM.Core.Extentions;
using JosephM.Core.Log;
using JosephM.Prism.XrmModule.Forms;
using JosephM.Prism.XrmModule.Xrm;
using JosephM.Record.Xrm.XrmRecord;
using System.Configuration;

#endregion

namespace JosephM.Prism.XrmModule.XrmConnection
{
    public class XrmConnectionModule : ModuleBase
    {
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

            RegisterTypeForNavigation<XrmMaintainViewModel>();
            RegisterTypeForNavigation<XrmCreateViewModel>();
            RegisterTypeForNavigation<XrmConnectionDialog>();
            //RegisterType<XrmFormController>();
            //RegisterType<XrmFormService>();
        }

        public static void RefreshXrmServices(IXrmRecordConfiguration xrmConfiguration, IApplicationController controller)
        {
            controller.RegisterInstance<IXrmRecordConfiguration>(xrmConfiguration);
            controller.RegisterInstance(new XrmRecordService(xrmConfiguration, controller.ResolveType<LogController>(), formService: new XrmFormService()));
            controller.AddNotification("XRMCONNECTION", string.Format("Connected To Instance '{0}'", xrmConfiguration));
        }

        public override void InitialiseModule()
        {
            AddSetting("Connect To Crm", ConnectToCrm);
            AddHelpUrl("Connect To Crm", "ConnectToCrm");
        }

        private void ConnectToCrm()
        {
            NavigateTo<XrmConnectionDialog>();
        }
    }
}