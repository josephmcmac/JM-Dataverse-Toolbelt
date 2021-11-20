using System;

namespace JosephM.Xrm.Vsix.App.Application
{
    public interface IMenuCommand
    {
        bool Visible { get; set; }
        bool Enabled { get; set; }
        void AddBeforeQueryStatusHandler(EventHandler eventHandler);
    }
}
