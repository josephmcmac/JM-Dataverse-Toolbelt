using JosephM.Application.Application;
using JosephM.Application.Desktop.Application;
using JosephM.Application.Modules;
using JosephM.Application.ViewModel.ApplicationOptions;
using JosephM.Application.ViewModel.Dialog;
using JosephM.Core.AppConfig;
using JosephM.Record.Xrm.XrmRecord;
using JosephM.Xrm.Vsix.App.Application;
using JosephM.Xrm.Vsix.Application;
using JosephM.Xrm.Vsix.Module;
using JosephM.Xrm.Vsix.Module.PackageSettings;
using JosephM.XrmModule.SavedXrmConnections;
using System;
using System.Reflection;

namespace JosephM.Xrm.Vsix
{
    public class VsixApplication : ApplicationBase
    {
        public VsixApplication(VsixApplicationController applicationController, ISettingsManager settingsManager, Guid commandSetId, bool isNonSolutionExplorerContext)
            : base(applicationController, new ApplicationOptionsViewModel(applicationController), settingsManager)
        {
            VsixApplicationController = applicationController;
            CommandSetId = commandSetId;
            IsNonSolutionExplorerContext = isNonSolutionExplorerContext;
            ApplicationName = applicationController.ApplicationName;
            Controller.RegisterType<IDialogController, DialogController>();
        }

        public VsixApplicationController VsixApplicationController
        {
            get; set;
        }

        public Guid CommandSetId { get; set; }
        public bool IsNonSolutionExplorerContext { get; }
        public string ApplicationName { get; set; }

        public void AddModule<T>(int commandId)
            where T : ActionModuleBase, new()
        {
            var module = AddModule<T>();

            if (!IsNonSolutionExplorerContext)
            {
                var xrmCommandService = Controller.ResolveType(typeof(IXrmMenuCommandService)) as IXrmMenuCommandService;

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

                var menuItem = xrmCommandService.AddMenuCommand(CommandSetId, commandId, menuItemCallback);

                var menuItemVisibleAttribute = typeof(T).GetCustomAttribute<MenuItemVisible>();
                if (menuItemVisibleAttribute != null)
                {
                    EventHandler clickHandler = (o, e) => menuItemVisibleAttribute.Process(Controller, menuItem);
                    menuItem.AddBeforeQueryStatusHandler(clickHandler);
                }
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
            if (settingsConnection.ConnectionType != containerConnection.ConnectionType
                || settingsConnection.ToolingConnectionId != containerConnection.ToolingConnectionId
                || settingsConnection.ClientId != containerConnection.ClientId
                || settingsConnection.ClientSecret?.GetRawPassword() != containerConnection.ClientSecret?.GetRawPassword()
                || settingsConnection.WebUrl != containerConnection.WebUrl)
            {
                SavedXrmConnectionsModule.RefreshXrmServices(settingsConnection, Controller);
            }
            var packageSettings = settingsManager.Resolve<XrmPackageSettings>();
            Controller.RegisterInstance(typeof(XrmPackageSettings), packageSettings);
        }

        public static VsixApplication Create(IVisualStudioService visualStudioService, IDependencyResolver dependencyResolver, string applicationName, Guid commandSetId, bool isNonSolutionExplorerContext = false)
        {
            var applicationController = new VsixApplicationController(dependencyResolver, applicationName);
            var vsixSettingsManager = new VsixSettingsManager(visualStudioService, new DesktopSettingsManager(applicationController));
            var app = new VsixApplication(applicationController, vsixSettingsManager, commandSetId, isNonSolutionExplorerContext);
            return app;
        }
    }
}