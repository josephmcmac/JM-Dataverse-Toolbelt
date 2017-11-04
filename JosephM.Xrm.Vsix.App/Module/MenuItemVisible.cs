using JosephM.Xrm.Vsix.Application;
using Microsoft.VisualStudio.Shell;
using System;

namespace JosephM.Xrm.Vsix.Module
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public abstract class MenuItemVisible : Attribute
    {
        public abstract bool IsVisible(IVisualStudioService visualStudioService);

        public void Process(IVisualStudioService visualStudioService, OleMenuCommand menuCommand)
        {
            SetHidden(menuCommand);
            if (IsVisible(visualStudioService))
                SetVisible(menuCommand);
        }

        private void SetVisible(OleMenuCommand menuCommand)
        {
            if (menuCommand != null)
            {
                menuCommand.Visible = true;
                menuCommand.Enabled = true;
            }
        }

        private void SetHidden(OleMenuCommand menuCommand)
        {
            if (menuCommand != null)
            {
                menuCommand.Visible = false;
                menuCommand.Enabled = false;
            }
        }
    }
}
