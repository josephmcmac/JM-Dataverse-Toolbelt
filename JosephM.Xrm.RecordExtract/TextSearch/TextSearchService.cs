using System;
using System.Collections.Generic;
using System.Linq;
using JosephM.Core.Constants;
using JosephM.Core.Extentions;
using JosephM.Core.Log;
using JosephM.Core.Service;
using JosephM.Record.Extentions;
using JosephM.Record.IService;
using JosephM.Record.Metadata;
using JosephM.Record.Query;
using JosephM.Xrm.RecordExtract.DocumentWriter;
using JosephM.Xrm.RecordExtract.RecordExtract;

namespace JosephM.Xrm.RecordExtract.TextSearch
{
    public class TextSearchService :
        ServiceBase<TextSearchRequest, TextSearchResponse, TextSearchResponseItem>
    {
        public TextSearchService(IRecordService service, ITextSearchSettings settings,
            DocumentWriter.DocumentWriter documentWriter,
            RecordExtractService recordExtractService)
        {
            Service = service;
            Settings = settings;
            DocumentWriter = documentWriter;
            RecordExtractService = recordExtractService;
        }

        private DocumentWriter.DocumentWriter DocumentWriter { get; set; }

        private ITextSearchSettings Settings { get; set; }

        private RecordExtractService RecordExtractService { get; set; }

        private IRecordService Service { get; set; }

        public override void ExecuteExtention(TextSearchRequest request, TextSearchResponse response,
            LogController controller)
        {
            controller.UpdateProgress(0, 1, "Loading Text Search");
            var document = DocumentWriter.NewDocument();
            var firstSection = document.AddSection();
            var nextSection = document.AddSection();
            var container = new TextSearchContainer(request, response, controller, nextSection);
            ProcessRecordsContainedInName(container);
            ProcessRecordsReferencingTheWord(container);
            ProcessEntireRecordExtracts(container);
            //insert title/summary
            firstSection.AddTitle("Text Search");
            var table = firstSection.Add2ColumnTable();
            table.AddFieldToTable("Execution Time", DateTime.Now.ToString(StringFormats.DateTimeFormat));
            table.AddFieldToTable("Search Text", request.SearchText);
            firstSection.AddTableOfContents(container.Bookmarks);

            //save document
            container.Controller.TurnOffLevel2();
            container.Controller.UpdateProgress(1, 2, "Creating Document");
            var folder = container.Request.SaveToFolder;
            var fileName = string.Format("Record Extract - {0} - {1}", "TextSearch",
                DateTime.Now.ToString("yyyyMMddHHmmss"));
            fileName = document.Save(folder, fileName, container.Request.DocumentFormat);
            container.Response.Folder = container.Request.SaveToFolder.FolderPath;
            container.Response.FileName = fileName;
        }

        private void ProcessEntireRecordExtracts(TextSearchContainer container)
        {
            //for each record with exact match on name do record extract
            //3. foreachrecord with name output them through recordextract
            var done = 0;
            var count = container.NameMatches.Count;
            var typesWithExactNameMatch = container.NameMatches.Select(r => r.Type).Distinct();
            var bookmark = container.AddHeadingWithBookmark("Detail of Records With Name Match");
            foreach (var type in typesWithExactNameMatch.OrderBy(Service.GetDisplayName))
            {
                var thisType = type;
                var thisBookmark = container.Section.AddHeading2WithBookmark(Service.GetCollectionName(thisType));
                bookmark.AddChildBookmark(thisBookmark);
                foreach (var record in container.NameMatches.Where(r => r.Type == thisType))
                {
                    try
                    {
                        container.Controller.UpdateProgress(done++, count,
                            string.Format("Extracting Detail For {0} {1}", Service.GetDisplayName(type),
                                record.GetStringField(Service.GetPrimaryField(type))));
                        var thisResponse =
                            RecordExtractService.ExtractRecordToDocument(container.Controller.GetLevel2Controller(),
                                record.ToLookup(),
                                container.Section, container.Request.DetailOfRecordsRelatedToMatches);
                        container.Response.AddResponseItems(
                            thisResponse.ResponseItems.Select(r => new TextSearchResponseItem(r)));
                        if (!thisResponse.Success)
                            throw thisResponse.Exception;
                        thisBookmark.AddChildBookmarks(thisResponse.Bookmarks);
                    }
                    catch (Exception ex)
                    {
                        container.Response.AddResponseItem(new TextSearchResponseItem("Error Extracting Record",
                            thisType, ex));
                    }
                }
            }
        }

        private void ProcessRecordsReferencingTheWord(TextSearchContainer container)
        {
            //Search For String Field Matches
            //Append The Search For Records Referencing Records Which ZHad Name Matches
            //Then Output The Matching Fields From Those RecordTypes
            var bookmark = container.AddHeadingWithBookmark("Field Matches");
            var recordTypes = GetSearchRecordTypes().ToArray();
            var count = recordTypes.Count();
            var done = 0;
            //load all the activity party references


            foreach (var recordType in recordTypes)
            {
                var recordsToOutput = new Dictionary<string, IRecord>();
                try
                {
                    AppendStringFieldMatches(container, recordType, done, count, recordsToOutput);
                    AppendReferenceMatches(container, done, count, recordType, recordsToOutput);
                    AppendFieldMatchesToDocument(container, recordsToOutput, recordType, bookmark);
                }
                catch (Exception ex)
                {
                    container.Response.AddResponseItem(
                        new TextSearchResponseItem("Error Searching Entity Fields",
                            recordType, ex));
                }
                done++;
            }
        }

        private void AppendFieldMatchesToDocument(TextSearchContainer container,
            Dictionary<string, IRecord> recordsToOutput, string recordType,
            ContentBookmark bookmark)
        {
            if (recordsToOutput.Any())
            {
                var recordOutput = false;
                var todoDone = 0;
                var todoCount = recordsToOutput.Count;
                var primaryField = Service.GetPrimaryField(recordType);

                var fieldsToExclude = new List<string>();
                fieldsToExclude.AddRange(Settings.GetFieldsToExclude());

                foreach (var field in Service.GetFields(recordType))
                {
                    var fieldType = Service.GetFieldType(field, recordType);
                    if (fieldType == RecordFieldType.Uniqueidentifier)
                        fieldsToExclude.Add(field);
                    else if (Service.GetFieldType(field, recordType) == RecordFieldType.String &&
                             Service.GetTextFormat(field, recordType) == TextFormat.PhoneticGuide)
                        fieldsToExclude.Add(field);
                    if (field.EndsWith("_base") && fieldType == RecordFieldType.Money)
                        fieldsToExclude.Add(field);
                }

                //some lookup names don;t get loaded into the record so will load them all now so i don't have to field by field
                recordsToOutput.Values.PopulateEmptyLookups(Service, Settings.GetRecordTypesToExclude());

                var primaryFieldLabel = Service.GetFieldLabel(primaryField, recordType);
                var recordTypeCollectionLabel = Service.GetCollectionName(recordType);
                foreach (var match in recordsToOutput.Values)
                {
                    container.Controller.UpdateLevel2Progress(todoDone++, todoCount,
                        string.Format("Appending {0} To Document", recordTypeCollectionLabel));

                    var fieldsToDisplay = new List<string>();
                    foreach (
                        var field in
                            match.GetFieldsInEntity().Where(f => f != primaryField && !fieldsToExclude.Contains(f)))
                    {
                        var value = Service.GetFieldAsDisplayString(match, field);
                        if (value != null)
                        {
                            var stringValue = value.CheckStripHtml(field);
                            if (IsSearchMatch(stringValue, container))
                                fieldsToDisplay.Add(field);
                        }
                    }

                    if (fieldsToDisplay.Any())
                    {
                        if (!recordOutput)
                        {
                            var thisBookmark =
                                container.Section.AddHeading2WithBookmark(string.Format("{0} ({1})", Service.GetCollectionName(recordType), recordsToOutput.Count()));
                            bookmark.AddChildBookmark(thisBookmark);
                            recordOutput = true;
                        }
                        var table = container.Section.Add2ColumnTable();
                        table.AddFieldToTable(primaryFieldLabel, match.GetStringField(primaryField));
                        foreach (var field in fieldsToDisplay)
                        {
                            var value = Service.GetFieldAsDisplayString(match, field);
                            if (value != null)
                            {
                                var stringValue = value.CheckStripHtml(field);
                                if (IsSearchMatch(stringValue, container))
                                    table.AddFieldToTable(Service.GetFieldLabel(field, recordType),
                                        stringValue);
                            }
                        }
                    }
                }
            }
        }

        private void AppendStringFieldMatches(TextSearchContainer container, string recordType, int done, int count,
            Dictionary<string, IRecord> recordsToOutput)
        {
            var primaryField = Service.GetPrimaryField(recordType);
            var thisRecordType = recordType;
            container.Controller.UpdateProgress(done, count,
                string.Format("Searching String Fields In {0}", Service.GetCollectionName(recordType)));
            try
            {
                var nonPrimaryStringFields = Service.GetStringFields(recordType)
                    .Where(f => f != primaryField)
                    .Where(f => Service.IsString(f, thisRecordType))
                    .ToArray();
                if (nonPrimaryStringFields.Any())
                {
                    var setSearchFields = Settings.GetTextSearchSetFields()
                        .Where(f => f.RecordType.Key == recordType)
                        .Select(f => f.RecordField.Key)
                        .ToArray();
                    var nonSetSearchFields = nonPrimaryStringFields.Where(f => !setSearchFields.Contains(f)).ToArray();
                    var fieldsTodo = nonSetSearchFields.Count();
                    var fieldsDone = 0;
                    foreach (var field in nonSetSearchFields)
                    {
                        container.Controller.UpdateLevel2Progress(fieldsDone++, fieldsTodo, "Searching String Fields");
                        try
                        {
                            var conditions =
                                new[]
                                {
                                    new Condition(field, ConditionType.Like,
                                        string.Format("%{0}%", container.Request.SearchText))
                                };
                            var stringFieldMatches =
                                Service.RetrieveAllOrClauses(recordType, conditions).ToArray();
                            foreach (var stringFieldMatch in stringFieldMatches)
                            {
                                if (!recordsToOutput.ContainsKey(stringFieldMatch.Id))
                                    recordsToOutput.Add(stringFieldMatch.Id, stringFieldMatch);
                            }
                        }
                        catch (Exception ex)
                        {
                            container.Response.AddResponseItem(
                                new TextSearchResponseItem("Error Searching String Fields", recordType, field, ex));
                        }
                    }
                    if (setSearchFields.Any())
                    {
                        //this code written as the crm web service / sql timedout when doing text searches over the entire record table
                        //i thus split all the records into sets defined by a date range and query the text in each set iteratively
                        //this way I limit the volume of text being searched in each crm web service query by a approximate number of records defined in the settings
                        const int initialQuerySetSize = 5000;
                        container.Controller.UpdateLevel2Progress(0, 1, string.Format("Configuring Search Sets"));
                        var startDate = new DateTime(1901, 01, 01);
                        var sortedDatesTemplate = new List<DateTime>();
                        //query the created dates of all records in the table
                        //this does iterative queries sorting by created date
                        //to avoid crm requerying the entire table for each iterative request
                        while (true)
                        {
                            var records =
                                Service.GetFirstX(recordType, initialQuerySetSize, new[] { "createdon" },
                                    new[]
                                            {
                                                new Condition("createdon", ConditionType.GreaterThan, startDate)
                                            },
                                    new[] { new SortExpression("createdon", SortType.Ascending) }).ToArray();

                            if (!records.Any())
                                break;
                            var theseDates =
                                records.Where(r => r.GetDateTime("createdon").HasValue)
                                    .Select(r => r.GetDateTime("createdon"))
                                    .Cast<DateTime>()
                                    .ToList();
                            theseDates.Sort();
                            startDate = theseDates.Last();
                            sortedDatesTemplate.AddRange(theseDates);
                            if (records.Count() < initialQuerySetSize)
                                break;
                        }
                        sortedDatesTemplate.Sort();
                        var fieldSetsTodo = sortedDatesTemplate.Count;
                        foreach (var field in setSearchFields)
                        {
                            try
                            {
                                var thisFieldSortedDates = sortedDatesTemplate.ToList();
                                var label = Service.GetFieldLabel(field, recordType);
                                //now query the text in each date range set
                                while (thisFieldSortedDates.Any())
                                {
                                    var fieldSetsDone = fieldSetsTodo - thisFieldSortedDates.Count;
                                    container.Controller.UpdateLevel2Progress(fieldSetsDone, fieldSetsTodo,
                                        string.Format("Searching {0}", label));
                                    var remaining = thisFieldSortedDates.Count;
                                    var first = thisFieldSortedDates.First();
                                    var i = remaining < Settings.TextSearchSetSize
                                        ? remaining - 1
                                        : Settings.TextSearchSetSize - 1;
                                    var limit = thisFieldSortedDates.ElementAt(i);
                                    if (first.Equals(limit) && thisFieldSortedDates.Any(l => l > first))
                                    {
                                        limit = thisFieldSortedDates.First(l => l > first);
                                    }

                                    var conditions = new[]
                                    {
                                        new Condition("createdon", ConditionType.GreaterEqual, first),
                                        new Condition("createdon", ConditionType.LessEqual, limit),
                                        new Condition(field, ConditionType.Like,
                                            string.Format("%{0}%", container.Request.SearchText))
                                    };
                                    var stringFieldMatches = Service.RetrieveAllAndClauses(recordType, conditions);
                                    foreach (var stringFieldMatch in stringFieldMatches)
                                    {
                                        recordsToOutput.Add(stringFieldMatch.Id, stringFieldMatch);
                                    }
                                    thisFieldSortedDates = thisFieldSortedDates.Where(d => d > limit).ToList();
                                }
                            }
                            catch (Exception ex)
                            {
                                container.Response.AddResponseItem(
                                    new TextSearchResponseItem("Error Searching String Fields", recordType, field, ex));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                container.Response.AddResponseItem(
                    new TextSearchResponseItem("Error Searching String Fields",
                        recordType, ex));
            }
        }

        private void AppendReferenceMatches(TextSearchContainer container, int done, int count, string recordType,
            Dictionary<string, IRecord> recordsToOutput)
        {
            try
            {
                var progressPrefix = string.Format("Searching Reference Fields In {0}",
                    Service.GetCollectionName(recordType));
                container.Controller.UpdateProgress(done, count, progressPrefix);
                var recordTypesWithNameMatch = container.GetRecordTypesWithNameMatch().ToArray();
                var oneToManyRelationships =
                    recordTypesWithNameMatch
                        .SelectMany(r => Service.GetOneToManyRelationships(r))
                        .Where(r => r.ReferencingEntity == recordType)
                        .ToArray();
                var level2Done = 0;
                var level2Count = oneToManyRelationships.Count();

                // get the activity party references
                if (Service.IsActivityType(recordType))
                {
                    var activityPartyReferences = new List<IRecord>();
                    //need to the activities which have an activity party match
                    foreach (var match in container.NameMatches)
                    {
                        if (Service.IsActivityPartyParticipant(match.Type))
                        {
                            var conditions = new[]
                            {
                                new Condition("partyid", ConditionType.Equal, match.Id)
                            };
                            //need conditions where the party is a type match and the activity is this type
                            //simpler just get for all types inititally
                            var activityParties = Service.RetrieveAllAndClauses(
                                "activityparty",
                                conditions
                                );
                            activityPartyReferences.AddRange(
                                activityParties.Where(ap => ap.GetLookupType("partyid") == match.Type));
                        }
                    }
                    if (activityPartyReferences.Any())
                    {
                        var conditions = activityPartyReferences
                            .Select(
                                ap =>
                                    new Condition(Service.GetPrimaryKey(recordType), ConditionType.Equal,
                                        ap.GetLookupId("activityid")));
                        var activities = Service.RetrieveAllOrClauses(recordType, conditions);
                        foreach (var activity in activities)
                        {
                            if (!recordsToOutput.ContainsKey(activity.Id))
                                recordsToOutput.Add(activity.Id, activity);
                        }
                    }
                }

                foreach (var recordTypeWithNameMatch in recordTypesWithNameMatch)
                {
                    var thisRecordTypeNameMatch = recordTypeWithNameMatch;

                    foreach (
                        var one2ManyRelationshipMetadata in
                            oneToManyRelationships.Where(r => r.ReferencedEntity == thisRecordTypeNameMatch))
                    {
                        var thisMetadata = one2ManyRelationshipMetadata;
                        try
                        {
                            container.Controller.UpdateLevel2Progress(level2Done++, level2Count,
                                string.Format("Searching {0} {1}",
                                    Service.GetFieldLabel(one2ManyRelationshipMetadata.ReferencingAttribute,
                                        one2ManyRelationshipMetadata.ReferencingEntity),
                                    Service.GetDisplayName(one2ManyRelationshipMetadata.ReferencedEntity)));
                            var conditions = container.NameMatches
                                .Where(r => r.Type == thisRecordTypeNameMatch)
                                .Select(
                                    m =>
                                        new Condition(thisMetadata.ReferencingAttribute,
                                            ConditionType.Equal,
                                            m.Id));
                            var relatedEntities = Service.RetrieveAllOrClauses(recordType, conditions);
                            foreach (var relatedEntity in relatedEntities)
                            {
                                if (!recordsToOutput.ContainsKey(relatedEntity.Id))
                                    recordsToOutput.Add(relatedEntity.Id, relatedEntity);
                            }
                        }
                        catch (Exception ex)
                        {
                            container.Response.AddResponseItem(
                                new TextSearchResponseItem("Error Searching Reference Fields", recordType,
                                    one2ManyRelationshipMetadata.ReferencingAttribute, ex));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                container.Response.AddResponseItem(
                    new TextSearchResponseItem("Error Searching Reference Fields",
                        recordType, ex));
            }
        }

        private
            void ProcessRecordsContainedInName(TextSearchContainer container)
        {
            var bookmark = container.AddHeadingWithBookmark("Records With Matching Name");
            var recordTypes = GetSearchRecordTypes().ToArray();
            var count = recordTypes.Count();
            var done = 0;
            foreach (var recordType in recordTypes)
            {
                try
                {
                    var progressTextPrefix = string.Format("Searching Record Names In {0}",
                        Service.GetCollectionName(recordType));
                    container.Controller.UpdateProgress(done++, count, progressTextPrefix);
                    var primaryField = Service.GetPrimaryField(recordType);
                    if (!primaryField.IsNullOrWhiteSpace())
                    {
                        var conditions = new[]
                        {
                            new Condition(primaryField, ConditionType.Like,
                                string.Format("%{0}%", container.Request.SearchText))
                        };
                        var matches =
                            Service.RetrieveAllAndClauses(recordType, conditions, new[] {primaryField}).ToArray();
                        if (matches.Any())
                        {
                            try
                            {
                                var thisBookmark =
                                    container.Section.AddHeading2WithBookmark(string.Format("{0} ({1})", Service.GetCollectionName(recordType), matches.Count()));
                                bookmark.AddChildBookmark(thisBookmark);
                                var table = container.Section.Add1ColumnTable();
                                var matchCount = matches.Count();
                                var matchCountDone = 0;
                                foreach (var match in matches)
                                {
                                    container.Controller.UpdateProgress(done, count,
                                        string.Format("{0} (Adding {1} Of {2})", progressTextPrefix, ++matchCountDone,
                                            matchCount));
                                    container.AddNameMatch(match);
                                    var outputText = match.GetStringField(primaryField);
                                    outputText = ExtractUtility.CheckStripFormatting(outputText, recordType,
                                        primaryField);
                                    table.AddRow(outputText);
                                }
                            }
                            catch (Exception ex)
                            {
                                container.Response.AddResponseItem(
                                    new TextSearchResponseItem("Error Adding Matched Record",
                                        recordType, ex));
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    container.Response.AddResponseItem(new TextSearchResponseItem("Error Adding Match Records",
                        recordType, ex));
                }
            }
        }

        private IEnumerable<string> GetSearchRecordTypes()
        {
            var recordTypes = Service.GetAllRecordTypesForSearch().OrderBy(n => Service.GetDisplayName(n));
            return Settings.GetRecordTypesToExclude() != null
                ? recordTypes.Except(Settings.GetRecordTypesToExclude())
                : recordTypes;
        }

        internal bool IsSearchMatch(string stringValue, TextSearchContainer container)
        {
            return stringValue != null && stringValue.ToLower().Contains(container.Request.SearchText.ToLower());
        }
    }
}