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
                        Entities.adx_contentsnippet,
                        Entities.adx_entityform,
                        Entities.adx_entityformmetadata,
                        Entities.adx_entitylist,
                        Entities.adx_entitypermission,
                        Entities.adx_pagetemplate,
                        Entities.adx_publishingstate,
                        Entities.adx_sitemarker,
                        Entities.adx_sitesetting,
                        Entities.adx_webfile,
                        Entities.adx_webform,
                        Entities.adx_webformmetadata,
                        Entities.adx_webformstep,
                        Entities.adx_weblink,
                        Entities.adx_weblinkset,
                        Entities.adx_webpage,
                        Entities.adx_webpageaccesscontrolrule,
                        Entities.adx_webrole,
                        Entities.adx_webtemplate,
                        Entities.adx_contentaccesslevel,
                    };

            var associationsToAdd = new Dictionary<string, IEnumerable<string>>
            {
                { Entities.adx_contentaccesslevel, new [] { Relationships.adx_contentaccesslevel_.adx_WebRoleContentAccessLevel.Name } },
                { Entities.adx_entitypermission, new [] { Relationships.adx_entitypermission_.adx_entitypermission_webrole.Name } },
                { Entities.adx_publishingstate, new [] { Relationships.adx_publishingstate_.adx_accesscontrolrule_publishingstate.Name } },
                { Entities.adx_webpageaccesscontrolrule, new [] { Relationships.adx_webpageaccesscontrolrule_.adx_webpageaccesscontrolrule_webrole.Name, Relationships.adx_webpageaccesscontrolrule_.adx_accesscontrolrule_publishingstate.Name } },
                { Entities.adx_webrole, new [] { Relationships.adx_webrole_.adx_entitypermission_webrole.Name, Relationships.adx_webrole_.adx_webpageaccesscontrolrule_webrole.Name, Relationships.adx_webrole_.adx_WebRoleContentAccessLevel.Name } },
            };

            var childButtons = new List<CustomGridFunction>();
            childButtons.Add(new CustomGridFunction("ADDPORTALDATAALL", "All Records", (DynamicGridViewModel g) =>
            {
                try
                {
                    var r = g.ParentForm;
                    if (r == null)
                        throw new NullReferenceException("Could Not Load The Form. The ParentForm Is Null");

                    ApplicationController.LogEvent("Add Portal Data Loaded", new Dictionary<string, string>
                    {
                        { "Filter", "All" }
                    });

                    var enumerableField = r.GetEnumerableFieldViewModel(g.ReferenceName);

                    foreach (var item in portalTypesToAdd.OrderByDescending(i => i))
                    {
                        var newRecord = g.RecordService.NewRecord(typeof(ExportRecordType).AssemblyQualifiedName);
                        newRecord.SetField(nameof(ExportRecordType.RecordType), new RecordType(item, item), g.RecordService);
                        newRecord.SetField(nameof(ExportRecordType.IncludeInactive), true, g.RecordService);
                        enumerableField.InsertRecord(newRecord, 0);
                    }

                    ApplicationController.LogEvent("Add Portal Data Completed", new Dictionary<string, string>
                    {
                        { "Filter", "All" }
                    });
                }
                catch (Exception ex)
                {
                    g.ApplicationController.ThrowException(ex);
                }
            }, visibleFunction: (g) =>
            {
                var lookupService = g.RecordService.GetLookupService(nameof(ExportRecordType.RecordType), typeof(ExportRecordType).AssemblyQualifiedName, g.ReferenceName, null);
                return lookupService != null && lookupService.RecordTypeExists(Entities.adx_webfile);
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
                    ApplicationController.LogEvent("Add Portal Data Loaded", new Dictionary<string, string>
                    {
                        { "Filter", modifiedPeriod.Key }
                    });

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

                            var dictionaryOfRecordsToAdd = new Dictionary<string, List<Lookup>>();

                            foreach (var item in portalTypesToAdd.Reverse())
                            {
                                try
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
                                        foreach (var webFileNote in webFileNotes)
                                        {
                                            if (!itemsLookups.Any(wf => wf.Id == webFileNote.GetLookupId(Fields.annotation_.objectid)))
                                            {
                                                itemsAdded = true;
                                                itemsLookups.Add(webFileNote.GetLookupField(Fields.annotation_.objectid));
                                            }
                                        }
                                    }
                                    dictionaryOfRecordsToAdd.Add(item, itemsLookups);
                                }
                                catch(Exception ex)
                                {
                                    throw new Exception($"Error Querying For Portal Type '{item}'", ex);
                                }
                            }
                            var copyDictionary = dictionaryOfRecordsToAdd.ToDictionary(d => d.Key, d => d.Value.ToArray());
                            foreach (var itemAddInInitialQueries in copyDictionary)
                            {
                                var thisType = itemAddInInitialQueries.Key;
                                if (itemAddInInitialQueries.Value.Any() && associationsToAdd.ContainsKey(itemAddInInitialQueries.Key))
                                {
                                    foreach(var nnRelationship in associationsToAdd[itemAddInInitialQueries.Key])
                                    {
                                        var rMetadata = lookupService.GetManyRelationshipMetadata(nnRelationship, thisType);
                                        var thisSideKey = lookupService.GetPrimaryKey(thisType);
                                        var otherType = rMetadata.RecordType1 == thisType ? rMetadata.RecordType2 : rMetadata.RecordType1;
                                        var otherSideKey = lookupService.GetPrimaryKey(otherType);
                                        var associatedIds = lookupService.RetrieveAllOrClauses(rMetadata.IntersectEntityName,
                                            itemAddInInitialQueries
                                            .Value
                                            .Select(l => new Condition(thisSideKey, ConditionType.Equal, l.Id)), new[] { otherSideKey })
                                            .Select(a => a.GetIdField(otherSideKey))
                                            .Distinct()
                                            .ToArray();
                                        if (associatedIds.Any())
                                        {
                                            if (!dictionaryOfRecordsToAdd.ContainsKey(otherType))
                                                dictionaryOfRecordsToAdd.Add(otherType, new List<Lookup>());
                                            var associatedRecords = lookupService.RetrieveAllOrClauses(otherType,
                                                associatedIds
                                                .Select(id => new Condition(otherSideKey, ConditionType.Equal, id)), null);
                                            foreach (var associatedRecord in associatedRecords)
                                            {
                                                if (!dictionaryOfRecordsToAdd[otherType].Any(l => l.Id == associatedRecord.Id))
                                                    dictionaryOfRecordsToAdd[otherType].Add(lookupService.ToLookupWithAltDisplayNameName(associatedRecord));
                                            }
                                        }
                                    }
                                }
                            }

                            foreach (var item in dictionaryOfRecordsToAdd.OrderByDescending(kv => kv.Key))
                            {
                                if (item.Value.Any())
                                {
                                    var newRecord = g.RecordService.NewRecord(typeof(ExportRecordType).AssemblyQualifiedName);
                                    newRecord.SetField(nameof(ExportRecordType.RecordType), new RecordType(item.Key, item.Key), g.RecordService);
                                    newRecord.SetField(nameof(ExportRecordType.Type), ExportType.SpecificRecords, g.RecordService);
                                    newRecord.SetField(nameof(ExportRecordType.IncludeInactive), true, g.RecordService);
                                    newRecord.SetField(nameof(ExportRecordType.SpecificRecordsToExport), item.Value.Select(r => new LookupSetting
                                    {
                                        Record = r
                                    }).ToArray(), g.RecordService);
                                    enumerableField.InsertRecord(newRecord, 0);
                                    itemsAdded = true;
                                }
                            }

                            var eventArgs = new Dictionary<string, string>
                            {
                                { "Filter", modifiedPeriod.Key }
                            };
                            foreach(var itemType in dictionaryOfRecordsToAdd)
                            {
                                if (itemType.Value.Count() > 0)
                                    eventArgs.Add(itemType.Key + " Count", itemType.Value.Count().ToString());
                            }
                            ApplicationController.LogEvent("Add Portal Data Completed", eventArgs);

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
                    return lookupService != null && lookupService.RecordTypeExists(Entities.adx_webfile);
                }));
            }

            this.AddCustomGridFunction(new CustomGridFunction("ADDPORTALDATA", "Add Portal Data", childButtons), typeof(ExportRecordType));
        }
    }
}