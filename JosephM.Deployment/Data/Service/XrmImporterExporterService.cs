#region

using JosephM.Core.Extentions;
using JosephM.Core.FieldType;
using JosephM.Core.Log;
using JosephM.Core.Service;
using JosephM.Core.Utility;
using JosephM.Record.Extentions;
using JosephM.Record.IService;
using JosephM.Record.Xrm.XrmRecord;
using JosephM.Xrm.Schema;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;

#endregion

namespace JosephM.Xrm.ImportExporter.Service
{
    public class XrmImporterExporterService<TService> :
        ServiceBase<XrmImporterExporterRequest, XrmImporterExporterResponse, XrmImporterExporterResponseItem>
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
                    return ((XrmRecordService)Service).XrmService;
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
                        ImportXml(request.Folder.FolderPath, controller, response);
                        break;
                    }
                case ImportExportTask.ExportXml:
                    {
                        ExportXml(request.RecordTypesToExport, request.Folder, request.IncludeNotes, request.IncludeNNRelationshipsBetweenEntities, controller);
                        break;
                    }
            }
        }

        private void ImportCsvs(XrmImporterExporterRequest request, LogController controller, XrmImporterExporterResponse response)
        {
            var organisationSettings = new OrganisationSettings(XrmService);
            controller.LogLiteral("Preparing Import");
            var csvFiles = request.FolderOrFiles == XrmImporterExporterRequest.CsvImportOption.Folder
                ? FileUtility.GetFiles(request.Folder.FolderPath).Where(f => f.EndsWith(".csv"))
                : request.CsvsToImport.Select(c => c.Csv.FileName).ToArray();

            var entities = new List<Entity>();
            var countToImport = csvFiles.Count();
            var countImported = 0;
            //this part maps the csvs into entity clr objects
            //then will just call the shared import method
            foreach (var csvFile in csvFiles)
            {
                try
                {
                    //try think if better way
                    controller.UpdateProgress(countImported++, countToImport, string.Format("Reading {0}", csvFile));
                    var getTypeResponse = GetTargetType(XrmService, csvFile);
                    var type = getTypeResponse.LogicalName;
                    var primaryField = XrmService.GetPrimaryNameField(type);
                    var fileInfo = new FileInfo(csvFile);
                    CsvUtility.ConstructTextSchema(fileInfo.Directory.FullName, Path.GetFileName(csvFile));
                    var rows = CsvUtility.SelectAllRows(csvFile);
                    var rowNumber = 0;
                    foreach (var row in rows)
                    {
                        rowNumber++;
                        try
                        {
                            var entity = new Entity(type);
                            var keyColumns =
                                    rows.First()
                                    .GetColumnNames()
                                    .Where(c => c.StartsWith("key|"));
                             
                            if(keyColumns.Any())
                            {
                                var fieldValues = new Dictionary<string, object>();
                                foreach(var key in keyColumns)
                                {
                                    var fieldName = MapColumnToFieldSchemaName(XrmService, type, key);
                                    var columnValue = row.GetFieldAsString(key);
                                    if (columnValue != null)
                                        columnValue = columnValue.Trim();

                                    fieldValues.Add(fieldName, columnValue);
                                }
                                var matchingEntity = GetMatchingEntities(type, fieldValues);
                                if (matchingEntity.Count() > 1)
                                    throw new Exception(string.Format("Specified Match By Name But More Than One {0} Record Matched To The Keys Of {1}", type, string.Join(",", fieldValues.Select(kv => kv.Key + "=" + kv.Value))));
                                if (matchingEntity.Count() == 1)
                                    entity.Id = matchingEntity.First().Id;
                            }
                            else if (request.MatchByName && !getTypeResponse.IsRelationship)
                            {
                                var primaryFieldColumns =
                                    rows.First()
                                    .GetColumnNames()
                                    .Where(c => MapColumnToFieldSchemaName(XrmService, type, c) == primaryField).ToArray();
                                var primaryFieldColumn = primaryFieldColumns.Any() ? primaryFieldColumns.First() : null;
                                if (request.MatchByName && primaryFieldColumn.IsNullOrWhiteSpace())
                                    throw new NullReferenceException(string.Format("Match By Name Was Specified But No Column In The CSV Matched To The Primary Field {0} ({1})", XrmService.GetFieldLabel(primaryField, type), primaryField));

                                var columnValue = row.GetFieldAsString(primaryFieldColumn);
                                if (columnValue != null)
                                    columnValue = columnValue.Trim();
                                var matchingEntity = GetMatchingEntities(type, primaryField, columnValue);
                                if (matchingEntity.Count() > 1)
                                    throw new Exception(string.Format("Specified Match By Name But More Than One {0} Record Matched To Name Of {1}", type, columnValue));
                                if (matchingEntity.Count() == 1)
                                    entity.Id = matchingEntity.First().Id;
                            }
                            foreach (var column in row.GetColumnNames())
                            {
                                var field = MapColumnToFieldSchemaName(XrmService, type, column);
                                if (true)//(getTypeResponse.IsRelationship || XrmService.IsWritable(field, type))
                                {
                                    var stringValue = row.GetFieldAsString(column);
                                    if (stringValue != null)
                                        stringValue = stringValue.Trim();
                                    if (getTypeResponse.IsRelationship)
                                    {
                                        //bit of hack
                                        //for csv relationships just set to a string and map it later
                                        //as the referenced record may not be created yet
                                        entity.SetField(field, stringValue);
                                    }
                                    else if (XrmService.IsLookup(field, type))
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

        private IEnumerable<Entity> GetMatchingEntities(string type, IDictionary<string,object> fieldValues)
        {
            var conditions = fieldValues.Select(fv =>
            fv.Value == null
            ? new ConditionExpression(fv.Key, ConditionOperator.Null)
            : new ConditionExpression(fv.Key, ConditionOperator.Equal, XrmService.ConvertToQueryValue(fv.Key, type, XrmService.ParseField(fv.Key, type, fv.Value)))
            ).ToList();
            if (type == "workflow")
                conditions.Add(new ConditionExpression("type", ConditionOperator.Equal, XrmPicklists.WorkflowType.Definition));
            if (type == "account" || type == "contact")
                conditions.Add(new ConditionExpression("merged", ConditionOperator.NotEqual, true));
            if (type == "knowledgearticle")
                conditions.Add(new ConditionExpression("islatestversion", ConditionOperator.Equal, true));
            return XrmService.RetrieveAllAndClauses(type, conditions, new String[0]);
        }

        private IEnumerable<Entity> GetMatchingEntities(string type, string field, string value)
        {
            return GetMatchingEntities(type, new Dictionary<string, object>()
            {
                { field, value }
            });
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
            if (column.StartsWith("key|"))
                column = column.Substring(4);
            var fields = service.GetFields(type);
            var fieldsForLabel = fields.Where(f => service.GetFieldLabel(f, type) == column);
            if (fieldsForLabel.Count() == 1)
                return fieldsForLabel.First();
            var fieldsForName = fields.Where(t => t.ToLower() == column.ToLower());
            if (fieldsForName.Any())
                return fieldsForName.First();
            throw new NullReferenceException(string.Format("No Unique Field Found On Record Type {0} Matched (Label Or Name) For Column Of {1}", type, column));
        }

        private class GetTargetTypeResponse
        {
            public GetTargetTypeResponse(string logicalName, bool isRelationship)
            {
                LogicalName = logicalName;
                IsRelationship = isRelationship;
            }

            public bool IsRelationship { get; set; }
            public string LogicalName { get; set; }
        }

        private GetTargetTypeResponse GetTargetType(XrmService service, string csvName)
        {
            var name = csvName;
            if (name.EndsWith(".csv"))
                name = name.Substring(0, name.IndexOf(".csv", StringComparison.Ordinal));
            name = Path.GetFileName(name);
            var recordTypes = service.GetAllEntityTypes();
            var typesForLabel = recordTypes.Where(t => service.GetEntityDisplayName(t) == name || service.GetEntityCollectionName(t) == name);
            if (typesForLabel.Count() == 1)
                return new GetTargetTypeResponse(typesForLabel.First(), false);
            var typesForName = recordTypes.Where(t => t == name);
            if (typesForName.Any())
                return new GetTargetTypeResponse(typesForName.First(), false);

            var relationshipEntities = service.GetAllNnRelationshipEntityNames();
            var matchingRelationships = relationshipEntities.Where(r => r == name);
            if (matchingRelationships.Count() == 1)
                return new GetTargetTypeResponse(matchingRelationships.First(), true);

            throw new NullReferenceException(string.Format("No Unique Record Type Or Relationship Matched (Label Or Name) For CSV Name Of {0}", name));
        }

        public void ImportXml(string folder, LogController controller,
            XrmImporterExporterResponse response)
        {
            var entities = LoadEntitiesFromXmlFiles(folder);

            DoImport(response, entities, controller);
        }

        public IEnumerable<Entity> LoadEntitiesFromXmlFiles(string folder)
        {
            var lateBoundSerializer = new DataContractSerializer(typeof(Entity));

            var entities = new List<Entity>();
            foreach (var file in Directory.GetFiles(folder, "*.xml"))
            {
                using (var fileStream = new FileStream(file, FileMode.Open))
                {
                    entities.Add((Entity)lateBoundSerializer.ReadObject(fileStream));
                }
            }
            return entities;
        }

        private void DoImport(XrmImporterExporterResponse response, IEnumerable<Entity> entities, LogController controller)
        {
            controller.LogLiteral("Preparing Import");

            var fieldsToRetry = new Dictionary<Entity, List<string>>();
            var typesToImport = entities.Select(e => e.LogicalName).Distinct();

            var allNNRelationships = XrmService.GetAllNnRelationshipEntityNames();
            var associationTypes = typesToImport.Where(allNNRelationships.Contains).ToArray();

            typesToImport = typesToImport.Where(t => !associationTypes.Contains(t)).ToArray();

            var orderedTypes = new List<string>();

            var idSwitches = new Dictionary<string, Dictionary<Guid, Guid>>();
            foreach (var item in typesToImport)
                idSwitches.Add(item, new Dictionary<Guid, Guid>());

            #region tryordertypes

            foreach (var type in typesToImport)
            {
                foreach (var type2 in orderedTypes)
                {
                    var thatType = type2;
                    var thatTypeEntities = entities.Where(e => e.LogicalName == thatType).ToList();
                    var fields = GetFieldsToImport(thatTypeEntities, thatType)
                        .Where(f => XrmService.FieldExists(f, thatType) && XrmService.IsLookup(f, thatType));

                    foreach (var field in fields)
                    {
                        if (thatTypeEntities.Any(e => e.GetLookupType(field) == type))
                        {
                            orderedTypes.Insert(orderedTypes.IndexOf(type2), type);
                            break;
                        }
                    }
                    if (orderedTypes.Contains(type))
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
                    var thisRecordType = recordType;
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

                    var importFieldsForEntity = GetFieldsToImport(thisTypeEntities, recordType).ToArray();
                    var fieldsDontExist = GetFieldsInEntities(thisTypeEntities)
                        .Where(f => !XrmService.FieldExists(f, thisRecordType))
                        .Distinct()
                        .ToArray();
                    foreach (var field in fieldsDontExist)
                    {
                        response.AddResponseItem(
                            new XrmImporterExporterResponseItem(recordType, field, null,
                                string.Format("Field {0} On Entity {1} Doesn't Exist In Target Instance And Will Be Ignored", field, recordType),
                                new NullReferenceException(string.Format("Field {0} On Entity {1} Doesn't Exist In Target Instance And Will Be Ignored", field, recordType))));
                    }

                    var selfReferenceFields = importFieldsForEntity.Where(
                                f =>
                                    XrmService.IsLookup(f, recordType) &&
                                    XrmService.GetLookupTargetEntity(f, recordType) == recordType).ToArray();

                    foreach (var entity in thisTypeEntities)
                    {
                        foreach (var entity2 in orderedEntities)
                        {
                            if (selfReferenceFields.Any(f => entity2.GetLookupGuid(f) == entity.Id || (entity2.GetLookupGuid(f) == Guid.Empty && entity2.GetLookupName(f) == entity.GetStringField(primaryField))))
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
                                idSwitches[recordType].Add(thisEntity.Id, matchRecord.Id);
                                thisEntity.Id = matchRecord.Id;
                                thisEntity.SetField(XrmService.GetPrimaryKeyField(thisEntity.LogicalName), thisEntity.Id);
                            }
                            var isUpdate = existingMatchingIds.Any();
                            foreach (var field in thisEntity.GetFieldsInEntity().ToArray())
                            {
                                if (importFieldsForEntity.Contains(field) &&
                                    XrmService.IsLookup(field, thisEntity.LogicalName) &&
                                    thisEntity.GetField(field) != null)
                                {
                                    var idNullable = thisEntity.GetLookupGuid(field);
                                    if (idNullable.HasValue)
                                    {
                                        var targetTypesToTry = GetTargetTypesToTry(thisEntity, field);
                                        var name = thisEntity.GetLookupName(field);
                                        var fieldResolved = false;
                                        foreach (var lookupEntity in targetTypesToTry)
                                        {
                                            var targetPrimaryKey = Service.GetPrimaryKey(lookupEntity);
                                            var targetPrimaryField = Service.GetPrimaryField(lookupEntity);
                                            var matchRecord = XrmService.GetFirst(lookupEntity, targetPrimaryKey,
                                                idNullable.Value);
                                            if (matchRecord != null)
                                            {
                                                thisEntity.SetLookupField(field, matchRecord);
                                                fieldResolved = true;
                                            }
                                            else
                                            {
                                                var matchRecords = name.IsNullOrWhiteSpace() ?
                                                    new Entity[0] :
                                                    GetMatchingEntities(lookupEntity,
                                                    targetPrimaryField,
                                                    name);
                                                if (matchRecords.Count() == 1)
                                                {
                                                    thisEntity.SetLookupField(field, matchRecords.First());
                                                    fieldResolved = true;
                                                }
                                            }
                                            if (!fieldResolved)
                                            {
                                                if (!fieldsToRetry.ContainsKey(thisEntity))
                                                    fieldsToRetry.Add(thisEntity, new List<string>());
                                                fieldsToRetry[thisEntity].Add(field);
                                            }
                                        }
                                    }
                                }
                            }
                            var fieldsToSet = new List<string>();
                            fieldsToSet.AddRange(thisEntity.GetFieldsInEntity()
                                .Where(importFieldsForEntity.Contains));
                            if (fieldsToRetry.ContainsKey(thisEntity))
                                fieldsToSet.RemoveAll(f => fieldsToRetry[thisEntity].Contains(f));
                            if (isUpdate)
                            {
                                var existingRecord = existingMatchingIds.First();
                                XrmService.Update(thisEntity, fieldsToSet.Where(f => !XrmEntity.FieldsEqual(existingRecord.GetField(f), thisEntity.GetField(f))));
                            }
                            else
                            {
                                PopulateRequiredCreateFields(fieldsToRetry, thisEntity, fieldsToSet);
                                CheckThrowValidForCreate(thisEntity, fieldsToSet);
                                thisEntity.Id = XrmService.Create(thisEntity, fieldsToSet);
                            }
                            if (!isUpdate && thisEntity.GetOptionSetValue("statecode") == XrmPicklists.State.Inactive)
                                XrmService.SetState(thisEntity, thisEntity.GetOptionSetValue("statecode"), thisEntity.GetOptionSetValue("statuscode"));
                            else if (isUpdate && existingMatchingIds.Any())
                            {
                                var matchRecord = existingMatchingIds.First();
                                var thisState = thisEntity.GetOptionSetValue("statecode");
                                var thisStatus = thisEntity.GetOptionSetValue("statuscode");
                                var matchState = matchRecord.GetOptionSetValue("statecode");
                                var matchStatus = matchRecord.GetOptionSetValue("statuscode");
                                if ((thisState != -1 && thisState != matchState)
                                    ||  (thisStatus != -1 && thisState != matchStatus))
                                {
                                    XrmService.SetState(thisEntity, thisEntity.GetOptionSetValue("statecode"), thisEntity.GetOptionSetValue("statuscode"));
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            if (fieldsToRetry.ContainsKey(thisEntity))
                                fieldsToRetry.Remove(thisEntity);
                            response.AddResponseItem(
                                new XrmImporterExporterResponseItem(recordType, null, entity.GetStringField(primaryField),
                                    string.Format("Error Importing Record Id={0}", entity.Id),
                                    ex));
                        }
                    }
                }
                catch (Exception ex)
                {
                    response.AddResponseItem(
                        new XrmImporterExporterResponseItem(recordType, null, null, string.Format("Error Importing Type {0}", recordType), ex));
                }
            }
            controller.TurnOffLevel2();
            countToImport = fieldsToRetry.Count;
            countImported = 0;
            foreach (var item in fieldsToRetry)
            {
                var thisEntity = item.Key;
                controller.UpdateProgress(countImported++, countToImport, string.Format("Retrying Unresolved Fields {0}", thisEntity.LogicalName));
                var thisPrimaryField = XrmService.GetPrimaryNameField(thisEntity.LogicalName);
                try
                {
                    foreach (var field in item.Value)
                    {
                        if (XrmService.IsLookup(field, thisEntity.LogicalName) && thisEntity.GetField(field) != null)
                        {
                            try
                            {
                                var targetTypesToTry = GetTargetTypesToTry(thisEntity, field);
                                var name = thisEntity.GetLookupName(field);
                                var idNullable = thisEntity.GetLookupGuid(field);
                                var fieldResolved = false;
                                foreach (var lookupEntity in targetTypesToTry)
                                {
                                    var targetPrimaryKey = Service.GetPrimaryKey(lookupEntity);
                                    var targetPrimaryField = Service.GetPrimaryField(lookupEntity);
                                    var matchRecord = idNullable.HasValue ? XrmService.GetFirst(lookupEntity, targetPrimaryKey,
                                        idNullable.Value) : null;
                                    if (matchRecord != null)
                                    {
                                        thisEntity.SetLookupField(field, matchRecord);
                                        fieldResolved = true;
                                    }
                                    else
                                    {
                                        var matchRecords = name.IsNullOrWhiteSpace() ?
                                            new Entity[0] :
                                            GetMatchingEntities(lookupEntity,
                                            targetPrimaryField,
                                            name);
                                        if (matchRecords.Count() == 1)
                                        {
                                            thisEntity.SetLookupField(field, matchRecords.First());
                                            fieldResolved = true;
                                        }
                                    }
                                    if (!fieldResolved)
                                    {
                                        throw new Exception(string.Format("Could not find matching record for field {0}.{1} {2}", thisEntity.LogicalName, field, name));
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                if (thisEntity.Contains(field))
                                    thisEntity.Attributes.Remove(field);
                                response.AddResponseItem(
                                    new XrmImporterExporterResponseItem(thisEntity.LogicalName, field, thisEntity.GetStringField(thisPrimaryField),
                                        string.Format("Error Setting Lookup Field Id={0}", thisEntity.Id), ex));
                            }
                        }
                    }
                    XrmService.Update(thisEntity, item.Value);
                }
                catch (Exception ex)
                {
                    response.AddResponseItem(
                        new XrmImporterExporterResponseItem(thisEntity.LogicalName, null, thisEntity.GetStringField(thisPrimaryField),
                            string.Format("Error Importing Record Id={0}", thisEntity.Id),
                            ex));
                }
            }
            countToImport = associationTypes.Count();
            countImported = 0;
            foreach (var relationshipEntityName in associationTypes)
            {
                var thisEntityName = relationshipEntityName;
                controller.UpdateProgress(countImported++, countToImport, string.Format("Associating {0} Records", thisEntityName));
                var thisTypeEntities = entities.Where(e => e.LogicalName == thisEntityName).ToList();
                var countRecordsToImport = thisTypeEntities.Count;
                var countRecordsImported = 0;
                foreach (var thisEntity in thisTypeEntities)
                {
                    try
                    {
                        controller.UpdateLevel2Progress(countRecordsImported++, countRecordsToImport, string.Format("Associating {0} Records", thisEntityName));
                        var relationship = XrmService.GetRelationshipMetadataForEntityName(thisEntityName);
                        var type1 = relationship.Entity1LogicalName;
                        var field1 = relationship.Entity1IntersectAttribute;
                        var type2 = relationship.Entity2LogicalName;
                        var field2 = relationship.Entity2IntersectAttribute;

                        //bit of hack
                        //when importing from csv just set the fields to the string name of the referenced record
                        //so either string when csv or guid when xml import/export
                        var value1 = thisEntity.GetField(relationship.Entity1IntersectAttribute);
                        var id1 = value1 is string
                            ? GetUniqueMatchingEntity(type1, Service.GetPrimaryField(type1), (string)value1).Id
                            : thisEntity.GetGuidField(relationship.Entity1IntersectAttribute);

                        var value2 = thisEntity.GetField(relationship.Entity2IntersectAttribute);
                        var id2 = value2 is string
                            ? GetUniqueMatchingEntity(type2, Service.GetPrimaryField(type2), (string)value2).Id
                            : thisEntity.GetGuidField(relationship.Entity2IntersectAttribute);

                        //add a where field lookup reference then look it up
                        if (idSwitches.ContainsKey(type1) && idSwitches[type1].ContainsKey(id1))
                            id1 = idSwitches[type1][id1];
                        if (idSwitches.ContainsKey(type2) && idSwitches[type2].ContainsKey(id2))
                            id2 = idSwitches[type2][id2];
                        XrmService.AssociateSafe(relationship.SchemaName, type1, field1, id1, type2, field2, new[] { id2 });
                    }
                    catch (Exception ex)
                    {
                        response.AddResponseItem(
                            new XrmImporterExporterResponseItem(
                                string.Format("Error Associating Record Of Type {0} Id {1}", thisEntity.LogicalName,
                                    thisEntity.Id),
                                ex));
                    }
                }
            }
        }

        private void PopulateRequiredCreateFields(Dictionary<Entity, List<string>> fieldsToRetry, Entity thisEntity, List<string> fieldsToSet)
        {
            if (thisEntity.LogicalName == "team"
                && !fieldsToSet.Contains("businessunitid")
                && XrmService.FieldExists("businessunitid", "team"))
            {
                thisEntity.SetLookupField("businessunitid", GetRootBusinessUnitId(), "businessunit");
                fieldsToSet.Add("businessunitid");
                if (fieldsToRetry.ContainsKey(thisEntity)
                    && fieldsToRetry[thisEntity].Contains("businessunitid"))
                    fieldsToRetry[thisEntity].Remove("businessunitid");
            }
            if (thisEntity.LogicalName == Entities.subject
                    && !fieldsToSet.Contains(Fields.subject_.featuremask)
                    && XrmService.FieldExists(Fields.subject_.featuremask, Entities.subject))
            {
                thisEntity.SetField(Fields.subject_.featuremask, 1);
                fieldsToSet.Add(Fields.subject_.featuremask);
                if (fieldsToRetry.ContainsKey(thisEntity)
                    && fieldsToRetry[thisEntity].Contains(Fields.subject_.featuremask))
                    fieldsToRetry[thisEntity].Remove(Fields.subject_.featuremask);
            }
        }

        private Guid GetRootBusinessUnitId()
        {
            return XrmService.GetFirst("businessunit", "parentbusinessunitid", null, new string[0]).Id;
        }

        private List<string> GetTargetTypesToTry(Entity thisEntity, string field)
        {
            var targetTypesToTry = new List<string>();

            if (!string.IsNullOrWhiteSpace(thisEntity.GetLookupType(field)))
            {
                targetTypesToTry.Add(thisEntity.GetLookupType(field));
            }
            else
            {
                switch (Service.GetFieldType(field, thisEntity.LogicalName))
                {
                    case Record.Metadata.RecordFieldType.Customer:
                        targetTypesToTry.Add("account");
                        targetTypesToTry.Add("contact");
                        break;
                    case Record.Metadata.RecordFieldType.Lookup:
                        targetTypesToTry.Add(thisEntity.GetLookupType(field));
                        break;
                    default:
                        throw new NotImplementedException(string.Format("Could not determine target type for field {0}.{1} of type {2}", thisEntity.LogicalName, field, Service.GetFieldType(field, thisEntity.LogicalName)));
                }
            }

            return targetTypesToTry;
        }

        private IEnumerable<Entity> GetMatchForExistingRecord(IEnumerable<Entity> existingEntitiesWithIdMatches, Entity thisEntity)
        {
            //this a bit messy
            var existingMatches = existingEntitiesWithIdMatches.Where(e => e.Id == thisEntity.Id);
            if (!existingMatches.Any())
            {
                var matchByNameEntities = new[] { "businessunit", "team", "pricelevel", "uomschedule", "uom", "entitlementtemplate" };
                if (thisEntity.LogicalName == "businessunit" && thisEntity.GetField("parentbusinessunitid") == null)
                {
                    existingMatches = XrmService.RetrieveAllAndClauses("businessunit",
                        new[] { new ConditionExpression("parentbusinessunitid", ConditionOperator.Null) });
                }
                else if (matchByNameEntities.Contains(thisEntity.LogicalName))
                {
                    var primaryField = XrmService.GetPrimaryNameField(thisEntity.LogicalName);
                    var name = thisEntity.GetStringField(primaryField);
                    if (name.IsNullOrWhiteSpace())
                        throw new NullReferenceException(string.Format("{0} Is Required On The {1}", XrmService.GetFieldLabel(primaryField, thisEntity.LogicalName), XrmService.GetEntityLabel(thisEntity.LogicalName)));
                    existingMatches = GetMatchingEntities(thisEntity.LogicalName, primaryField, name);
                    if (existingMatches.Count() > 1)
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
                    case "annotation":
                        if (!fieldsToSet.Contains("objectid"))
                            throw new NullReferenceException(string.Format("Cannot create {0} {1} as its parent {2} does not exist"
                                , XrmService.GetEntityLabel(thisEntity.LogicalName), thisEntity.GetStringField(XrmService.GetPrimaryNameField(thisEntity.LogicalName))
                                , thisEntity.GetStringField("objecttypecode") != null ? XrmService.GetEntityLabel(thisEntity.GetStringField("objecttypecode")) : "Unknown Type"));
                        break;
                    case "productpricelevel":
                        if (!fieldsToSet.Contains("pricelevelid"))
                            throw new NullReferenceException(string.Format("Cannot create {0} {1} as its parent {2} is empty"
                                , XrmService.GetEntityLabel(thisEntity.LogicalName), thisEntity.GetStringField(XrmService.GetPrimaryNameField(thisEntity.LogicalName))
                                , XrmService.GetEntityLabel("pricelevel")));
                        break;
                }
            }
            return;
        }

        private IEnumerable<string> GetFieldsInEntities(IEnumerable<Entity> thisTypeEntities)
        {
            return thisTypeEntities.SelectMany(e => e.GetFieldsInEntity());
        }

        private IEnumerable<string> GetFieldsToImport(IEnumerable<Entity> thisTypeEntities, string type)
        {
            var fields = GetFieldsInEntities(thisTypeEntities)
                .Where(f => IsIncludeField(f, type))
                .Distinct();
            return fields;
        }

        public bool IsIncludeField(string fieldName, string entityType)
        {
            var hardcodeInvalidFields = new[]
            {
                "administratorid", "owneridtype", "ownerid", "timezoneruleversionnumber", "utcconversiontimezonecode", "organizationid", "owninguser", "owningbusinessunit","owningteam",
                "overriddencreatedon", "statuscode", "statecode", "createdby", "createdon", "modifiedby", "modifiedon", "modifiedon", "jmcg_currentnumberposition", "calendarrules"
            };
            if (hardcodeInvalidFields.Contains(fieldName))
                return false;
            //these are just hack since they are not updateable fields (IsWriteable)
            if (fieldName == "parentbusinessunitid")
                return true;
            if (fieldName == "businessunitid")
                return true;
            if (fieldName == "pricelevelid")
                return true;
            if (fieldName == "salesliteratureid")
                return true;
            return
                Service.FieldExists(fieldName, entityType) && Service.GetFieldMetadata(fieldName, entityType).Writeable;

        }

        public void ExportXml(IEnumerable<ImportExportRecordType> exports, Folder folder, bool includeNotes, bool incoldeNNBetweenEntities, LogController controller)
        {
            if (!Directory.Exists(folder.FolderPath))
                Directory.CreateDirectory(folder.FolderPath);
            if (exports == null || !exports.Any())
                throw new Exception("Error No Record Types To Export");
            var countToExport = exports.Count();
            var countsExported = 0;
            var exported = new Dictionary<string, List<Entity>>();

            //this distinct is a bit inconsistent with actually allowing duplicates
            foreach (var exportType in exports)
            {
                var type = exportType.RecordType == null ? null : exportType.RecordType.Key;
                controller.UpdateProgress(countsExported++, countToExport, string.Format("Exporting {0} Records", type));
                var conditions = new List<ConditionExpression>();
                if (type == "list")
                    conditions.Add(new ConditionExpression("type", ConditionOperator.Equal, XrmPicklists.ListType.Dynamic));
                if (type == "knowledgearticle")
                    conditions.Add(new ConditionExpression("islatestversion", ConditionOperator.Equal, true));
                //doesn't work for too many notes
                //should have option on each or all entities for notes maybe
                IEnumerable<Entity> entities;
                switch (exportType.Type)
                {
                    case ExportType.AllRecords:
                        {
                            entities = XrmService.RetrieveAllAndClauses(type, conditions)
                                .Where(e => !CheckIgnoreForExport(exportType, e))
                                .ToArray();
                            break;
                        }
                    case ExportType.FetchXml:
                        {
                            var queryExpression = XrmService.ConvertFetchToQueryExpression(exportType.FetchXml);
                            queryExpression.ColumnSet = new ColumnSet(true);
                            entities = queryExpression.PageInfo != null && queryExpression.PageInfo.Count > 0
                                ? XrmService.RetrieveFirstX(queryExpression, queryExpression.PageInfo.Count)
                                : XrmService.RetrieveAll(queryExpression);
                            entities = entities
                                .Where(e => !CheckIgnoreForExport(exportType, e))
                                .ToArray();
                            break;
                        }
                    case ExportType.SpecificRecords:
                        {
                            var primaryKey = XrmService.GetPrimaryKeyField(type);
                            var ids = exportType.SpecificRecordsToExport == null
                                ? new string[0]
                                : exportType.SpecificRecordsToExport
                                    .Select(r => r.Record == null ? null : r.Record.Id)
                                    .Where(s => !s.IsNullOrWhiteSpace()).Distinct().ToArray();
                            entities = ids.Any()
                                ? XrmService.RetrieveAllOrClauses(type,
                                    ids.Select(
                                        i => new ConditionExpression(primaryKey, ConditionOperator.Equal, new Guid(i))))
                                : new Entity[0];
                            break;
                        }
                    default:
                        {
                            throw new NotImplementedException(string.Format("No Export Implemented For {0} {1} For {2} Records", typeof(ExportType).Name, exportType.Type, type));
                        }
                }

                var excludeFields = exportType.ExcludeTheseFieldsInExportedRecords == null
                    ? new string[0]
                    : exportType.ExcludeTheseFieldsInExportedRecords.Select(f => f.RecordField == null ? null : f.RecordField.Key).Distinct().ToArray();

                var fieldsAlwaysExclude = new[] { "calendarrules" };
                excludeFields = excludeFields.Union(fieldsAlwaysExclude).ToArray();

                foreach (var entity in entities)
                {
                    entity.RemoveFields(excludeFields);
                    WriteToXml(entity, folder.FolderPath, false);
                }
                if (!exported.ContainsKey(type))
                    exported.Add(type, new List<Entity>());
                exported[type].AddRange(entities);
                if (includeNotes)
                {
                    var notes = XrmService
                        .RetrieveAllOrClauses("annotation",
                            new[] { new ConditionExpression("objecttypecode", ConditionOperator.Equal, type) });
                    foreach (var note in notes)
                    {
                        var objectId = note.GetLookupGuid("objectid");
                        if (objectId.HasValue && entities.Select(e => e.Id).Contains(objectId.Value))
                        {
                            WriteToXml(note, folder.FolderPath, false);
                        }
                    }
                }
            }
            var relationshipsDone = new List<string>();
            if (incoldeNNBetweenEntities)
            {
                foreach (var type in exported.Keys)
                {
                    var nnRelationships = XrmService.GetEntityManyToManyRelationships(type)
                        .Where(
                            r =>
                                exported.Keys.Contains(r.Entity1LogicalName) && exported.Keys.Contains(r.Entity2LogicalName));
                    foreach (var item in nnRelationships)
                    {
                        var type1 = item.Entity1LogicalName;
                        var type2 = item.Entity2LogicalName;
                        if (!relationshipsDone.Contains(item.SchemaName))
                        {
                            var associations = XrmService.RetrieveAllEntityType(item.IntersectEntityName);
                            foreach (var association in associations)
                            {
                                if (exported[type1].Any(e => e.Id == association.GetGuidField(item.Entity1IntersectAttribute))
                                    && exported[type2].Any(e => e.Id == association.GetGuidField(item.Entity2IntersectAttribute)))
                                    WriteToXml(association, folder.FolderPath, true);
                            }
                            relationshipsDone.Add(item.SchemaName);
                        }
                    }
                }
            }
        }

        private void WriteToXml(Entity entity, string folder, bool association)
        {
            var lateBoundSerializer = new DataContractSerializer(typeof(Entity));
            var namesToUse = association ? "association" : entity.GetStringField(XrmService.GetPrimaryNameField(entity.LogicalName).Left(15));
            if (!namesToUse.IsNullOrWhiteSpace())
            {
                var invalidChars = Path.GetInvalidFileNameChars();
                foreach (var character in invalidChars)
                    namesToUse = namesToUse.Replace(character, '_');
            }
            //ensure dont exceed max filename length
            var fileName = string.Format(@"{0}\{1}_{2}_{3}", folder, entity.LogicalName, entity.Id,
                namesToUse).Left(240);
            fileName = fileName + ".xml";
            fileName = fileName.Replace('-', '_');
            using (var fileStream = new FileStream(fileName, FileMode.Create))
            {
                lateBoundSerializer.WriteObject(fileStream, entity);
            }
        }

        private bool CheckIgnoreForExport(ImportExportRecordType exportType, Entity entity)
        {
            if (XrmService.FieldExists("statecode", entity.LogicalName))
            {
                if (!exportType.IncludeInactiveRecords)
                {
                    var activeStates = new List<int>(new []{ XrmPicklists.State.Active });
                    if (entity.LogicalName == "product")
                        activeStates.AddRange(new[] { 2, 3 });//draft and under revision for latest crm releases
                    if (entity.LogicalName == "knowledgearticle")
                    {
                        activeStates.Clear();
                        activeStates.AddRange(new[] { 1, 2, 3 });//draft and under revision for latest crm releases
                    }
                    if (!activeStates.Contains(entity.GetOptionSetValue("statecode")))
                        return true;
                }
            }
            //include 1 = public
            if (entity.LogicalName == "queue" && entity.GetOptionSetValue("queueviewtype") == 1)
                return true;
            //exclude 1 = customer service 2 = holiday schedule
            if (entity.LogicalName == "calendar" && !new[] { 1, 2, }.Contains(entity.GetOptionSetValue("type")))
                return true;
            return false;
        }
    }
}