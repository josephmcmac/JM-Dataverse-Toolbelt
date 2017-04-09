//------------------------------------------------------------------------------
// <copyright file="Command1.cs" company="Company">
//     Copyright (c) Company.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using EnvDTE;
using JosephM.XRM.VSIX.Dialogs;
using JosephM.XRM.VSIX.Utilities;
using Microsoft.VisualStudio.Shell;

namespace JosephM.XRM.VSIX.Commands.ManagePluginTriggers
{
    internal sealed class ManagePluginTriggersCommand : CommandBase
    {
        public override int CommandId
        {
            get { return 0x0104; }
        }

        public override string CommandSetId
        {
            get { return "dd8ecc36-be41-4089-831f-e9ee4dafecae"; }
        }

        private ManagePluginTriggersCommand(XrmPackage package)
            : base(package)
        {
        }

        public static ManagePluginTriggersCommand Instance { get; private set; }


        public static void Initialize(XrmPackage package)
        {
            Instance = new ManagePluginTriggersCommand(package);
        }

        public override void MenuItemCallback(object sender, EventArgs e)
        {
            var selectedItems = GetSelectedItems();
            foreach (SelectedItem item in selectedItems)
            {
                var project = item.Project;
                if (project.Name != null)
                {
                    var service = GetXrmRecordService();
                    var settings = VsixUtility.GetPackageSettings(GetDte2());
                    if (settings == null)
                        settings = new XrmPackageSettings();
                    var assemblyName = VsixUtility.GetProperty(project.Properties, "AssemblyName");

                    var dialog = new ManagePluginTriggersDialog(DialogUtility.CreateDialogController(), assemblyName, service, settings);

                    DialogUtility.LoadDialog(dialog);
                } 
            }
        }
    }
}
