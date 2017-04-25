//------------------------------------------------------------------------------
// <copyright file="Command1.cs" company="Company">
//     Copyright (c) Company.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using JosephM.XRM.VSIX.Dialogs;
using JosephM.XRM.VSIX.Utilities;
using EnvDTE;
using System.Collections.Generic;

namespace JosephM.XRM.VSIX.Commands.GetSolution
{
    internal sealed class ImportCsvsCommand : SolutionItemCommandBase
    {
        public override IEnumerable<string> ValidExtentions { get { return new[] { "csv" }; } }

        public override int CommandId
        {
            get { return 0x0108; }
        }

        public override string CommandSetId
        {
            get { return "dd8ecc36-be41-4089-831f-e9ee4dafecae"; }
        }

        private ImportCsvsCommand(XrmPackage package)
            : base(package)
        {
        }

        public static ImportCsvsCommand Instance { get; private set; }


        public static void Initialize(XrmPackage package)
        {
            Instance = new ImportCsvsCommand(package);
        }

        public override void MenuItemCallback(object sender, EventArgs e)
        {
            var settings = VsixUtility.GetPackageSettings(GetDte2());
            if (settings == null)
                settings = new XrmPackageSettings();

            var files = new List<string>();
            var selectedItems = GetSelectedItems();
            foreach (SelectedItem item in selectedItems)
            {
                if (item.ProjectItem != null && !string.IsNullOrWhiteSpace(item.Name))
                {
                    files.Add(item.ProjectItem.FileNames[0]);
                }
            }

            var dialog = new ImportCsvsDialog(DialogUtility.CreateDialogController(), GetXrmRecordService(), settings, GetVisualStudioService(), files);
            DialogUtility.LoadDialog(dialog);
        }
    }
}
