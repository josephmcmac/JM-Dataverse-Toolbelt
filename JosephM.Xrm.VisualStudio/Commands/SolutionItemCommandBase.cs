using System;
using System.Collections.Generic;
using System.Linq;
using EnvDTE;
using Microsoft.VisualStudio.Shell;

namespace JosephM.XRM.VSIX.Commands
{
    abstract class SolutionItemCommandBase : CommandBase
    {
        public virtual IEnumerable<string> ValidFileNames { get { return null; } }
        public abstract IEnumerable<string> ValidExtentions { get; }

        protected SolutionItemCommandBase(XrmPackage package)
            : base(package)
        {
        }

        protected override void menuCommand_BeforeQueryStatus(object sender, EventArgs e)
        {
            SetHidden();

            var selectedItems = GetSelectedItems();
            var validSelection = false;
            foreach (SelectedItem item in selectedItems)
            {
                if (item.ProjectItem != null && ValidExtentions != null &&
                    ValidExtentions.Any(ext => item.ProjectItem.Name.EndsWith("." + ext))
                    && (ValidFileNames == null || ValidFileNames.Contains(item.Name)))
                {
                    validSelection = true;
                }
                else
                {
                    validSelection = false;
                    break;
                }
            }
            if(validSelection)
                SetVisible();
        }
    }
}