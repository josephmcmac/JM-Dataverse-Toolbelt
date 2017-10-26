using EnvDTE;
using EnvDTE80;
using JosephM.Application.Application;
using JosephM.Application.ViewModel.Dialog;
using JosephM.Application.ViewModel.Grid;
using JosephM.Core.Extentions;
using JosephM.Prism.Infrastructure.Module.Crud;
using JosephM.Prism.XrmModule.Crud;
using JosephM.Prism.XrmModule.SavedXrmConnections;
using JosephM.Record.Service;
using JosephM.Record.Xrm.XrmRecord;
using JosephM.XRM.VSIX.Dialogs;
using JosephM.XRM.VSIX.Utilities;
using Microsoft.Practices.Unity;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Windows;

namespace JosephM.XRM.VSIX.Commands
{
    internal abstract class CommandBase<T>
        where T : CommandBase<T>, new()
    {
        public static T Instance { get; set; }

        private XrmPackage XrmPackage { get; set; }

        public static void Initialize(XrmPackage package)
        {
            Instance = new T()
            {
                XrmPackage = package
            };
            if (package == null)
                throw new ArgumentNullException("package");

            Instance.XrmPackage = package;

            OleMenuCommandService commandService =
                Instance.ServiceProvider.GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if (commandService != null)
            {
                Instance.MenuCommandId = new CommandID(new Guid(Instance.CommandSetId), Instance.CommandId);
                Instance.MenuItem = new OleMenuCommand(Instance.MenuItemCallback, Instance.MenuCommandId);
                commandService.AddCommand(Instance.MenuItem);

                Instance.MenuItem.BeforeQueryStatus += Instance.menuCommand_BeforeQueryStatus;
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
            get { return XrmPackage; }
        }

        public OleMenuCommand MenuItem { get; set; }
        public CommandID MenuCommandId { get; private set; }

        public void MenuItemCallback(object sender, EventArgs e)
        {
            TryDoSomething(DoDialog);
        }

        public void TryDoSomething(Action doSomething)
        {
            try
            {
                doSomething();
            }
            catch (Exception ex)
            {
                UserMessage(ex.DisplayString());
            }
        }

        public static void UserMessage(string message)
        {
            MessageBox.Show(message);
        }

        public abstract void DoDialog();

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
            return XrmPackage.GetXrmRecordService();
        }

        protected IVisualStudioService GetVisualStudioService()
        {
            return XrmPackage.GetVisualStudioService();
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

        public DialogController CreateDialogController(XrmPackageSettings settings = null)
        {
            if (settings == null)
                settings = GetPackageSettings();
            var container = VsixDependencyContainer.Create(settings, GetVisualStudioService());
            var applicationController = new VsixApplicationController(container);
            var customGridFunction = new CustomGridFunction("CRUD", "Browse Selected", (g) =>
            {
                //todo this code should be consolidated with the module
                if (g.SelectedRows.Count() != 1)
                {
                    applicationController.UserMessage("Please select 1 connection to browse");
                }
                else
                {
                    var selectedRow = g.SelectedRows.First();
                    var instance = ((ObjectRecord)selectedRow.Record).Instance as SavedXrmRecordConfiguration;
                    if (instance != null)
                    {
                        var xrmRecordService = new XrmRecordService(instance, formService: new XrmFormService());
                        var dialog = new CrudDialog(new DialogController(applicationController), xrmRecordService);
                        dialog.SetTabLabel("Browse " + instance.Name);
                        g.LoadDialog(dialog);
                    }
                }                
            }, (g) => g.GridRecords != null && g.GridRecords.Any());
            var functions = new CustomGridFunctions();
            functions.AddFunction(customGridFunction);
            //todo this should add the function not just inject it
            applicationController.RegisterInstance(typeof(CustomGridFunctions), typeof(SavedXrmRecordConfiguration).AssemblyQualifiedName, functions);
            var controller = new DialogController(new VsixApplicationController(container));
            container.RegisterInstance(typeof(IApplicationController),applicationController);
            container.RegisterType<IDialogController, DialogController>();
            return controller;
        }
    }
}
