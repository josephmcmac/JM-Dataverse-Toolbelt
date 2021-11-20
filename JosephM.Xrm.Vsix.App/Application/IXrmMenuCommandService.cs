using System;

namespace JosephM.Xrm.Vsix.App.Application
{
    public interface IXrmMenuCommandService
    {
        IMenuCommand AddMenuCommand(Guid commandSetId, int commandId, EventHandler menuItemCallback);
    }
}
