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
using System.Linq;
using System.Windows;
using JosephM.Core.Extentions;

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
            try
            {
                DoDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.DisplayString());
            }
        }

        private void DoDialog()
        {
            var xrmService = GetXrmRecordService();

            var codeGeneratorService = new XrmCodeGeneratorService(xrmService);

            var selectedItems = GetSelectedFileNamesQualified();
            if (selectedItems.Count() != 1)
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
            FileInfo fileInfo = new FileInfo(selectedItems.First());

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
