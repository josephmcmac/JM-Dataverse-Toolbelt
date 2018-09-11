using JosephM.Application;
using JosephM.Application.Modules;
using JosephM.Core.FieldType;
using JosephM.Deployment.ImportCsvs;
using JosephM.XrmModule.XrmConnection;
using JosephM.Xrm.Vsix.Application;
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
                MatchByName = true,
                DateFormat = DateFormat.English,
                CsvsToImport = files.Select(f => new ImportCsvsRequest.CsvToImport() { SourceCsv = new FileReference(f) }).ToArray()
            };

            var uri = new UriQuery();
            uri.AddObject(nameof(ImportCsvsDialog.Request), request);
            ApplicationController.NavigateTo(typeof(ImportCsvsDialog), uri);
        }
    }
}
