using JosephM.Application.Modules;
using JosephM.Application.ViewModel.Extentions;
using JosephM.Application.ViewModel.Grid;
using JosephM.Application.ViewModel.SettingTypes;
using JosephM.Core.FieldType;
using JosephM.Deployment.ExportXml;
using JosephM.Record.Extentions;
using JosephM.Record.Query;
using JosephM.Xrm.Schema;
using System;
using System.Collections.Generic;
using System.Linq;

namespace JosephM.Deployment
{
    public class AddPortalDataModule : ModuleBase
    {
        public override void InitialiseModule()
        {
        }

        public override void RegisterTypes()
        {
            AddPortalDataButtonToExportRecordTypeGrid();
        }

        private void AddPortalDataButtonToExportRecordTypeGrid()
        {
            var portalTypesToAdd = new[]
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

            var childButtons = new List<CustomGridFunction>();
            childButtons.Add(new CustomGridFunction("ADDPORTALDATAALL", "All Records", (DynamicGridViewModel g) =>
            {
                var r = g.ParentForm;
                if (r == null)
                    throw new NullReferenceException("Could Not Load The Form. The ParentForm Is Null");
                try
                {
                    var enumerableField = r.GetEnumerableFieldViewModel(g.ReferenceName);

                    foreach (var item in portalTypesToAdd.Reverse())
                    {
                        var newRecord = g.RecordService.NewRecord(typeof(ExportRecordType).AssemblyQualifiedName);
                        newRecord.SetField(nameof(ExportRecordType.RecordType), new RecordType(item, item), g.RecordService);
                        newRecord.SetField(nameof(ExportRecordType.IncludeInactive), true, g.RecordService);
                        enumerableField.InsertRecord(newRecord, 0);
                    }
                }
                catch (Exception ex)
                {
                    g.ApplicationController.ThrowException(ex);
                }
            }, visibleFunction: (g) =>
            {
                var lookupService = g.RecordService.GetLookupService(nameof(ExportRecordType.RecordType), typeof(ExportRecordType).AssemblyQualifiedName, g.ReferenceName, null);
                return lookupService != null && lookupService.RecordTypeExists("adx_webfile");
            }));
            var todayUtc = DateTime.Today.ToUniversalTime();
            var modifedConditions = new Dictionary<string, Condition>
            {
                { "Edited Today", new Condition("modifiedon", ConditionType.Today) },
                { "Edited Last 7 Days", new Condition("modifiedon", ConditionType.GreaterEqual, todayUtc.AddDays(-7)) },
                { "Edited Last 2 Weeks", new Condition("modifiedon", ConditionType.GreaterEqual, todayUtc.AddDays(-14)) },
                { "Edited Last Month", new Condition("modifiedon", ConditionType.GreaterEqual, todayUtc.AddMonths(-1)) },
            };
            foreach(var modifiedPeriod in modifedConditions)
            {
                childButtons.Add(new CustomGridFunction("ADDPORTALDATA" + modifiedPeriod.Key, modifiedPeriod.Key, (DynamicGridViewModel g) =>
                {
                    var recordForm = g.ParentForm;
                    if (recordForm == null)
                        throw new NullReferenceException("Could Not Load The Form. The ParentForm Is Null");
                    recordForm.LoadingViewModel.IsLoading = true;
                    g.ApplicationController.DoOnAsyncThread(() =>
                    {
                        try
                        {
                            var enumerableField = recordForm.GetEnumerableFieldViewModel(g.ReferenceName);
                            var itemsAdded = false;
                            var lookupService = g.RecordService.GetLookupService(nameof(ExportRecordType.RecordType), typeof(ExportRecordType).AssemblyQualifiedName, g.ReferenceName, null);
                            foreach (var item in portalTypesToAdd.Reverse())
                            {
                                var query = new QueryDefinition(item);
                                query.RootFilter.Conditions.Add(modifiedPeriod.Value);
                                var itemsLookups = lookupService
                                    .RetreiveAll(query)
                                    .Select(r => lookupService.ToLookupWithAltDisplayNameName(r))
                                    .ToList();
                                if (item == Entities.adx_webfile)
                                {
                                    var queryNotes = new QueryDefinition(Entities.annotation);
                                    queryNotes.RootFilter.Conditions.Add(modifiedPeriod.Value);
                                    queryNotes.Joins.Add(new Join(Fields.annotation_.objectid, Entities.adx_webfile, Fields.adx_webfile_.adx_webfileid));
                                    var webFileNotes = lookupService.RetreiveAll(queryNotes);
                                    webFileNotes.PopulateEmptyLookups(lookupService, null);
                                    foreach(var webFileNote in webFileNotes)
                                    {
                                        if (!itemsLookups.Any(wf => wf.Id == webFileNote.GetLookupId(Fields.annotation_.objectid)))
                                        {
                                            itemsAdded = true;
                                            itemsLookups.Add(webFileNote.GetLookupField(Fields.annotation_.objectid));
                                        }
                                    }
                                }
                                if (itemsLookups.Any())
                                {
                                    var newRecord = g.RecordService.NewRecord(typeof(ExportRecordType).AssemblyQualifiedName);
                                    newRecord.SetField(nameof(ExportRecordType.RecordType), new RecordType(item, item), g.RecordService);
                                    newRecord.SetField(nameof(ExportRecordType.Type), ExportType.SpecificRecords, g.RecordService);
                                    newRecord.SetField(nameof(ExportRecordType.IncludeInactive), true, g.RecordService);
                                    newRecord.SetField(nameof(ExportRecordType.SpecificRecordsToExport), itemsLookups.Select(r => new LookupSetting
                                    {
                                        Record = r
                                    }).ToArray(), g.RecordService);
                                    enumerableField.InsertRecord(newRecord, 0);
                                    itemsAdded = true;
                                }
                            }
                            if (!itemsAdded)
                                g.ApplicationController.UserMessage("No Items Were Found To Add For The Selected Period");
                        }
                        catch (Exception ex)
                        {
                            g.ApplicationController.ThrowException(ex);
                        }
                        finally
                        {
                            recordForm.LoadingViewModel.IsLoading = false;
                        }
                    });
                }, visibleFunction: (g) =>
                {
                    var lookupService = g.RecordService.GetLookupService(nameof(ExportRecordType.RecordType), typeof(ExportRecordType).AssemblyQualifiedName, g.ReferenceName, null);
                    return lookupService != null && lookupService.RecordTypeExists("adx_webfile");
                }));
            }

            this.AddCustomGridFunction(new CustomGridFunction("ADDPORTALDATA", "Add Portal Data", childButtons), typeof(ExportRecordType));
        }
    }
}