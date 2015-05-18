#region

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using JosephM.Core.Extentions;
using JosephM.Core.Log;
using JosephM.Core.Service;
using JosephM.Core.Sql;
using JosephM.Core.Utility;
using JosephM.Record.Cache;
using JosephM.Record.IService;
using JosephM.Record.Query;
using JosephM.Record.Xrm.XrmRecord;

#endregion

namespace JosephM.Xrm.ImportExporter.Service
{
    public class XrmImporterExporterService<TRecord, TService> :
        ServiceBase<XrmImporterExporterRequest, XrmImporterExporterResponse, XrmImporterExporterResponseItem>
        where TRecord : IRecord
        where TService : IRecordService
    {
        public XrmImporterExporterService(TService service)
        {
            Service = service;
        }

        private IRecordService Service { get; set; }

        //as the module structure uses Service
        //where want to use Xrm directly have this property
        private XrmService XrmService
        {
            get
            {
                if (Service is XrmRecordService)
                    return ((XrmRecordService) Service).XrmService;
                else throw new NotSupportedException(string.Format("Only Implemented Where {0} Of Type {1}", "Service", typeof(XrmRecordService).Name));
            }
        }

        public override void ExecuteExtention(XrmImporterExporterRequest request, XrmImporterExporterResponse response,
            LogController controller)
        {
            switch (request.ImportExportTask)
            {
                case ImportExportTask.ImportCsvs:
                {
                    ImportCsvs(request, controller, response);
                    break;
                }
                case ImportExportTask.ImportXml:
                {
                    Import(request, controller, response);
                    break;
                }
                case ImportExportTask.ExportXml:
                {
                    Export(request, controller, response);
                    break;
                }
            }
        }

        private void ImportCsvs(XrmImporterExporterRequest request, LogController controller, XrmImporterExporterResponse response)
        {
            var organisationSettings = new OrganisationSettings(XrmService);
            controller.LogLiteral("Preparing Import");
            var csvFiles = FileUtility.GetFiles(request.FolderPath.FolderPath).Where(f => f.EndsWith(".csv"));
            var entities = new List<Entity>();
            var countToImport = csvFiles.Count();
            var countImported = 0;
            //this part maps the csvs into entity clr objects
            //then will just call the shared import method
            foreach (var csvFile in csvFiles)
            {
                try
                {
                    controller.UpdateProgress(countImported++, countToImport, string.Format("Reading {0}", csvFile));
                    var type = GetTargetType(XrmService, csvFile);
                    var primaryField = XrmService.GetPrimaryNameField(type);
                    var rows = CsvUtility.SelectPropertyBagsFromCsv(csvFile);
                    var primaryFieldColumns =
                            rows.First()
                            .GetColumnNames()
                            .Where(c => MapColumnToFieldSchemaName(XrmService, type, c) == primaryField).ToArray();
                    var primaryFieldColumn = primaryFieldColumns.Any() ? primaryFieldColumns.First() : null;
                    if (request.MatchByName && primaryFieldColumn.IsNullOrWhiteSpace())
                        throw new NullReferenceException(string.Format("Match By Name Was Specified But No Column In The CSV Matched To The Primary Field {0} ({1})", XrmService.GetFieldLabel(primaryField, type), primaryField));
                    var rowNumber = 0;
                    foreach (var row in rows)
                    {
                        rowNumber++;
                        try
                        {
                            var entity = new Entity(type);
                            if (request.MatchByName)
                            {
                                var columnValue = row.GetFieldAsString(primaryFieldColumn);
                                var matchingEntity = GetMatchingEntities(type, primaryField, columnValue);
                                if (matchingEntity.Count() > 1)
                                    throw new Exception(string.Format("Specified Match By Name But More Than One {0} Record Matched To Name Of {1}", type, columnValue));
                                if (matchingEntity.Count() == 1)
                                    entity.Id = matchingEntity.First().Id;
                            }
                            foreach (var column in row.GetColumnNames())
                            {
                                var field = MapColumnToFieldSchemaName(XrmService, type, column);
                                if (IsIncludeField(field, type))
                                {
                                    var stringValue = row.GetFieldAsString(column);
                                    if (XrmService.IsLookup(field, type))
                                    {
                                        //for lookups am going to set to a empty guid and allow the import part to replace with a correct guid
                                        if (!stringValue.IsNullOrWhiteSpace())
                                            entity.SetField(field,
                                                new EntityReference(XrmService.GetLookupTargetEntity(field, type),
                                                    Guid.Empty)
                                                {
                                                    Name = stringValue
                                                });
                                    }
                                    else
                                    {
                                        entity.SetField(field, XrmService.ParseField(field, type, stringValue, request.DateFormat == DateFormat.American));
                                    }
                                }
                            }
                            if (XrmService.FieldExists("transactioncurrencyid", type) &&
                                !entity.GetLookupGuid("transactioncurrencyid").HasValue)
                            {
                                entity.SetLookupField("transactioncurrencyid", organisationSettings.BaseCurrencyId,
                                    "transactioncurrency");
                            }
                            entities.Add(entity);
                        }
                        catch (Exception ex)
                        {
                            response.AddResponseItem(new XrmImporterExporterResponseItem(string.Format("Error Parsing Row {0} To Entity", rowNumber), csvFile, ex));
                        }
                    }
                }
                catch (Exception ex)
                {
                    response.AddResponseItem(new XrmImporterExporterResponseItem("Not Imported", csvFile, ex));
                }
            }
            DoImport(response, entities, controller);
        }

        public string GetBaseTransactionId(IRecordService service)
        {
            var organisation = service.GetFirst("organization");
            return organisation.GetLookupId("basecurrencyid");
        }

        private IEnumerable<Entity> GetMatchingEntities(string type, string field, string value)
        {
            var conditions = new List<ConditionExpression>
            {
                new ConditionExpression(field, ConditionOperator.Equal,
                    XrmService.ParseField(field, type, value)),
            };
            if(type == "workflow")
                conditions.Add(new ConditionExpression("type", ConditionOperator.Equal, XrmPicklists.WorkflowType.Definition));
            return XrmService.RetrieveAllAndClauses(type, conditions, new String[0]);
        }

        private Entity GetUniqueMatchingEntity(string type, string field, string value)
        {
            var matchingRecords = GetMatchingEntities(type, field, value);
            if (!matchingRecords.Any())
                throw new NullReferenceException(string.Format("No Record Matched To The {0} Of {1} When Matching The Name",
                        "Name", value));
            if (matchingRecords.Count() > 1)
                throw new Exception(string.Format("More Than One Record Match To The {0} Of {1} When Matching The Name",
                    "Name", value));
            return matchingRecords.First();
        }

        private string MapColumnToFieldSchemaName(XrmService service, string type, string column)
        {
            var fields = service.GetFields(type);
            var fieldsForLabel = fields.Where(f => service.GetFieldLabel(f, type) == column);
            if (fieldsForLabel.Count() == 1)
                return fieldsForLabel.First();
            var fieldsForName = fields.Where(t => t == column);
            if (fieldsForName.Any())
                return fieldsForName.First();
            throw new NullReferenceException(string.Format("No Unique Field Found On Record Type {0} Matched (Label Or Name) For Column Of {1}", type, column));
        }

        private string GetTargetType(XrmService service, string csvName)
        {
            var name = csvName;
            if(name.EndsWith(".csv"))
                name = name.Substring(0, name.IndexOf(".csv", StringComparison.Ordinal));
            name = Path.GetFileName(name);
            var recordTypes = service.GetAllEntityTypes();
            var typesForLabel = recordTypes.Where(t => service.GetEntityDisplayName(t) == name || service.GetEntityCollectionName(t) == name);
            if (typesForLabel.Count() == 1)
                return typesForLabel.First();
            var typesForName = recordTypes.Where(t => t == name);
            if (typesForName.Any())
                return typesForName.First();
            throw new NullReferenceException(string.Format("No Unique Record Type Matched (Label Or Name) For CSV Name Of {0}", name));
        }

        private void Import(XrmImporterExporterRequest request, LogController controller,
            XrmImporterExporterResponse response)
        {
            var folder = request.FolderPath.FolderPath;
            var lateBoundSerializer = new DataContractSerializer(typeof(Entity));

            var entities = new List<Entity>();
            foreach (var file in Directory.GetFiles(folder, "*.xml"))
            {
                using (var fileStream = new FileStream(file, FileMode.Open))
                {
                    entities.Add((Entity)lateBoundSerializer.ReadObject(fileStream));
                }
            }

            DoImport(response, entities, controller);
        }

        private void DoImport(XrmImporterExporterResponse response, List<Entity> entities, LogController controller)
        {
            controller.LogLiteral("Preparing Import");
            var fieldsToRetry = new Dictionary<Entity, List<string>>();
            var typesToImport = entities.Select(e => e.LogicalName).Distinct();
            var orderedTypes = new List<string>();

            #region tryordertypes

            foreach (var type in typesToImport)
            {
                var thisTypeEntities = entities.Where(e => e.LogicalName == type).ToList();
                var fields = GetFieldsToImport(thisTypeEntities, type);
                foreach (var type2 in orderedTypes)
                {
                    if (
                        fields.Any(
                            f => XrmService.IsLookup(f, type) && XrmService.GetLookupTargetEntity(f, type) == type2))
                    {
                        type.Insert(orderedTypes.IndexOf(type2), type);
                    }
                    break;
                }
                if (!orderedTypes.Contains(type))
                    orderedTypes.Add(type);
            }

            #endregion tryordertypes

            var countToImport = orderedTypes.Count;
            var countImported = 0;
            foreach (var recordType in orderedTypes)
            {
                try
                {
                    controller.UpdateProgress(countImported++, countToImport, string.Format("Importing {0} Records", recordType));
                    var primaryField = XrmService.GetPrimaryNameField(recordType);
                    var thisTypeEntities = entities.Where(e => e.LogicalName == recordType).ToList();

                    var orConditions = thisTypeEntities
                        .Select(
                            e =>
                                new ConditionExpression(XrmService.GetPrimaryKeyField(e.LogicalName),
                                    ConditionOperator.Equal, e.Id))
                        .ToArray();
                    var existingEntities = XrmService.RetrieveAllOrClauses(recordType, orConditions);

                    var orderedEntities = new List<Entity>();

                    #region tryorderentities

                    var selfReferenceFields =
                        GetFieldsToImport(thisTypeEntities, recordType)
                            .Where(
                                f =>
                                    XrmService.IsLookup(f, recordType) &&
                                    XrmService.GetLookupTargetEntity(f, recordType) == recordType);
                    foreach (var entity in thisTypeEntities)
                    {
                        foreach (var entity2 in orderedEntities)
                        {
                            if (selfReferenceFields.Any(f => entity.GetLookupGuid(f) == entity2.Id || (entity.GetLookupGuid(f) == Guid.Empty && entity.GetLookupName(f) == entity.GetStringField(primaryField))))
                            {
                                orderedEntities.Insert(orderedEntities.IndexOf(entity2), entity);
                                break;
                            }
                        }
                        if (!orderedEntities.Contains(entity))
                            orderedEntities.Add(entity);
                    }

                    #endregion tryorderentities
                    var countRecordsToImport = orderedEntities.Count;
                    var countRecordsImported = 0;
                    foreach (var entity in orderedEntities)
                    {
                        var thisEntity = entity;
                        try
                        {
                            controller.UpdateLevel2Progress(countRecordsImported++, countRecordsToImport, string.Format("Importing {0} Records", recordType));
                            var existingMatchingIds = GetMatchForExistingRecord(existingEntities, thisEntity);
                            if (existingMatchingIds.Any())
                            {
                                var matchRecord = existingMatchingIds.First();
                                thisEntity.Id = matchRecord.Id;
                                thisEntity.SetField(XrmService.GetPrimaryKeyField(thisEntity.LogicalName), thisEntity.Id);
                            }
                            var isUpdate = existingMatchingIds.Any();
                            foreach (var field in thisEntity.GetFieldsInEntity().ToArray())
                            {
                                if (IsIncludeField(field, recordType) &&
                                    XrmService.IsLookup(field, thisEntity.LogicalName) &&
                                    thisEntity.GetField(field) != null)
                                {
                                    var lookupEntity = thisEntity.GetLookupType(field);
                                    var idNullable = thisEntity.GetLookupGuid(field);
                                    if (idNullable.HasValue)
                                    {
                                        var name = thisEntity.GetLookupName(field);
                                        var targetPrimaryKey = Service.GetPrimaryKey(lookupEntity);
                                        var targetPrimaryField = Service.GetPrimaryField(lookupEntity);
                                        var matchRecord = XrmService.GetFirst(lookupEntity, targetPrimaryKey,
                                            idNullable.Value);
                                        if (matchRecord != null)
                                            thisEntity.SetLookupField(field, matchRecord);
                                        else
                                        {
                                            var matchRecords = name.IsNullOrWhiteSpace() ?
                                                new Entity[0] :
                                                GetMatchingEntities(lookupEntity,
                                                targetPrimaryField,
                                                name);
                                            if (matchRecords.Count() != 1)
                                            {
                                                if (!fieldsToRetry.ContainsKey(thisEntity))
                                                    fieldsToRetry.Add(thisEntity, new List<string>());
                                                fieldsToRetry[thisEntity].Add(field);
                                            }
                                            else
                                                thisEntity.SetLookupField(field, matchRecords.First());
                                        }
                                    }
                                }
                            }
                            var fieldsToSet = new List<string>();
                            fieldsToSet.AddRange(thisEntity.GetFieldsInEntity()
                                .Where(f => IsIncludeField(f, recordType)));
                            if (fieldsToRetry.ContainsKey(thisEntity))
                                fieldsToSet.RemoveAll(f => fieldsToRetry[thisEntity].Contains(f));
                            if (isUpdate)
                            {
                                var existingRecord = existingMatchingIds.First();
                                XrmService.Update(thisEntity, fieldsToSet.Where(f => !XrmEntity.FieldsEqual(existingRecord.GetField(f), thisEntity.GetField(f))));
                            }
                            else
                            {
                                CheckThrowValidForCreate(thisEntity, fieldsToSet);
                                thisEntity.Id = XrmService.Create(thisEntity, fieldsToSet);
                            }
                        }
                        catch (Exception ex)
                        {
                            if (fieldsToRetry.ContainsKey(thisEntity))
                                fieldsToRetry.Remove(thisEntity);
                            response.AddResponseItem(
                                new XrmImporterExporterResponseItem(
                                    string.Format("Error Importing Record Of Type {0} Id {1}", entity.LogicalName,
                                        entity.Id),
                                    ex));
                        }
                    }
                }
                catch (Exception ex)
                {
                    response.AddResponseItem(
                        new XrmImporterExporterResponseItem(string.Format("Error Importing Type {0}", recordType), ex));
                }
            }
            controller.TurnOffLevel2();
            countToImport = fieldsToRetry.Count;
            countImported = 0;
            foreach (var item in fieldsToRetry)
            {
                var thisEntity = item.Key;
                try
                {
                    controller.UpdateProgress(countImported++, countToImport, string.Format("Retrying Unresolved Fields {0}", thisEntity.LogicalName));
                    foreach (var field in item.Value)
                    {
                        if (XrmService.IsLookup(field, thisEntity.LogicalName) && thisEntity.GetField(field) != null)
                        {
                            try
                            {
                                var lookupEntity = thisEntity.GetLookupType(field);
                                var idNullable = thisEntity.GetLookupGuid(field);
                                if (idNullable.HasValue)
                                {
                                    var name = thisEntity.GetLookupName(field);
                                    var primaryKey = Service.GetPrimaryKey(lookupEntity);
                                    var primaryField = Service.GetPrimaryField(lookupEntity);
                                    var matchRecord = XrmService.GetFirst(lookupEntity, primaryKey,
                                        idNullable.Value);
                                    if (matchRecord != null)
                                        thisEntity.SetLookupField(field, matchRecord);
                                    else
                                    {
                                        if(name.IsNullOrWhiteSpace())
                                            throw new NullReferenceException(string.Format("No Record Matched By Id and Name Is Empty"));
                                        matchRecord = GetUniqueMatchingEntity(lookupEntity,
                                            primaryField,
                                            name);
                                        thisEntity.SetLookupField(field, matchRecord);
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                if (thisEntity.Contains(field))
                                    thisEntity.Attributes.Remove(field);
                                response.AddResponseItem(
                                    new XrmImporterExporterResponseItem(
                                        string.Format(
                                            "Warning Error Setting Lookup Field {0} Record Of Type {1} Id {2}", field,
                                            thisEntity.LogicalName, thisEntity.Id), ex));
                            }
                        }
                    }
                    XrmService.Update(thisEntity, item.Value);
                }
                catch (Exception ex)
                {
                    response.AddResponseItem(
                        new XrmImporterExporterResponseItem(
                            string.Format("Error Importing Record Of Type {0} Id {1}", thisEntity.LogicalName,
                                thisEntity.Id),
                            ex));
                }
            }
        }

        private IEnumerable<Entity> GetMatchForExistingRecord(IEnumerable<Entity> existingEntitiesWithIdMatches, Entity thisEntity)
        {
            //this a bit messy
            var existingMatches = existingEntitiesWithIdMatches.Where(e => e.Id == thisEntity.Id);
            if (!existingMatches.Any())
            {
                var matchByNameEntities = new[] {"businessunit", "team"};
                if (thisEntity.LogicalName == "businessunit" && thisEntity.GetField("parentbusinessunitid") == null)
                {
                    existingMatches = XrmService.RetrieveAllAndClauses("businessunit",
                        new[] {new ConditionExpression("parentbusinessunitid", ConditionOperator.Null)});
                }
                else if (matchByNameEntities.Contains(thisEntity.LogicalName))
                {
                    var primaryField = XrmService.GetPrimaryNameField(thisEntity.LogicalName);
                    var name = thisEntity.GetStringField(primaryField);
                    if(name.IsNullOrWhiteSpace())
                        throw new NullReferenceException(string.Format("{0} Is Required On The {1}", XrmService.GetFieldLabel(primaryField, thisEntity.LogicalName), XrmService.GetEntityLabel(thisEntity.LogicalName)));
                    existingMatches = GetMatchingEntities(thisEntity.LogicalName, primaryField, name);
                    if(existingMatches.Count() > 1)
                        throw new Exception(string.Format("More Than One Record Match To The {0} Of {1} When Matching The Name",
                            "Name", name));
                }
            }
            return existingMatches;
        }

        private void CheckThrowValidForCreate(Entity thisEntity, List<string> fieldsToSet)
        {
            if (thisEntity != null)
            {
                switch (thisEntity.LogicalName)
                {
                    case "annotation" :
                        if(!fieldsToSet.Contains("objectid"))
                            throw new NullReferenceException(string.Format("Cannot create {0} {1} as its parent {2} does not exist"
                                , XrmService.GetEntityLabel(thisEntity.LogicalName), thisEntity.GetStringField(XrmService.GetPrimaryNameField(thisEntity.LogicalName))
                                , thisEntity.GetStringField("objecttypecode") != null ? XrmService.GetEntityLabel(thisEntity.GetStringField("objecttypecode")) : "Unknown Type"));
                        break;
                }
            }
            return;
        }

        private IEnumerable<string> GetFieldsToImport(List<Entity> thisTypeEntities, string type)
        {
            var fields = thisTypeEntities.SelectMany(e => e.GetFieldsInEntity().Where(f => IsIncludeField(f, type)));
            return fields;
        }

        public bool IsIncludeField(string fieldName, string entityType)
        {
            var hardcodeInvalidFields = new[]
            {
                "administratorid", "owneridtype", "ownerid", "timezoneruleversionnumber", "utcconversiontimezonecode", "organizationid", "owninguser", "owningbusinessunit","owningteam",
                "overriddencreatedon", "statuscode", "statecode", "createdby", "createdon", "modifiedby", "modifiedon", "modifiedon", "myr_currentnumberposition"
            };
            if (hardcodeInvalidFields.Contains(fieldName))
                return false;
            if (fieldName == "parentbusinessunitid")
                return true;
            if (fieldName == "businessunitid")
                return true;
            return
                Service.IsWritable(fieldName, entityType);
               
        }

        private void Export(XrmImporterExporterRequest request, LogController controller,
            XrmImporterExporterResponse response)
        {
            var folder = request.FolderPath.FolderPath;
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);
            if (request.RecordTypes == null || !request.RecordTypes.Any())
                throw new Exception("Error no Record Types To Export");
            var countToExport = request.RecordTypes.Count();
            var countsExported = 0;
            foreach (var type in request.RecordTypes.Select(r => r.RecordType.Key))
            {
                controller.UpdateProgress(countsExported++, countToExport, string.Format("Exporting {0} Records", type));
                var conditions = new List<ConditionExpression>();
                if (type == "list")
                    conditions.Add(new ConditionExpression("type", ConditionOperator.Equal, XrmPicklists.ListType.Dynamic));
                var entities = XrmService.RetrieveAllAndClauses(type, conditions);
                var lateBoundSerializer = new DataContractSerializer(typeof(Entity));

                foreach (var entity in entities)
                {
                    if (!CheckIgnoreForExport(request, entity))
                    {
                        var namesToUse =
                            entity.GetStringField(XrmService.GetPrimaryNameField(entity.LogicalName).Left(15));
                        if (!namesToUse.IsNullOrWhiteSpace())
                        {
                            var invalidChars = Path.GetInvalidFileNameChars();
                            foreach (var character in invalidChars)
                                namesToUse = namesToUse.Replace(character, '_');
                        }
                        var fileName = string.Format(@"{0}\{1}_{2}_{3}.xml", folder, entity.LogicalName, entity.Id,
                            namesToUse);
                        using (var fileStream = new FileStream(fileName, FileMode.Create))
                        {
                            lateBoundSerializer.WriteObject(fileStream, entity);
                        }
                    }
                }
            }
        }

        private bool CheckIgnoreForExport(XrmImporterExporterRequest request, Entity entity)
        {
            //exlcude 1 = public
            if (entity.LogicalName == "queue" && entity.GetOptionSetValue("queueviewtype") == 1)
                return true;
            return false;
        }
    }
}