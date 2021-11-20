using JosephM.Xrm.Vsix.App.Application;
using Microsoft.VisualStudio.Shell;
using System;
using System.ComponentModel.Design;

namespace JosephM.Xrm.Vsix.App.Vs2019.Application
{
    public class XrmMenuCommandService : IXrmMenuCommandService
    {
        private readonly IMenuCommandService _menuCommandService;

        public XrmMenuCommandService(IMenuCommandService menuCommandService)
        {
            _menuCommandService = menuCommandService;
        }

        public IMenuCommand AddMenuCommand(Guid commandSetId, int commandId, EventHandler menuItemCallback)
        {
            var menuCommandId = new CommandID(commandSetId, commandId);
            var menuItem = new OleMenuCommand(menuItemCallback, menuCommandId);
            _menuCommandService.AddCommand(menuItem);
            return new MenuCommand(menuItem);
        }
    }
}
