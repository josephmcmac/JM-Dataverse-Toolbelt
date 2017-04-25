using System;
using System.Linq;
using JosephM.Application.ViewModel.Dialog;
using JosephM.Record.Xrm.XrmRecord;
using JosephM.XRM.VSIX.Utilities;
using JosephM.Xrm.ImportExporter.Service;
using JosephM.XRM.VSIX.Dialogs;
using JosephM.ObjectMapping;
using JosephM.Prism.XrmModule.SavedXrmConnections;
using JosephM.Core.FieldType;
using System.Collections.Generic;

namespace JosephM.XRM.VSIX.Commands.GetSolution
{
    public class ImportCsvsDialog : DialogViewModel
    {
        public XrmRecordService Service { get; set; }
        public XrmPackageSettings PackageSettings { get; set; }
        public IVisualStudioService VisualStudioService { get; set; }
        public IEnumerable<string> CsvFiles { get; set; }

        public ImportCsvsDialog(IDialogController dialogController, XrmRecordService xrmRecordService, XrmPackageSettings packageSettings, IVisualStudioService visualStudioService, IEnumerable<string> csvFiles)
            : base(dialogController)
        {
            Service = xrmRecordService;
            PackageSettings = packageSettings;
            VisualStudioService = visualStudioService;
            CsvFiles = csvFiles;
        }

        protected override void LoadDialogExtention()
        {
            StartNextAction();
        }

        protected override void CompleteDialogExtention()
        {
            LoadingViewModel.IsLoading = true;

            if (PackageSettings.Solution == null)
                throw new NullReferenceException("Solution is not populated in the package settings");

            var mapper = new ClassSelfMapper();
            var savedConnection = new SavedXrmRecordConfiguration();
            mapper.Map(Service.XrmRecordConfiguration, savedConnection);

            var folderPath = VisualStudioService.SolutionDirectory + "/Customisations";

            var request = new XrmImporterExporterRequest()
            {
                ImportExportTask = ImportExportTask.ImportCsvs,
                FolderOrFiles = XrmImporterExporterRequest.CsvImportOption.SpecificFiles,
                MatchByName = true,
                DateFormat = DateFormat.English,
                CsvsToImport = CsvFiles.Select(f => new XrmImporterExporterRequest.CsvToImport() { Csv = new FileReference(f) }).ToArray()
            };

            var service = new XrmImporterExporterService<XrmRecordService>(Service);

            var dialog = new VsixServiceDialog<XrmImporterExporterService<XrmRecordService>, XrmImporterExporterRequest, XrmImporterExporterResponse, XrmImporterExporterResponseItem>(
                service,
                request,
                new DialogController(new VsixApplicationController("VSIX", null)));

            DialogUtility.LoadDialog(dialog);

            LoadingViewModel.IsLoading = false;
        }
    }
}