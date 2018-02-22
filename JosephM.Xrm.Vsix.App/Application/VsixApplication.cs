using JosephM.Application.Application;
using JosephM.Application.Modules;
using JosephM.Application.ViewModel.ApplicationOptions;
using JosephM.Application.ViewModel.Dialog;
using JosephM.Prism.XrmModule.XrmConnection;
using JosephM.Record.Xrm.XrmRecord;
using JosephM.Xrm.Vsix.Application;
using JosephM.Xrm.Vsix.Module;
using JosephM.Xrm.Vsix.Module.PackageSettings;
using Microsoft.VisualStudio.Shell;
using System;
using System.ComponentModel.Design;
using System.Reflection;

namespace JosephM.Xrm.Vsix
{
    public class VsixApplication : ApplicationBase
    {
        public VsixApplication(VsixApplicationController applicationController, ISettingsManager settingsManager, Guid commandSetId)
            : base(applicationController, new ApplicationOptionsViewModel(applicationController), settingsManager)
        {
            CommandSetId = commandSetId;
            ApplicationName = applicationController.ApplicationName;
            Controller.RegisterType<IDialogController, DialogController>();
        }

        public Guid CommandSetId { get; set; }

        public string ApplicationName { get; set; }

        public void AddModule<T>(int commandId)
            where T : ActionModuleBase, new()
        {
            var module = AddModule<T>();

            var commandService = Controller.ResolveType(typeof(IMenuCommandService)) as IMenuCommandService;
            if (commandService == null)
                throw new NullReferenceException("commandService");

            var menuCommandId = new CommandID(CommandSetId, commandId);

            EventHandler menuItemCallback = (sender, e) =>
            {
                try
                {
                    CheckRefreshActiveSettings();
                    module.DialogCommand();
                }
                catch (Exception ex)
                {
                    Controller.ThrowException(ex);
                }
            };

            var menuItem = new OleMenuCommand(menuItemCallback, menuCommandId);
            commandService.AddCommand(menuItem);

            var menuItemVisibleAttribute = typeof(T).GetCustomAttribute<MenuItemVisible>();
            if (menuItemVisibleAttribute != null)
            {
                EventHandler clickHandler = (o, e) => menuItemVisibleAttribute.Process(Controller.ResolveType(typeof(IVisualStudioService)) as IVisualStudioService, menuItem);
                menuItem.BeforeQueryStatus += clickHandler;
            }
        }


        public void CheckRefreshActiveSettings()
        {
            //okay so I could not subscribe to when a solution is opened
            //at which point I would need to load these settings into the unity container
            //so each time a button is clicked I will just ensure the objects in unity
            //are consistent with the active solutions settings

            var settingsManager = (ISettingsManager)Controller.ResolveType(typeof(ISettingsManager));
            var settingsConnection = settingsManager.Resolve<XrmRecordConfiguration>();
            var containerConnection = (IXrmRecordConfiguration)Controller.ResolveType(typeof(IXrmRecordConfiguration));
            if (settingsConnection.AuthenticationProviderType != containerConnection.AuthenticationProviderType
                || settingsConnection.DiscoveryServiceAddress != containerConnection.DiscoveryServiceAddress
                || settingsConnection.OrganizationUniqueName != containerConnection.OrganizationUniqueName
                || settingsConnection.Domain != containerConnection.Domain
                || settingsConnection.Username != containerConnection.Username
                || settingsConnection.Password?.GetRawPassword() != containerConnection.Password?.GetRawPassword())
            {
                XrmConnectionModule.RefreshXrmServices(settingsConnection, Controller);
            }
            var packageSettings = settingsManager.Resolve<XrmPackageSettings>();
            Controller.RegisterInstance(typeof(XrmPackageSettings), packageSettings);
        }
    }
}