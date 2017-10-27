using JosephM.Core.FieldType;
using JosephM.Deployment.ImportCsvs;
using JosephM.Record.Xrm.XrmRecord;
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

            var request = new ImportCsvsRequest()
            {
                FolderOrFiles = ImportCsvsRequest.CsvImportOption.SpecificFiles,
                MatchByName = true,
                DateFormat = DateFormat.English,
                CsvsToImport = files.Select(f => new ImportCsvsRequest.CsvToImport() { Csv = new FileReference(f) }).ToArray()
            };

            var service = new ImportCsvsService(GetXrmRecordService());

            var dialog = new VsixServiceDialog<ImportCsvsService, ImportCsvsRequest, ImportCsvsResponse, ImportCsvsResponseItem>(
                service,
                request,
                CreateDialogController());

            DialogUtility.LoadDialog(dialog);
        }
    }
}
