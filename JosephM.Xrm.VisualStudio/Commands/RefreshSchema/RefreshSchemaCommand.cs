//------------------------------------------------------------------------------
// <copyright file="Command1.cs" company="Company">
//     Copyright (c) Company.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using EnvDTE;
using JosephM.Application.ViewModel.Dialog;
using JosephM.CodeGenerator.Service;
using JosephM.CodeGenerator.Xrm;
using JosephM.Core.FieldType;
using JosephM.XRM.VSIX.Dialogs;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace JosephM.XRM.VSIX.Commands.RefreshSchema
{
    internal sealed class RefreshSchemaCommand : SolutionItemCommandBase
    {
        public override IEnumerable<string> ValidFileNames
        {
            get { return new[] { "Schema.cs" }; }
        }
        public override IEnumerable<string> ValidExtentions
        {
            get { return new[] { "cs" }; }
        }
        public override int CommandId
        {
            get { return 0x0100; }
        }

        public override string CommandSetId
        {
            get { return "dd8ecc36-be41-4089-831f-e9ee4dafecae"; }
        }

        private RefreshSchemaCommand(XrmPackage package)
            : base(package)
        {
        }

        public static RefreshSchemaCommand Instance { get; private set; }

        public static void Initialize(XrmPackage package)
        {
            Instance = new RefreshSchemaCommand(package);
        }

        public override void MenuItemCallback(object sender, EventArgs e)
        {
            var xrmService = GetXrmRecordService();

            var codeGeneratorService = new XrmCodeGeneratorService(xrmService);

            var selectedItems = GetSelectedItems();
            if (selectedItems.Count != 1)
            {
                VsShellUtilities.ShowMessageBox(
                    this.ServiceProvider,
                    "Only one file may be selected to refresh",
                    "XRM",
                    OLEMSGICON.OLEMSGICON_INFO,
                    OLEMSGBUTTON.OLEMSGBUTTON_OK,
                    OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
                return;
            }
            FileInfo fileInfo = null;
            foreach (SelectedItem item in selectedItems)
            {
                if(item.ProjectItem != null && item.ProjectItem.FileCount > 0)
                {
                    fileInfo = new FileInfo(item.ProjectItem.FileNames[0]);
                }
            }
            if (fileInfo == null)
            {
                VsShellUtilities.ShowMessageBox(
                    this.ServiceProvider,
                    "Error getting file info",
                    "XRM",
                    OLEMSGICON.OLEMSGICON_INFO,
                    OLEMSGBUTTON.OLEMSGBUTTON_OK,
                    OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
                return;
            }

            var request = new CodeGeneratorRequest()
            {
                Type = CodeGeneratorType.CSharpMetadata,
                Folder = new Folder(fileInfo.DirectoryName),
                FileName = fileInfo.Name,
                Namespace = "Schema"
            };
            var dialog = new VsixServiceDialog<XrmCodeGeneratorService, CodeGeneratorRequest, CodeGeneratorResponse, CodeGeneratorResponseItem>(
                codeGeneratorService,
                request,
                new DialogController(new VsixApplicationController("VSIX", null)));
            //refresh cache in case customisation changes have been made
            xrmService.ClearCache();
            DialogUtility.LoadDialog(dialog);
        }
    }
}
