using JosephM.Application;
using JosephM.Application.Modules;
using JosephM.Core.FieldType;
using JosephM.Deployment.ImportCsvs;
using JosephM.Prism.XrmModule.XrmConnection;
using JosephM.Xrm.Vsix.Utilities;
using System;
using System.Linq;

namespace JosephM.Xrm.Vsix.Module.ImportCsvs
{
    [MenuItemVisibleCsvs]
    [DependantModule(typeof(XrmConnectionModule))]
    public class VsixImportCsvsModule : ImportCsvsModule
    {
        public override void InitialiseModule()
        {
        }

        public override void RegisterTypes()
        {
        }

        public override void DialogCommand()
        {
            var visualStudioService = ApplicationController.ResolveType(typeof(IVisualStudioService)) as IVisualStudioService;
            if (visualStudioService == null)
                throw new NullReferenceException("visualStudioService");

            var files = visualStudioService.GetSelectedFileNamesQualified();

            var request = new ImportCsvsRequest()
            {
                FolderOrFiles = ImportCsvsRequest.CsvImportOption.SpecificFiles,
                MatchByName = true,
                DateFormat = DateFormat.English,
                CsvsToImport = files.Select(f => new ImportCsvsRequest.CsvToImport() { Csv = new FileReference(f) }).ToArray()
            };

            var uri = new UriQuery();
            uri.AddObject(nameof(ImportCsvsDialog.Request), request);
            uri.AddObject(nameof(ImportCsvsDialog.SkipObjectEntry), true);
            //todo need to pass through to skip the entry screen
            ApplicationController.RequestNavigate("Main", typeof(ImportCsvsDialog), uri);
        }
    }
}
