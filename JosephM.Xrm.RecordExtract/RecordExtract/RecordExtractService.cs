using System;
using System.Collections.Generic;
using System.Linq;
using JosephM.Core.Constants;
using JosephM.Core.Extentions;
using JosephM.Core.FieldType;
using JosephM.Core.Log;
using JosephM.Core.Service;
using JosephM.Record.Extentions;
using JosephM.Record.IService;
using JosephM.Record.Metadata;
using JosephM.Record.Query;
using JosephM.Xrm.RecordExtract.DocumentWriter;

namespace JosephM.Xrm.RecordExtract.RecordExtract
{
    public class RecordExtractService :
        ServiceBase<RecordExtractRequest, RecordExtractResponse, RecordExtractResponseItem>
    {
        public RecordExtractService(IRecordService service, DocumentWriter.DocumentWriter documentWriter)
        {
            Service = service;
            DocumentWriter = documentWriter;
        }

        private DocumentWriter.DocumentWriter DocumentWriter { get; set; }

        private IRecordService Service { get; set; }

        public override void ExecuteExtention(RecordExtractRequest request, RecordExtractResponse response,
            LogController controller)
        {
            var document = DocumentWriter.NewDocument();
            var firstSection = document.AddSection();
            var nextSection = document.AddSection();
            var extractToDocumentRequest = new RecordExtractToDocumentRequest(request.RecordLookup, nextSection,
                controller, request.DetailOfRelatedRecords
                , request.RecordTypesOnlyDisplayName.Where(r => r.RecordType != null).Select(r => r.RecordType.Key).ToArray()
                 , request.FieldsToExclude, request.RecordTypesToExclude.Where(r => r.RecordType != null).Select(r => r.RecordType.Key).ToArray()
                 , request.IncludeCreatedByAndOn, request.IncludeModifiedByAndOn, request.IncludeCrmOwner, request.IncludeState, request.IncludeStatus);
            var extractResponse = ExtractRecordToDocument(extractToDocumentRequest);
            response.AddResponseItems(extractResponse.ResponseItems);
            //insert title/summary
            firstSection.AddTitle("Record Extract");
            var table = firstSection.Add2ColumnTable();
            table.AddFieldToTable("Execution Time", DateTime.Now.ToString(StringFormats.DateTimeFormat));
            table.AddFieldToTable("Record Type", Service.GetDisplayName(request.RecordLookup.RecordType));
            table.AddFieldToTable("Record Name",
                extractResponse.Record.GetStringField(Service.GetPrimaryField(request.RecordLookup.RecordType)));
            firstSection.AddTableOfContents(extractResponse.Bookmarks);
            //save document
            controller.UpdateProgress(1, 2, "Creating Document");
            var folder = request.SaveToFolder;
            var recordToExtractname =
                extractResponse.Record.GetStringField(Service.GetPrimaryField(request.RecordLookup.RecordType));
            var fileName = string.Format("Record Extract - {0} - {1}", recordToExtractname,
                DateTime.Now.ToString("yyyyMMddHHmmss"));
            fileName = document.Save(folder, fileName, request.DocumentFormat);

            response.Folder = request.SaveToFolder.FolderPath;
            response.FileName = fileName;
        }

        public RecordExtractToDocumentResponse ExtractRecordToDocument(LogController controller, Lookup lookup,
            Section section, DetailLevel relatedDetail)
        {
            var request = new RecordExtractToDocumentRequest(lookup, section, controller, relatedDetail);
            var response = ExtractRecordToDocument(request);
            return response;
        }

        private RecordExtractToDocumentResponse ExtractRecordToDocument(RecordExtractToDocumentRequest request)
        {
            var container = new RecordExtractContainer(request);
            var recordToExtract = ExtractMainRecord(container);
            var hasRelatedRecordOutput = false;
            ContentBookmark bookmark = null;
            try
            {
                var relatedTypes = GetRelatedTypes(container);

                foreach (var relatedType in relatedTypes.OrderBy(r => Service.GetDisplayName(r)))
                {
                    var recordType = relatedType;
                    try
                    {
                        var recordsToOutput = new Dictionary<string, IRecord>();
                        GetOneToManyRelated(container, relatedType, recordsToOutput);
                        GetManyToManyRelated(container, relatedType, recordsToOutput);
                        GetActivityPartyRelated(container, relatedType, recordsToOutput);
                        hasRelatedRecordOutput = AppendRelatedToDocument(recordsToOutput, hasRelatedRecordOutput,
                            container, recordType, ref bookmark, request);
                    }
                    catch (Exception ex)
                    {
                        container.Response.AddResponseItem(new RecordExtractResponseItem(
                            "Error Loading Related Records",
                            recordType, ex));
                    }
                }
            }
            catch (Exception ex)
            {
                container.Response.AddResponseItem(new RecordExtractResponseItem("Error Loading Relationships",
                    recordToExtract.Type, ex));
            }
            return container.Response;
        }

        private IEnumerable<string> GetRelatedTypes(RecordExtractContainer container)
        {
            var relatedTypes = new List<string>();
            var oneToManyRelationships = GetOneToManyRelationships(container);
            var manyToManyRelationships = GetManyToManyRelationships(container);
            var extractRecordType = container.RecordToExtractType;

            relatedTypes.AddRange(oneToManyRelationships.Select(r => r.ReferencingEntity));
            relatedTypes.AddRange(
                manyToManyRelationships.Where(r => r.RecordType1 == extractRecordType)
                    .Select(r => r.RecordType2));
            relatedTypes.AddRange(
                manyToManyRelationships.Where(r => r.RecordType2 == extractRecordType)
                    .Select(r => r.RecordType1));
            if (Service.GetRecordTypeMetadata(container.RecordToExtractType).IsActivityParticipant)
                relatedTypes.AddRange(Service.GetAllRecordTypes().Where(r => Service.GetRecordTypeMetadata(r).IsActivityType));
            relatedTypes = relatedTypes.Distinct()
                .Where(r => !GetRecordTypesToExclude().Contains(r))
                //we exclude activity parties from this as we get the links when processing activity types
                .Where(r => r != "activityparty")
                .ToList();

            return relatedTypes;
        }

        private IEnumerable<string> GetRecordTypesToExclude()
        {
            return ExtractUtility.GetSystemRecordTypesToExclude();
        }

        private void GetActivityPartyRelated(RecordExtractContainer container, string relatedType,
            Dictionary<string, IRecord> recordsToOutput)
        {
            //if the extracted record could be an activity participant for this type
            //then get and append them
            if (Service.GetRecordTypeMetadata(container.RecordToExtractType).IsActivityParticipant && Service.GetRecordTypeMetadata(relatedType).IsActivityType)
            {
                var activities = Service.GetLinkedRecordsThroughBridge(relatedType, "activityparty",
                    container.RecordToExtractType, "partyid", "activityid", container.RecordToExtractId);
                
                foreach (var activity in activities)
                {
                    if (!recordsToOutput.ContainsKey(activity.Id))
                        recordsToOutput.Add(activity.Id, activity);
                }
            }
        }

        private bool AppendRelatedToDocument(Dictionary<string, IRecord> recordsToOutput, bool hasRelatedRecordOutput,
            RecordExtractContainer container, string recordType, ref ContentBookmark bookmark, RecordExtractToDocumentRequest request)
        {
            if (recordsToOutput.Any())
            {
                var outputNumbers = recordsToOutput.Count > 1 && container.Request.RelatedDetail == DetailLevel.AllFields;
                recordsToOutput.Values.PopulateEmptyLookups(Service, GetRecordTypesToExclude());
                var recordTypeCollectionLabel = Service.GetCollectionName(recordType);
                var recordTypeLabel = Service.GetDisplayName(recordType);
                if (!hasRelatedRecordOutput)
                {
                    hasRelatedRecordOutput = true;
                    bookmark = container.Section.AddHeading2WithBookmark("Related Records");
                    container.AddBookmark(bookmark);
                }
                var todoDone = 0;
                var todoCount = recordsToOutput.Count;

                var thisBookmark =
                    container.Section.AddHeading3WithBookmark(string.Format("{0} ({1})", recordTypeCollectionLabel, recordsToOutput.Count));
                bookmark.AddChildBookmark(thisBookmark);

                var i = 1;
                foreach (var match in recordsToOutput.Values)
                {
                    if (outputNumbers)
                        container.Section.AddParagraph(string.Format("{0} {1}", recordTypeLabel, i++), true);
                    container.Controller.UpdateProgress(todoDone++, todoCount,
                        string.Format("Appending Related {0} To Document", recordTypeCollectionLabel));
                    WriteRecordToSection(match, container.Section, container.Request.RelatedDetail, request);
                }
            }
            return hasRelatedRecordOutput;
        }

        private void GetManyToManyRelated(RecordExtractContainer container, string relatedType,
            Dictionary<string, IRecord> recordsToOutput)
        {
            var recordTypeCollectionLabel = Service.GetCollectionName(relatedType);
            var relatedTypeManyToManyRelationships = GetManyToManyRelationshipsForType(container, relatedType);
            var todoDone = 0;
            var todoCount = relatedTypeManyToManyRelationships.Count();
            foreach (var manyToMany in relatedTypeManyToManyRelationships)
            {
                container.Controller.UpdateProgress(todoDone++, todoCount,
                    string.Format("Searching Many To Many Relationships To {0}", recordTypeCollectionLabel));
                try
                {
                    if (manyToMany.RecordType1 == relatedType)
                    {
                        var theseRelatedEntities = Service.GetRelatedRecords(container.RecordToExtract, manyToMany,
                            false);
                        foreach (var relatedEntity in theseRelatedEntities)
                        {
                            if (!recordsToOutput.ContainsKey(relatedEntity.Id))
                                recordsToOutput.Add(relatedEntity.Id, relatedEntity);
                        }
                    }
                    if (manyToMany.RecordType2 == relatedType)
                    {
                        var theseRelatedEntities = Service.GetRelatedRecords(container.RecordToExtract, manyToMany,
                            true);
                        foreach (var relatedEntity in theseRelatedEntities)
                        {
                            if (!recordsToOutput.ContainsKey(relatedEntity.Id))
                                recordsToOutput.Add(relatedEntity.Id, relatedEntity);
                        }
                    }
                }
                catch
                    (Exception ex)
                {
                    container.Response.AddResponseItem(
                        new RecordExtractResponseItem("Error Loading Many To Many Records",
                            manyToMany.SchemaName, ex));
                }
            }
        }

        private void GetOneToManyRelated(RecordExtractContainer container, string relatedType,
            IDictionary<string, IRecord> recordsToOutput)
        {
            var recordTypeCollectionLabel = Service.GetCollectionName(relatedType);
            var relatedTypeOneToManyRelationships = GetOneToManyRelationshipsForType(container, relatedType);
            var todoDone = 0;
            var todoCount = relatedTypeOneToManyRelationships.Count();
            foreach (var oneToMany in relatedTypeOneToManyRelationships)
            {
                container.Controller.UpdateProgress(todoDone++, todoCount,
                    string.Format("Searching One To Many Relationships To {0}", recordTypeCollectionLabel));
                try
                {
                    var theseRelatedEntities = Service.RetrieveAllAndClauses(oneToMany.ReferencingEntity,
                        new[]
                        {
                            new Condition(oneToMany.ReferencingAttribute, ConditionType.Equal,
                                container.RecordToExtract.Id)
                        }, null);
                    foreach (var relatedEntity in theseRelatedEntities)
                    {
                        if (!recordsToOutput.ContainsKey(relatedEntity.Id))
                            recordsToOutput.Add(relatedEntity.Id, relatedEntity);
                    }
                }
                catch (Exception ex)
                {
                    container.Response.AddResponseItem(
                        new RecordExtractResponseItem("Error Loading One To Many Records",
                            oneToMany.SchemaName, ex));
                }
            }
        }

        internal IEnumerable<IMany2ManyRelationshipMetadata> GetManyToManyRelationshipsForType(
            RecordExtractContainer container, string recordType)
        {
            return
                GetManyToManyRelationships(container)
                    .Where(r => r.RecordType1 == recordType || r.RecordType2 == recordType);
        }

        private IEnumerable<IOne2ManyRelationshipMetadata> GetOneToManyRelationshipsForType(
            RecordExtractContainer container, string recordType)
        {
            return GetOneToManyRelationships(container).Where(r => r.ReferencingEntity == recordType);
        }

        private IEnumerable<IMany2ManyRelationshipMetadata> GetManyToManyRelationships(RecordExtractContainer container)
        {
            return
                Service.GetManyToManyRelationships(container.RecordToExtractType)
                    .Where(r => !GetRelationshipsToExclude().Contains(r.SchemaName));
        }

        private IEnumerable<IOne2ManyRelationshipMetadata> GetOneToManyRelationships(RecordExtractContainer container)
        {
            return Service.GetOneToManyRelationships(container.RecordToExtractType)
                .Where(r => !GetRelationshipsToExclude().Contains(r.SchemaName));
        }

        private IEnumerable<string> GetRelationshipsToExclude()
        {
            return ExtractUtility.GetSystemRelationshipsToExclude();
        }

        private IRecord ExtractMainRecord(RecordExtractContainer container)
        {
            container.Controller.UpdateProgress(1, 3, "Loading Record");
            var recordToExtract = Service.Get(container.Request.RecordLookup.RecordType,
                container.Request.RecordLookup.Id);
            container.RecordToExtract = recordToExtract;
            var primaryField = Service.GetPrimaryField(recordToExtract.Type);
            var recordName = recordToExtract.GetStringField(primaryField);
            container.AddBookmark(ExtractUtility.CheckStripFormatting(recordName, container.RecordToExtractType,
                primaryField));
            container.Response.Record = recordToExtract;
            container.Controller.UpdateProgress(2, 3, "Loading Record");
            WriteRecordToSection(recordToExtract, container.Section, DetailLevel.AllFields, container.Request);
            return recordToExtract;
        }

        private void WriteRecordToSection(IRecord record, Section section, DetailLevel detailLevel, RecordExtractToDocumentRequest request)
        {
            if (detailLevel == DetailLevel.CountsOnly)
                return;

            var primaryField = Service.GetPrimaryField(record.Type);
            var recordName = Service.GetFieldAsDisplayString(record, primaryField);
            var table = section.Add2ColumnTable();
            table.AddFieldToTable(Service.GetFieldLabel(primaryField, record.Type),
                ExtractUtility.CheckStripFormatting(recordName, record.Type, primaryField));

            if (detailLevel == DetailLevel.Names || request.OnlyDisplayName(recordName))
                return;

            var fields = record.GetFieldsInEntity();
            if (fields.Any())
            {
                var primaryKey = Service.GetPrimaryKey(record.Type);
                var actualFieldsToExclude = new List<string> {primaryField, primaryKey};

                actualFieldsToExclude.AddRange(request.GetAllFieldsToExclude(record.Type));

                foreach (var field in fields)
                {
                    var fieldType = Service.GetFieldType(field, record.Type);
                    if (fieldType == RecordFieldType.Uniqueidentifier)
                        actualFieldsToExclude.Add(field);
                    else if (Service.GetFieldType(field, record.Type) == RecordFieldType.String &&
                             new[] {TextFormat.PhoneticGuide, TextFormat.VersionNumber}.Contains(
                                 Service.GetFieldMetadata(field, record.Type).TextFormat))
                        actualFieldsToExclude.Add(field);
                    if (field.EndsWith("_base") && fields.Contains(field.Left(field.Length - 5)))
                        actualFieldsToExclude.Add(field);
                }

                var orderedFieldsToDisplay = fields
                    .Where(f => !actualFieldsToExclude.Contains(f))
                    .OrderBy(f => Service.GetFieldLabel(f, record.Type));


                foreach (var field in orderedFieldsToDisplay)
                {
                    var label = Service.GetFieldLabel(field, record.Type);
                    var display = Service.GetFieldAsDisplayString(record, field);
                    if (!label.IsNullOrWhiteSpace() && !display.IsNullOrWhiteSpace() &&
                        !GetStringValuesToExclude().Contains(display))
                        table.AddFieldToTable(label, display);
                }
            }
        }

        public IEnumerable<string> GetStringValuesToExclude()
        {
            return ExtractUtility.GetStringValuesToExclude();
        }
    }
}