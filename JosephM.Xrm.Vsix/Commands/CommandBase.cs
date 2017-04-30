using EnvDTE;
using EnvDTE80;
using JosephM.Record.Xrm.XrmRecord;
using JosephM.XRM.VSIX.Utilities;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;

namespace JosephM.XRM.VSIX.Commands
{
    internal abstract class CommandBase
    {
        protected XrmPackage package;

        protected CommandBase(XrmPackage package)
        {
            if (package == null)
            {
                throw new ArgumentNullException("package");
            }

            this.package = package;

            OleMenuCommandService commandService =
                this.ServiceProvider.GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if (commandService != null)
            {
                MenuCommandId = new CommandID(new Guid(CommandSetId), CommandId);
                MenuItem = new OleMenuCommand(this.MenuItemCallback, MenuCommandId);
                commandService.AddCommand(MenuItem);

                MenuItem.BeforeQueryStatus += menuCommand_BeforeQueryStatus;
            }
        }

        protected virtual void menuCommand_BeforeQueryStatus(object sender, EventArgs e)
        {
        }

        public abstract int CommandId { get; }
        public string CommandSetId { get { return "43816e6d-4db8-48d6-8bfa-75916cb080f0"; } }

        /// <summary>
        /// Gets the service provider from the owner package.
        /// </summary>
        protected IServiceProvider ServiceProvider
        {
            get { return this.package; }
        }

        public OleMenuCommand MenuItem { get; set; }
        public CommandID MenuCommandId { get; private set; }

        public abstract void MenuItemCallback(object sender, EventArgs e);

        protected void SetVisible()
        {
            if (MenuItem != null)
            {
                MenuItem.Visible = true;
                MenuItem.Enabled = true;
            }
        }

        protected void SetHidden()
        {
            if (MenuItem != null)
            {
                MenuItem.Visible = false;
                MenuItem.Enabled = false;
            }
        }

        protected SelectedItems GetSelectedItems()
        {
            var dte = ServiceProvider.GetService(typeof (SDTE)) as DTE2;
            if (dte != null)
            {
                return dte.SelectedItems;
            }
            return null;
        }

        protected IEnumerable<string> GetSelectedFileNamesQualified()
        {
            var fileNames = new List<string>();

            var items = GetSelectedItems();
            foreach (SelectedItem item in items)
            {
                if (item.ProjectItem != null && !string.IsNullOrWhiteSpace(item.Name))
                {
                    string fileName = null;
                    try
                    {
                        fileName = item.ProjectItem.FileNames[0];
                    }
                    catch(Exception) {}
                    if (fileName == null)
                        try
                        {
                            fileName = item.ProjectItem.FileNames[1];
                        }
                        catch (Exception) { }
                    if (fileName == null)
                        throw new Exception("Could not extract file name for ProjectItem " + item.Name);
                    fileNames.Add(fileName);
                }
            }

            return fileNames;
        }

        protected XrmRecordService GetXrmRecordService()
        {
            return package.GetXrmRecordService();
        }

        protected IVisualStudioService GetVisualStudioService()
        {
            return package.GetVisualStudioService();
        }

        protected string GetFile(string getFileName)
        {
            return VsixUtility.GetFile(ServiceProvider, getFileName);
        }

        public XrmPackageSettings GetPackageSettings()
        {
            return VsixUtility.GetPackageSettings(GetDte2());
        }

        protected Solution2 GetSolution()
        {
            var dte = GetDte2();
            if (dte == null)
                return null;
            return dte.Solution as Solution2;
        }

        public void IterateSolutionAndProjects()
        {
            var dte = GetDte2();
            if (dte != null)
            {
                var selectedItems = dte.SelectedItems;
                var selectionCount = selectedItems.Count;
                if (selectionCount > 0)
                {
                    foreach (SelectedItem item in selectedItems)
                    {
                        var name = item.Name;
                        var projectItem = item.ProjectItem;
                        var project = item.Project;
                    }
                }

                var count = dte.Solution.Projects.Count;
                foreach (Project project in dte.Solution.Projects)
                {
                    var name = project.Name;
                    var itemCount = project.ProjectItems.Count;
                    foreach (ProjectItem item in project.ProjectItems)
                    {
                        var itemName = item.Name;
                    }
                }
            }
        }

        protected DTE2 GetDte2()
        {
            var dte = this.ServiceProvider.GetService(typeof (SDTE)) as DTE2;
            return dte;
        }
    }
}
