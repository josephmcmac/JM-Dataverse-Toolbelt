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
using System.Windows;
using JosephM.Core.Extentions;
using JosephM.Xrm.ImportExporter.Service;
using System.Linq;
using JosephM.Record.Xrm.XrmRecord;
using JosephM.Core.FieldType;
using JosephM.Application.ViewModel.Dialog;
using JosephM.CustomisationImporter.Service;

namespace JosephM.XRM.VSIX.Commands.ImportCustomisations
{
    internal sealed class ImportCustomisationsCommand : SolutionItemCommandBase
    {
        public override IEnumerable<string> ValidExtentions { get { return new[] { "xls", "xlsx" }; } }

        public override int CommandId
        {
            get { return 0x010A; }
        }

        private ImportCustomisationsCommand(XrmPackage package)
            : base(package)
        {
        }

        public static ImportCustomisationsCommand Instance { get; private set; }


        public static void Initialize(XrmPackage package)
        {
            Instance = new ImportCustomisationsCommand(package);
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
            var settings = GetPackageSettings();
            var codeGeneratorService = new CustomisationImportRequest();

            var selectedItems = GetSelectedFileNamesQualified();
            if (selectedItems.Count() != 1)
            {
                MessageBox.Show("Only one file may be selected to refresh");
                return;
            }
            var request = new CustomisationImportRequest()
            {
                 ExcelFile = new FileReference(selectedItems.First()),
                 AddToSolution = settings.AddToSolution,
                 Solution = settings.Solution,
                 HideExcelFile = true,
                 HideSolutionOptions = true
            };
            var customisationImportService = new CustomisationImportService(xrmService);
            var dialog = new VsixServiceDialog<CustomisationImportService, CustomisationImportRequest, CustomisationImportResponse, CustomisationImportResponseItem>(
                customisationImportService,
                request,
                new DialogController(new VsixApplicationController("VSIX", null))
                , showRequestEntryForm: true);
            //refresh cache in case customisation changes have been made
            xrmService.ClearCache();
            DialogUtility.LoadDialog(dialog);
        }
    }
}
