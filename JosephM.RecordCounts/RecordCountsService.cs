using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using JosephM.Core.Extentions;
using JosephM.Core.Log;
using JosephM.Core.Service;
using JosephM.Core.Utility;
using JosephM.Record.Extentions;
using JosephM.Record.IService;
using JosephM.Record.Metadata;
using JosephM.Record.Xrm.XrmRecord;
using JosephM.Xrm.Schema;
using JosephM.Record.Query;
using System.Threading;

namespace JosephM.RecordCounts.Exporter
{
    public class RecordCountsService :
        ServiceBase<RecordCountsRequest, RecordCountsResponse, RecordCountsResponseItem>
    {
        public RecordCountsService(XrmRecordService service)
        {
            Service = service;
        }

        private XrmRecordService Service { get; set; }

        public override void ExecuteExtention(RecordCountsRequest request,
            RecordCountsResponse response,
            LogController controller)
        {
            controller.LogLiteral("Loading Types");

            var recordTypes = request.AllRecordTypes
                ? Service.GetAllRecordTypes()
                .Where(r => Service.GetRecordTypeMetadata(r).Searchable)
                .OrderBy(n => Service.GetDisplayName(n))
                .ToArray()
                : request.RecordTypes.Select(p => p.RecordType.Key).ToArray();

            if (request.OnlyIncludeSelectedOwner)
                recordTypes = recordTypes.Where(r => Service.GetRecordTypeMetadata(r).HasOwner).ToArray();

            var numberOfTypes = recordTypes.Count();
            var numberOfTypesCompleted = 0;

            var noOwnerIndex = "Organisation";
            var recordCountsByUser = new Dictionary<string, Dictionary<string, long>>();
            //due to limitations in crm web service cannot do a standard aggregate
            //as they are limitied to 50K records OOTB
            //need to query all records and manually aggregate
            foreach(var recordType in recordTypes)
            {
                var thisDictionary = new Dictionary<string, long>();

                try
                {
                    var metadata = Service.GetRecordTypeMetadata(recordType);
                    var hasOwner = metadata.HasOwner;
                    if (!hasOwner)
                        thisDictionary.Add(noOwnerIndex, 0);
                    long totalThisIteration = 0;
                    var func = hasOwner
                        ? ((records) =>
                       {
                           totalThisIteration += records.Count();
                           controller.UpdateProgress(numberOfTypesCompleted, numberOfTypes, string.Format("Counting {0} ({1})", recordType, totalThisIteration));
                           foreach (var record in records)
                           {
                               var ownerType = record.GetLookupType("ownerid");
                               var ownerId = record.GetLookupId("ownerid");
                               var format = string.Format("{0}:{1}", ownerType, ownerId);
                               if (!thisDictionary.ContainsKey(format))
                                   thisDictionary.Add(format, 0);
                               thisDictionary[format]++;
                           }

                       })
                        : (Action<IEnumerable<IRecord>>)((records) =>
                       {
                           totalThisIteration += records.Count();
                           controller.UpdateProgress(numberOfTypesCompleted, numberOfTypes, string.Format("Counting {0} ({1})", recordType, totalThisIteration));
                           foreach (var record in records)
                           {
                               thisDictionary[noOwnerIndex]++;
                           }
                       });
                    var fields = hasOwner
                        ? new[] { "ownerid" }
                        : new string[0];
                    var conditions = request.OnlyIncludeSelectedOwner
                        ? new[] { new Condition("ownerid", ConditionType.Equal, request.Owner.Id) }
                        : new Condition[0];

                    Service.ProcessResults(recordType, fields, conditions, func);
                    recordCountsByUser.Add(recordType, thisDictionary);
                }
                catch (Exception ex)
                {
                    response.AddResponseItem(new RecordCountsResponseItem(recordType, "Error Generating Counts", ex));
                }
                finally
                {
                    numberOfTypesCompleted++;
                }
            }
            controller.LogLiteral("Generating CSV files");

            var groupByOwner = request.GroupCountsByOwner
                || request.OnlyIncludeSelectedOwner;
            if (!groupByOwner)
            {
                var totals = new List<RecordCount>();
                foreach(var dictionary in recordCountsByUser)
                {
                    totals.Add(new RecordCount(Service.GetDisplayName(dictionary.Key), dictionary.Value.Sum(kv => kv.Value)));
                }
                response.RecordCounts = totals;
            }
            if (groupByOwner)
            {
                var teamNameField = Service.GetRecordTypeMetadata(Entities.team).PrimaryFieldSchemaName;
                var userNameField = Service.GetRecordTypeMetadata(Entities.systemuser).PrimaryFieldSchemaName;

                var allDistinctOwners = recordCountsByUser
                    .SelectMany(kv => kv.Value.Select(kv2 => kv2.Key))
                    .Distinct();
                var allTeams = allDistinctOwners
                    .Where(s => s.StartsWith(Entities.team))
                    .Distinct();
                var allUsers = allDistinctOwners
                    .Where(s => s.StartsWith(Entities.systemuser))
                    .Distinct();

                var ownerKeyLabels = new Dictionary<string, string>();
                var teams = Service.RetrieveAll(Entities.team, new[] { teamNameField } );
                foreach(var team in teams)
                {
                    ownerKeyLabels.Add(string.Format("{0}:{1}", Entities.team, team.Id), team.GetStringField(teamNameField));
                }
                var users = Service.RetrieveAll(Entities.systemuser, null);
                foreach (var user in users)
                {
                    ownerKeyLabels.Add(string.Format("{0}:{1}", Entities.systemuser, user.Id), user.GetStringField(userNameField));
                }

                var ownerTotals = new List<RecordCountByOwner>();
                foreach (var dictionary in recordCountsByUser)
                {
                    foreach(var owner in dictionary.Value)
                    {
                        if(owner.Key == noOwnerIndex)
                            ownerTotals.Add(new RecordCountByOwner(Service.GetDisplayName(dictionary.Key), owner.Value, noOwnerIndex, noOwnerIndex));
                        else
                        {
                            if(ownerKeyLabels.ContainsKey(owner.Key))
                            {
                                ownerTotals.Add(new RecordCountByOwner(Service.GetDisplayName(dictionary.Key), owner.Value, Service.GetDisplayName(owner.Key.Split(':')[0]), ownerKeyLabels[owner.Key]));
                            }
                            else
                            {
                                ownerTotals.Add(new RecordCountByOwner(Service.GetDisplayName(dictionary.Key), owner.Value, "Unknown", "Unknown"));
                            }
                        }
                    }
                    
                }
                response.RecordCounts = ownerTotals;
            }
        }
    }
}