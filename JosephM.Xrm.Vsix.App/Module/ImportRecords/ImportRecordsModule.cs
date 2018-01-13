using JosephM.Application;
using JosephM.Application.Modules;
using JosephM.Prism.Infrastructure.Module;
using JosephM.Prism.XrmModule.XrmConnection;
using JosephM.Xrm.Vsix.Module.PackageSettings;
using JosephM.Xrm.Vsix.Application;
using System;
using System.Linq;
using JosephM.Core.FieldType;
using JosephM.Deployment;

namespace JosephM.Xrm.Vsix.Module.ImportRecords
{
    [MenuItemVisibleXml]
    [DependantModule(typeof(XrmPackageSettingsModule))]
    [DependantModule(typeof(XrmConnectionModule))]
    public class ImportRecordsModule : ServiceRequestModule<ImportRecordsDialog, ImportRecordsService, ImportRecordsRequest, ImportRecordsResponse, DataImportResponseItem>
    {
        public override void DialogCommand()
        {
            var visualStudioService = ApplicationController.ResolveType(typeof(IVisualStudioService)) as IVisualStudioService;
            if (visualStudioService == null)
                throw new NullReferenceException("visualStudioService");
            var selectedItems = visualStudioService.GetSelectedFileNamesQualified();

            var request = new ImportRecordsRequest()
            {
                XmlFiles = selectedItems.Select(f => new FileReference(f)).ToArray()
            };
            var uri = new UriQuery();
            uri.AddObject(nameof(ImportRecordsDialog.Request), request);
            ApplicationController.RequestNavigate("Main", typeof(ImportRecordsDialog), uri);
        }
    }
}
