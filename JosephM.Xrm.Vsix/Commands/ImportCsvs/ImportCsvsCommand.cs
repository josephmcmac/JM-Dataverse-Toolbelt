using JosephM.Application.ViewModel.Dialog;
using JosephM.Core.FieldType;
using JosephM.Record.Xrm.XrmRecord;
using JosephM.Xrm.ImportExporter.Service;
using JosephM.XRM.VSIX.Dialogs;
using System.Collections.Generic;
using System.Linq;

namespace JosephM.XRM.VSIX.Commands.ImportCsvs
{
    internal sealed class ImportCsvsCommand : SolutionItemCommandBase<ImportCsvsCommand>
    {
        public override IEnumerable<string> ValidExtentions { get { return new[] { "csv" }; } }

        public override int CommandId
        {
            get { return 0x0108; }
        }

        public override void DoDialog()
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
                CreateDialogController());

            DialogUtility.LoadDialog(dialog);
        }
    }
}
