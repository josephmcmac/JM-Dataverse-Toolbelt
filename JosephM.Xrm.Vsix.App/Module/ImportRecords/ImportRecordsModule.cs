﻿using JosephM.Application;
using JosephM.Application.Desktop.Module.ServiceRequest;
using JosephM.Application.Modules;
using JosephM.Core.FieldType;
using JosephM.Xrm.DataImportExport.Import;
using JosephM.Xrm.Vsix.Application;
using JosephM.Xrm.Vsix.Module.PackageSettings;
using JosephM.XrmModule.SavedXrmConnections;
using System;
using System.Linq;

namespace JosephM.Xrm.Vsix.Module.ImportRecords
{
    [MenuItemVisibleImportRecords]
    [DependantModule(typeof(XrmPackageSettingsModule))]
    [DependantModule(typeof(SavedXrmConnectionsModule))]
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
            ApplicationController.NavigateTo(typeof(ImportRecordsDialog), uri);
        }
    }
}
