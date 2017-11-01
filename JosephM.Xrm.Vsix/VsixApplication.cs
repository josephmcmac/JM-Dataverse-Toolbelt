using System;
using JosephM.Application.Application;
using JosephM.Application.ViewModel.ApplicationOptions;
using JosephM.Application.ViewModel.Dialog;
using JosephM.XRM.VSIX.Dialogs;
using System.ComponentModel.Design;
using Microsoft.VisualStudio.Shell;
using JosephM.Application.Modules;
using JosephM.XRM.VSIX.Utilities;
using System.Reflection;
using JosephM.Xrm.Vsix.Module;
using JosephM.Xrm.Vsix.Utilities;

namespace JosephM.XRM.VSIX
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
    }
}