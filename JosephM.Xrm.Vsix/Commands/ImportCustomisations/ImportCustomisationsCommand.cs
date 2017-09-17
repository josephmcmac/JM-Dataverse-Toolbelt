using JosephM.Application.ViewModel.Dialog;
using JosephM.Core.FieldType;
using JosephM.CustomisationImporter.Service;
using JosephM.XRM.VSIX.Dialogs;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace JosephM.XRM.VSIX.Commands.ImportCustomisations
{
    internal sealed class ImportCustomisationsCommand : SolutionItemCommandBase<ImportCustomisationsCommand>
    {
        public override IEnumerable<string> ValidExtentions { get { return new[] { "xls", "xlsx" }; } }

        public override int CommandId
        {
            get { return 0x010A; }
        }

        public override void DoDialog()
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
                CreateDialogController()
                , showRequestEntryForm: true);
            //refresh cache in case customisation changes have been made
            xrmService.ClearCache();
            DialogUtility.LoadDialog(dialog);
        }
    }
}
