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

namespace JosephM.XRM.VSIX.Commands.ImportCsvs
{
    internal sealed class ImportCsvsCommand : SolutionItemCommandBase
    {
        public override IEnumerable<string> ValidExtentions { get { return new[] { "csv" }; } }

        public override int CommandId
        {
            get { return 0x0108; }
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
            var files = GetSelectedFileNamesQualified();

            var request = new XrmImporterExporterRequest()
            {
                ImportExportTask = ImportExportTask.ImportCsvs,
                FolderOrFiles = XrmImporterExporterRequest.CsvImportOption.SpecificFiles,
                MatchByName = true,
                DateFormat = DateFormat.English,
                CsvsToImport = files.Select(f => new XrmImporterExporterRequest.CsvToImport() { Csv = new FileReference(f) }).ToArray()
            };

            var service = new XrmImporterExporterService<XrmRecordService>(GetXrmRecordService());

            var dialog = new VsixServiceDialog<XrmImporterExporterService<XrmRecordService>, XrmImporterExporterRequest, XrmImporterExporterResponse, XrmImporterExporterResponseItem>(
                service,
                request,
                new DialogController(new VsixApplicationController("VSIX", null)));

            DialogUtility.LoadDialog(dialog);
        }
    }
}
