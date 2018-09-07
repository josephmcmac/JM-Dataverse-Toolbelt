#region

using JosephM.Core.Extentions;
using JosephM.Core.FieldType;
using JosephM.Core.Log;
using JosephM.Core.Service;
using JosephM.Core.Utility;
using JosephM.Record.Extentions;
using JosephM.Record.IService;
using JosephM.Record.Xrm;
using JosephM.Record.Xrm.XrmRecord;
using JosephM.Xrm;
using JosephM.Xrm.Schema;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;

#endregion

namespace JosephM.Deployment.ImportCsvs
{
    public class ImportCsvsService :
        DataImportServiceBase<ImportCsvsRequest, ImportCsvsResponse, ImportCsvsResponseItem>
    {
        public ImportCsvsService(XrmRecordService xrmRecordService)
            : base(xrmRecordService)
        {
        }

        public override void ExecuteExtention(ImportCsvsRequest request, ImportCsvsResponse response,
            LogController controller)
        {
            ImportCsvs(request, controller, response);
        }

        private void ImportCsvs(ImportCsvsRequest request, LogController controller, ImportCsvsResponse response)
        {
            var organisationSettings = new OrganisationSettings(XrmService);
            controller.LogLiteral("Preparing Import");
            var csvFiles = request.FolderOrFiles == ImportCsvsRequest.CsvImportOption.Folder
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

                            //lets set the id if one matches in the target
                            if (keyColumns.Any())
                            {
                                var fieldValues = new Dictionary<string, object>();
                                foreach (var key in keyColumns)
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
                            else
                            {
                                var typeConfig = XrmRecordService.GetTypeConfigs().GetFor(type);
                                var comparisonFields = XrmRecordService.GetTypeConfigs().GetComparisonFieldsFor(type, XrmRecordService);
                                if (typeConfig != null && comparisonFields.Any())
                                {
                                    //if we have a type config then it will find the target during the import
                                    //these are for example price list items which must match on price list, product & unit
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
                            }
                            //lets parse all the fields in to the entity
                            //any lookup fields we will populate the name with Guid.Empty for now
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
                            response.AddResponseItem(new ImportCsvsResponseItem(string.Format("Error Parsing Row {0} To Entity", rowNumber), csvFile, ex));
                        }
                    }
                }
                catch (Exception ex)
                {
                    response.AddResponseItem(new ImportCsvsResponseItem("Not Imported", csvFile, ex));
                }
            }
            foreach(var contact in entities.Where(e => e.LogicalName == Entities.contact))
            {
                if(contact.Contains(Fields.contact_.fullname)
                    && !contact.Contains(Fields.contact_.firstname)
                    && !contact.Contains(Fields.contact_.lastname))
                {
                    //okay for these dudes lets split their name into first and last name somehow
                    var name = contact.GetStringField(Fields.contact_.fullname);
                    if (name != null)
                    {
                        name = name.Trim();
                        var lastSpaceIndex = name.LastIndexOf(" ");
                        if (lastSpaceIndex == -1)
                        {
                            contact.SetField(Fields.contact_.firstname, name);
                        }
                        else
                        {
                            contact.SetField(Fields.contact_.firstname, name.Substring(0,lastSpaceIndex));
                            contact.SetField(Fields.contact_.lastname, name.Substring(lastSpaceIndex + 1));
                        }
                    }
                }
            }
            var imports = DoImport(entities, controller, request.MaskEmails, matchExistingRecords: request.MatchByName);
            foreach (var item in imports)
                response.AddResponseItem(new ImportCsvsResponseItem(item));
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
    }
}