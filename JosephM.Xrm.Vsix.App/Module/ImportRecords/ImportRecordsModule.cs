using JosephM.Application;
using JosephM.Application.Modules;
using JosephM.Application.Prism.Module.ServiceRequest;
using JosephM.Core.FieldType;
using JosephM.Deployment;
using JosephM.Prism.XrmModule.XrmConnection;
using JosephM.Xrm.Vsix.Application;
using JosephM.Xrm.Vsix.Module.PackageSettings;
using System;
using System.Linq;

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
