using JosephM.Application.Modules;
using JosephM.Application.ViewModel.Dialog;
using JosephM.Application.ViewModel.Extentions;
using JosephM.Application.ViewModel.Grid;
using JosephM.Application.ViewModel.RecordEntry.Form;
using JosephM.Core.Attributes;
using JosephM.Core.FieldType;
using JosephM.Core.AppConfig;
using JosephM.Record.Extentions;
using JosephM.Record.Service;
using JosephM.Record.Xrm.XrmRecord;
using JosephM.Xrm.RecordExtract.RecordExtract;
using JosephM.XrmModule.Crud;
using JosephM.XrmModule.SavedXrmConnections;
using System;
using System.Linq;

namespace JosephM.Xrm.RecordExtract.TextSearch
{
    [DependantModule(typeof(SavedXrmConnectionsModule))]
    [DependantModule(typeof(XrmRecordExtractModule))]
    [MyDescription("Search Records In Dynamics For A Specific Piece Of Text")]
    public class XrmTextSearchModule :
        TextSearchModuleBase
            <XrmTextSearchDialog, XrmTextSearchService>
    {
        public override string MainOperationName
        {
            get { return "Text Search"; }
        }

        public override string MenuGroup => "Reports";

        public override void InitialiseModule()
        {
            base.InitialiseModule();
        }

        public override void RegisterTypes()
        {
            base.RegisterTypes();
            AddTextSearchButtonToSavedConnectionsGrid();
            AddPortalDataButtonToRequestFormGrid();
            AddDialogCompletionLinks();
        }

        private void AddDialogCompletionLinks()
        {
            this.AddCustomFormFunction(new CustomFormFunction("OPENDOCUMENT", "Open Document"
                , (r) => r.ApplicationController.StartProcess(r.GetRecord().GetStringField(nameof(RecordExtractResponse.FileNameQualified)))
                , (r) => !string.IsNullOrWhiteSpace(r.GetRecord().GetStringField(nameof(RecordExtractResponse.FileName))))
                , typeof(TextSearchResponse));
            this.AddCustomFormFunction(new CustomFormFunction("OPENFOLDER", "Open Folder"
                , (r) => r.ApplicationController.StartProcess(r.GetRecord().GetStringField(nameof(RecordExtractResponse.Folder)))
                , (r) => !string.IsNullOrWhiteSpace(r.GetRecord().GetStringField(nameof(RecordExtractResponse.Folder))))
                , typeof(TextSearchResponse));
        }

        private void AddTextSearchButtonToSavedConnectionsGrid()
        {
            var customGridFunction = new CustomGridFunction("TEXTSEARCH", "Text Search", (g) =>
            {
                if (g.SelectedRows.Count() != 1)
                {
                    g.ApplicationController.UserMessage("Please Select One Row To Run This Function");
                }
                else
                {
                    var selectedRow = g.SelectedRows.First();
                    var instance = ((ObjectRecord)selectedRow.Record).Instance as SavedXrmRecordConfiguration;
                    if (instance != null)
                    {
                        var xrmRecordService = new XrmRecordService(instance, ApplicationController.ResolveType<IOrganizationConnectionFactory>(), formService: new XrmFormService());
                        var xrmTextSearchService = new XrmTextSearchService(xrmRecordService, new DocumentWriter.DocumentWriter());
                        var dialog = new XrmTextSearchDialog(xrmTextSearchService, new DialogController(ApplicationController), xrmRecordService)
                        {
                            LoadedFromConnection = true
                        };
                        dialog.SetTabLabel(instance.Name + " " + dialog.TabLabel);
                        g.ApplicationController.NavigateTo(dialog);
                    }
                }
            }, (g) => g.GridRecords != null && g.GridRecords.Any());
            this.AddCustomGridFunction(customGridFunction, typeof(SavedXrmRecordConfiguration));
        }

        private void AddPortalDataButtonToRequestFormGrid()
        {
            var customGridFunction = new CustomGridFunction("ADDPORTALDATA", "Add Portal Types", (DynamicGridViewModel g) =>
            {
                try
                {
                    var r = g.ParentForm;
                    if (r == null)
                        throw new NullReferenceException("Could Not Load The Form. The ParentForm Is Null");
                    var typesGrid = r.GetEnumerableFieldViewModel(nameof(TextSearchRequest.TypesToSearch));
                    var typesToAdd = new[]
                    {
                        "adx_contentsnippet",
                        "adx_entityform",
                        "adx_entityformmetadata",
                        "adx_entitylist",
                        "adx_entitypermission",
                        "adx_pagetemplate",
                        "adx_publishingstate",
                        "adx_sitemarker",
                        "adx_sitesetting",
                        "adx_webfile",
                        "adx_webform",
                        "adx_webformmetadata",
                        "adx_webformstep",
                        "adx_weblink",
                        "adx_weblinkset",
                        "adx_webpage",
                        "adx_webpageaccesscontrolrule",
                        "adx_webrole",
                        "adx_webtemplate",
                    };
                    var typesGridService = typesGrid.GetRecordService();
                    foreach (var item in typesToAdd.Reverse())
                    {
                        var newRecord = typesGridService.NewRecord(typeof(TextSearchRequest.TypeToSearch).AssemblyQualifiedName);
                        newRecord.SetField(nameof(TextSearchRequest.TypeToSearch.RecordType), new RecordType(item, item), typesGridService);
                        typesGrid.InsertRecord(newRecord, 0);
                    }
                }
                catch (Exception ex)
                {
                    g.ApplicationController.ThrowException(ex);
                }
            }, visibleFunction: (g) =>
            {
                var lookupService = g.RecordService.GetLookupService(nameof(TextSearchRequest.TypesToSearch) + "." + nameof(TextSearchRequest.TypeToSearch.RecordType), typeof(TextSearchRequest.TypeToSearch).AssemblyQualifiedName, nameof(TextSearchRequest.TypesToSearch), null);
                return lookupService != null && lookupService.RecordTypeExists("adx_webfile");
            });
            this.AddCustomGridFunction(customGridFunction, typeof(TextSearchRequest.TypeToSearch));
        }
    }
}