using JosephM.Application.ViewModel.SettingTypes;
using JosephM.Core.Extentions;
using JosephM.Core.Service;
using JosephM.Record.Extentions;
using JosephM.Record.IService;
using JosephM.Record.Metadata;
using JosephM.Record.Query;
using System;
using System.Collections.Generic;
using System.Linq;

namespace JosephM.Xrm.TextSearch
{
    public class TextSearchService :
        ServiceBase<TextSearchRequest, TextSearchResponse, TextSearchResponseItem>
    {
        public TextSearchService(IRecordService service)
        {
            Service = service;
        }

        private string[] ignoreErrorMessages = new[]
            {
                "FeatureFCBNotEnabled",
                "'RetrieveMultiple' method does not support entities of type",
                "https://graph.microsoft.com is not allowed",
                "Unauthorized to get shared workspace connection string",
                "FCB.RealTimeCollaboration is not enabled",
                "We can’t reach the service",
                "Retrieving Service Endpoint for Activity Analysis Service or Relationship Intelligence Service from config entity is not allowed"
            };

        private IRecordService Service { get; set; }

        public override void ExecuteExtention(TextSearchRequest request, TextSearchResponse response,
            ServiceRequestController controller)
        {
            controller.UpdateProgress(0, 1, "Loading Text Search");

            var container = new TextSearchContainer(request, response, controller.Controller);

            ProcessRecordsReferencingTheWord(container);

            //save document
            container.Controller.TurnOffLevel2();
            container.Response.GenerateSummaryItems(Service);
            if (!container.Response.Summary.Any())
                container.Response.Message = "No Matches Were Found For The Search Criteria";
        }

        private void ProcessRecordsReferencingTheWord(TextSearchContainer container)
        {
            //Search For String Field Matches
            //Append The Search For Records Referencing Records Which ZHad Name Matches
            //Then Output The Matching Fields From Those RecordTypes
            var recordTypes = GetSearchRecordTypes(container).ToArray();
            var count = recordTypes.Count();
            var done = 0;
            //load all the activity party references

            if(container.Request.SearchAllTypes)
            {
                container.Controller.LogLiteral("Loading metadata - this may take a while");
                Service.LoadFieldsForAllEntities(container.Controller);
            }

            foreach (var recordType in recordTypes)
            {
                var recordsToOutput = new Dictionary<string, IRecord>();
                try
                {
                    AppendStringFieldMatches(container, recordType, done, count, recordsToOutput);
                }
                catch (Exception ex)
                {
                    if (ex.Message == null || !ignoreErrorMessages.Any(iem => ex.Message.Contains(iem)))
                    {
                        container.Response.AddResponseItem(
                        new TextSearchResponseItem("Error searching entity fields",
                            recordType, ex));
                    }
                }
                done++;
            }
        }

        private bool IsHtmlField(string recordType, string fieldName, TextSearchContainer container)
        {
            var fields = ExtractUtility.GetSystemHtmlFields().Union(container.Request.CustomHtmlFields ?? new RecordFieldSetting[0]);
            return fields.Any(f => f.RecordType.Key == recordType && f.RecordField.Key == fieldName);
        }

        private List<string> GetFieldsToExlcude(TextSearchContainer container, string recordType)
        {
            var fieldsToExclude = new List<string>();
            fieldsToExclude.AddRange(ExtractUtility.GetSystemFieldsToExclude());
            if (container.Request.FieldExclusions != null)
            {
                fieldsToExclude.AddRange(container.Request.FieldExclusions.Where(fe => fe.RecordType.Key == recordType).Select(fe => fe.RecordField.Key));
            }

            foreach (var field in Service.GetFields(recordType))
            {
                var fieldType = Service.GetFieldType(field, recordType);
                if (fieldType == RecordFieldType.Uniqueidentifier)
                    fieldsToExclude.Add(field);
                else if (Service.GetFieldType(field, recordType) == RecordFieldType.String &&
                         Service.GetFieldMetadata(field, recordType).TextFormat == TextFormat.PhoneticGuide)
                    fieldsToExclude.Add(field);
                if (field.EndsWith("_base") && fieldType == RecordFieldType.Money)
                    fieldsToExclude.Add(field);
            }

            return fieldsToExclude;
        }

        private void AppendStringFieldMatches(TextSearchContainer container, string recordType, int done, int count,
            Dictionary<string, IRecord> recordsToOutput)
        {
            var thisRecordType = recordType;
            container.Controller.UpdateProgress(done, count,
                string.Format("Searching String Fields In {0}", Service.GetCollectionName(recordType)));
            try
            {
                var fieldsToExclude = GetFieldsToExlcude(container, recordType);

                var stringFields = Service.GetFields(recordType)
                    .Where(f => Service.IsString(f, recordType))
                    .Where(f => Service.IsString(f, thisRecordType))
                    .Except(fieldsToExclude)
                    .ToArray();

                if (stringFields.Any())
                {
                    var htmlSearchFields = container.Request.StripHtmlTagsPriorToSearch
                        ? stringFields
                        .Where(s => IsHtmlField(recordType, s, container)).ToArray()
                        : new string[0];
                    var setSearchFields = stringFields
                        .Intersect(ExtractUtility.GetSystemTextSearchSetFields()
                        .Where(f => f.RecordType.Key == recordType)
                        .Select(f => f.RecordField.Key)
                        .Except(htmlSearchFields))
                        .ToArray();
                    var nonSetSearchFields = stringFields
                        .Except(setSearchFields)
                        .Except(htmlSearchFields)
                        .ToArray();



                    if (htmlSearchFields.Any())
                    {
                        //this code written as the crm web service / sql timedout when doing text searches over the entire record table
                        //i thus split all the records into sets defined by a date range and query the text in each set iteratively
                        //this way I limit the volume of text being searched in each crm web service query by a approximate number of records defined in the settings
                        int totalCount = 0;
                        var sortedDatesTemplate = GetDateRangesForSetSearches(recordType, container, out totalCount);
                        var totalDone = 0;
                        foreach (var field in htmlSearchFields)
                        {
                            try
                            {
                                var thisFieldSortedDates = sortedDatesTemplate.ToList();

                                var label = Service.GetFieldLabel(field, recordType);

                                //now query the text in each date range set
                                while (thisFieldSortedDates.Any())
                                {
                                    var first = thisFieldSortedDates.First();
                                    var limit = thisFieldSortedDates.Count > 1 ? thisFieldSortedDates[1] : (DateTime?)null;
                                    if (first.Equals(limit) && thisFieldSortedDates.Any(l => l > first))
                                    {
                                        limit = thisFieldSortedDates.First(l => l > first);
                                    }

                                    var query = new QueryDefinition(recordType);
                                    query.RootFilter = new Filter();
                                    query.RootFilter.ConditionOperator = container.Request.Operator == TextSearchRequest.SearchTermOperator.And ? FilterOperator.And : FilterOperator.Or;
                                    query.RootFilter.SubFilters = container.Request.SearchTerms.Select(s =>
                                    {
                                        var searchTermFilter = new Filter();
                                        searchTermFilter.Conditions = new List<Condition>();
                                        searchTermFilter.Conditions.Add(new Condition("createdon", ConditionType.GreaterEqual, first));
                                        if(limit.HasValue)
                                            searchTermFilter.Conditions.Add(new Condition("createdon", ConditionType.LessThan, limit.Value));
                                        return searchTermFilter;
                                    }).ToList();


                                    var allOfType = Service.RetreiveAll(query);
                                    foreach (var item in allOfType)
                                    {
                                        container.Controller.UpdateLevel2Progress(totalDone++, totalCount, string.Format("Searching Html {0}", label));
                                        var fieldValue = item.GetStringField(field);
                                        if(fieldValue != null)
                                        {
                                            var stripHtml = fieldValue.StripHtml();
                                            if (IsSearchMatch(stripHtml, container))
                                            {
                                                recordsToOutput.Add(item.Id, item);
                                                container.AddMatchedRecord(field, item);
                                            }
                                        }
                                    }
                                    thisFieldSortedDates.RemoveAt(0);
                                }
                            }
                            catch (Exception ex)
                            {
                                if (ex.Message == null || !ignoreErrorMessages.Any(iem => ex.Message.Contains(iem)))
                                {
                                    container.Response.AddResponseItem(
                                    new TextSearchResponseItem("Error Searching Html Fields", recordType, field, ex));
                                }
                            }
                        }
                    }

                    var fieldsTodo = nonSetSearchFields.Count();
                    var fieldsDone = 0;
                    foreach (var field in nonSetSearchFields.OrderBy(f => Service.GetFieldLabel(f, recordType)))
                    {
                        container.Controller.UpdateLevel2Progress(fieldsDone++, fieldsTodo, $"Searching {Service.GetFieldLabel(field, recordType)}");
                        try
                        {
                            var conditions =
                                container.Request.SearchTerms.Select(s => 
                                 new Condition(field, ConditionType.Like, string.Format("%{0}%", s.Text)))
                                 .ToArray();
                            var stringFieldMatches = (container.Request.Operator == TextSearchRequest.SearchTermOperator.And
                                ? Service.RetrieveAllAndClauses(recordType, conditions, null)
                                : Service.RetrieveAllOrClauses(recordType, conditions, null))
                                .ToArray();
                            foreach (var stringFieldMatch in stringFieldMatches)
                            {
                                container.AddMatchedRecord(field, stringFieldMatch);
                                if (!recordsToOutput.ContainsKey(stringFieldMatch.Id))
                                    recordsToOutput.Add(stringFieldMatch.Id, stringFieldMatch);
                            }
                        }
                        catch (Exception ex)
                        {
                            if (ex.Message == null || !ignoreErrorMessages.Any(iem => ex.Message.Contains(iem)))
                            {
                                container.Response.AddResponseItem(
                                new TextSearchResponseItem("Error Searching String Fields", recordType, field, ex));
                            }
                        }
                    }
                    if (setSearchFields.Any())
                    {
                        //this code written as the crm web service / sql timedout when doing text searches over the entire record table
                        //i thus split all the records into sets defined by a date range and query the text in each set iteratively
                        //this way I limit the volume of text being searched in each crm web service query by a approximate number of records defined in the settings
                        container.Controller.UpdateLevel2Progress(0, 1, string.Format("Configuring Search Sets"));
                        var fieldSetsTodo = 0;
                        var sortedDatesTemplate = GetDateRangesForSetSearches(recordType, container, out fieldSetsTodo);
                        foreach (var field in setSearchFields)
                        {
                            try
                            {
                                var thisFieldSortedDates = sortedDatesTemplate.ToList();
                                var fieldSetsDone = 0;
                                var label = Service.GetFieldLabel(field, recordType);
                                //now query the text in each date range set
                                while (thisFieldSortedDates.Any())
                                {
                                    container.Controller.UpdateLevel2Progress(fieldSetsDone + ExtractUtility.TextSearchSetSize, fieldSetsTodo,
                                        string.Format("Searching {0}", label));
                                    var first = thisFieldSortedDates.First();
                                    var limit = thisFieldSortedDates.Count > 1 ? thisFieldSortedDates[1] : (DateTime?)null;
                                    var query = new QueryDefinition(recordType);
                                    query.RootFilter = new Filter();
                                    query.RootFilter.ConditionOperator = container.Request.Operator == TextSearchRequest.SearchTermOperator.And ? FilterOperator.And : FilterOperator.Or;
                                    query.RootFilter.SubFilters = container.Request.SearchTerms.Select(s =>
                                    {
                                        var searchTermFilter = new Filter();
                                        searchTermFilter.Conditions = new List<Condition>();
                                        searchTermFilter.Conditions.Add(new Condition(field, ConditionType.Like, string.Format("%{0}%", s.Text)));
                                        searchTermFilter.Conditions.Add(new Condition("createdon", ConditionType.GreaterEqual, first));
                                        if(limit.HasValue)
                                            searchTermFilter.Conditions.Add(new Condition("createdon", ConditionType.LessThan, limit.Value));
                                        return searchTermFilter;
                                    }).ToList();


                                    var stringFieldMatches = Service.RetreiveAll(query);
                                    foreach (var stringFieldMatch in stringFieldMatches)
                                    {
                                        container.AddMatchedRecord(field, stringFieldMatch);
                                        if (!recordsToOutput.ContainsKey(stringFieldMatch.Id))
                                            recordsToOutput.Add(stringFieldMatch.Id, stringFieldMatch);
                                    }
                                    thisFieldSortedDates.RemoveAt(0);
                                }
                            }
                            catch (Exception ex)
                            {
                                if (ex.Message == null || !ignoreErrorMessages.Any(iem => ex.Message.Contains(iem)))
                                {
                                    container.Response.AddResponseItem(
                                    new TextSearchResponseItem("Error Searching String Fields", recordType, field, ex));
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                if (ex.Message == null || !ignoreErrorMessages.Any(iem => ex.Message.Contains(iem)))
                {
                    container.Response.AddResponseItem(
                    new TextSearchResponseItem("Error Searching String Fields",
                        recordType, ex));
                }
            }
        }

        private List<DateTime> GetDateRangesForSetSearches(string recordType, TextSearchContainer container, out int totalCount)
        {
            totalCount = 0;
            var startDate = new DateTime(1901, 01, 01);
            var sortedDatesTemplate = new List<DateTime>();
            container.Controller.UpdateLevel2Progress(1, 2, string.Format("Loading Search Sets For {0}", Service.GetCollectionName(recordType)));
            //query the created dates of all records in the table
            //this does iterative queries sorting by created date
            //to avoid crm requerying the entire table for each iterative request
            while (true)
            {
                var records =
                    Service.GetFirstX(recordType, ExtractUtility.TextSearchSetSize, new[] { "createdon" },
                        new[]
                                {
                                     new Condition("createdon", ConditionType.GreaterThan, startDate)
                                },
                        new[] { new SortExpression("createdon", SortType.Ascending) }).ToArray();
                totalCount = totalCount + records.Count();
                container.Controller.UpdateLevel2Progress(1, 2, string.Format("Loading Search Sets For {0} ({1} Processed)", Service.GetCollectionName(recordType), totalCount));
                if (!records.Any())
                    break;
                var theseDates =
                    records.Where(r => r.GetDateTime("createdon").HasValue)
                        .Select(r => r.GetDateTime("createdon"))
                        .Cast<DateTime>()
                        .ToList();
                theseDates.Sort();
                sortedDatesTemplate.Add(startDate);
                startDate = theseDates.Last();
                if (records.Count() < ExtractUtility.TextSearchSetSize)
                    break;
            }
            sortedDatesTemplate.Sort();
            return sortedDatesTemplate;
        }

        private IEnumerable<string> GetSearchRecordTypes(TextSearchContainer container)
        {
            var typesToSearch = new List<string>();
            if (!container.Request.SearchAllTypes)
            {
                typesToSearch.AddRange(container.Request.TypesToSearch.Select(r => r.RecordType.Key));
            }
            else
            {

                typesToSearch.AddRange(Service.GetAllRecordTypes().Where(r => Service.GetRecordTypeMetadata(r).Searchable));
                var systemExcludes = ExtractUtility.GetSystemRecordTypesToExclude();
                if(systemExcludes != null)
                {
                    typesToSearch = typesToSearch.Except(systemExcludes).ToList();
                }
                if (container.Request.OtherExclusions != null)
                {
                    typesToSearch = typesToSearch.Except(container.Request.OtherExclusions.Select(r => r.RecordType.Key)).ToList();
                }
                if (container.Request.ExcludeEmails)
                    typesToSearch.RemoveAll(s => s == "email");
                if (container.Request.ExcludeEmails)
                    typesToSearch.RemoveAll(s => s == "post");
            }
            return typesToSearch.OrderBy(n => Service.GetDisplayName(n)).ToArray();
        }

        internal bool IsSearchMatch(string stringValue, TextSearchContainer container)
        {
            var match = false;
            if(stringValue != null)
            {
                foreach(var term in container.Request.SearchTerms)
                {
                    if (stringValue.ToLower().Contains(term.Text.ToLower()))
                    {
                        match = true;
                        break;
                    }
                }
            }
            return match;
        }
    }
}