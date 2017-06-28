using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using $safeprojectname$.Core;
using $safeprojectname$.Xrm;
using System;
using System.Linq;

namespace $safeprojectname$.Aggregate
{
    /// <summary>
    ///     Class for processing and updating aggregate fields in plugins
    ///     For an aggregate from a lookup tthis classes execute method needs to be called from a plugin which is registered
    ///     for the PostOperation, Synchronous event
    ///     for Create, Update, SetStateDynamic and Delete messages
    ///     The Update, SetStateDynamic and Delete messages need to include a preimage with the lookup field, the aggregated
    ///     field, the state and all fields in the filter conditions
    ///     For an aggregate from an association relationahip
    ///     this classes execute method needs to be called
    ///     1. for the entity which is aggregated from a plugin which is registered for the PostOperation, Synchronous event
    ///     for Create, Update, SetStateDynamic and Delete messages
    ///     The Update, SetStateDynamic and Delete messages need to include a preimage with the lookup field, the aggregated
    ///     field, the state and all fields in the filter conditions
    ///     2. for the relationship from a plugin which is registered for the (1) PostOperation, Synchronous, Association event
    ///     (2) PreOperation, Synchronous, Disassociation event
    /// </summary>
    public class XrmLookupAggregate : XrmAggregate
    {
        public XrmLookupAggregate(string recordTypeWithAggregate, string aggregateField, string recordTypeAggregated,
            string aggregatedField, AggregateType linkType, string lookupName)
            : base(recordTypeWithAggregate, aggregateField, recordTypeAggregated, aggregatedField, linkType)
        {
            LookupName = lookupName;
        }

        public string LookupName { get; private set; }

        protected override void ProcessAggregate(XrmEntityPlugin plugin)
        {
            if (plugin is XrmEntityPlugin)
                ProcessLookupAggregate(plugin);
        }

        private void ProcessLookupAggregate(XrmEntityPlugin plugin)
        {
            CheckInitialValue(plugin);
            CheckAggregation(plugin);
        }

        private void CheckAggregation(XrmEntityPlugin plugin)
        {
            if (plugin.TargetType != RecordTypeAggregated)
                return;
            if (
                (plugin.MessageName == PluginMessage.Create || plugin.MessageName == PluginMessage.Update ||
                 plugin.MessageName == PluginMessage.Delete)
                && plugin.Stage == PluginStage.PostEvent
                && plugin.Mode == PluginMode.Synchronous
                )
            {
                var isDependencyChanging = false;

                switch (plugin.MessageName)
                {
                    case PluginMessage.Delete:
                        {
                            isDependencyChanging = plugin.PreImageEntity.Contains(LookupName) &&
                                                   plugin.MeetsConditionsChanging(Filters);
                            break;
                        }
                    case PluginMessage.Update:
                        {
                            if (plugin.FieldChanging(LookupName)
                                || (AggregatedField != null && plugin.FieldChanging(AggregatedField))
                                || (LinkEntity != null && plugin.FieldChanging(LinkEntity.LinkFromAttributeName)))
                                isDependencyChanging = true;
                            else
                                isDependencyChanging = plugin.MeetsConditionsChanging(Filters);
                            break;
                        }
                    case PluginMessage.Create:
                        {
                            isDependencyChanging =
                                plugin.TargetEntity.Contains(LookupName)
                                && (AggregatedField == null || plugin.TargetEntity.Contains(AggregatedField))
                                && plugin.MeetsConditionsChanging(Filters);
                            break;
                        }
                }
                if (isDependencyChanging)
                {
                    object preImageLookup = plugin.PreImageEntity.GetLookupGuid(LookupName);
                    object contextLookup = null;
                    if (plugin.MessageName == PluginMessage.Create || plugin.MessageName == PluginMessage.Update)
                        contextLookup = plugin.TargetEntity.GetLookupGuid(LookupName);
                    var processPreImage = false;
                    var processContextGuid = false;
                    //If they aren't the same do both
                    if (!XrmEntity.FieldsEqual(preImageLookup, contextLookup))
                    {
                        processPreImage = true;
                        processContextGuid = true;
                    }
                    //else just do the first not null one
                    else
                    {
                        if (preImageLookup != null)
                            processPreImage = true;
                        else
                            processContextGuid = true;
                    }
                    if (processPreImage && preImageLookup != null)
                        RefreshAggregate((Guid)preImageLookup, plugin.XrmService, plugin.Controller);
                    if (processContextGuid && contextLookup != null)
                        RefreshAggregate((Guid)contextLookup, plugin.XrmService, plugin.Controller);
                }
            }
        }

        private void CheckInitialValue(XrmEntityPlugin plugin)
        {
            //a count aggregate should be initialised to zero when the record created
            if (plugin.MessageName == PluginMessage.Create
                && plugin.Stage == PluginStage.PreOperationEvent
                && plugin.TargetType == RecordTypeWithAggregate)
            {
                if (NullAmount != null)
                    plugin.SetField(AggregateField, NullAmount);
            }
        }

        public void RefreshAggregate(Guid id, XrmService service, LogController controller)
        {
            var newValue = GetAggregate(id, service);
            controller.LogLiteral(string.Format("Refreshing Field {0} In {1} To Value {2}", AggregateField, RecordTypeWithAggregate, newValue));
            service.SetFieldIfChanging(RecordTypeWithAggregate, id, AggregateField, newValue);
        }

        public object GetAggregate(Guid id, XrmService service)
        {
            object newValue = null;

            switch (AggregateType)
            {
                case AggregateType.Exists:
                    {
                        //if the aggregate returns a result > 0 then one exists
                        var fetch = GetLookupFetch(id);
                        var result = service.RetrieveAllFetch(fetch);
                        newValue = result.Any() &&
                               XrmEntity.GetInt(result.ElementAt(0).GetFieldValue(FetchAlias)) > 0;
                        break;
                    }
                case AggregateType.Count:
                    {
                        var result = service.RetrieveAllFetch(GetLookupFetch(id));
                        if (result.Any())
                            newValue = result.ElementAt(0).GetFieldValue(FetchAlias);
                        break;
                    }
                case AggregateType.Sum:
                    {
                        var result = service.RetrieveAllFetch(GetLookupFetch(id));
                        if (result.Any())
                            newValue = result.ElementAt(0).GetFieldValue(FetchAlias);
                        break;
                    }
                case AggregateType.Min:
                    {
                        var query = GetAggregatedRecordQueryForLookup(id);
                        query.AddOrder(AggregatedField, OrderType.Ascending);
                        var minRecord = service.RetrieveFirst(query);
                        newValue = minRecord.GetField(AggregatedField);
                        break;
                    }
                case AggregateType.CSV:
                case AggregateType.PSV:
                    {
                        var query = GetAggregatedRecordQueryForLookup(id);

                        query.AddOrder(AggregatedField, OrderType.Ascending);
                        var records = service.RetrieveAll(query);
                        var labels =
                            records.Select(
                                delegate (Entity item) { return item.GetField(AggregatedField); }).
                                ToArray();
                        if (AggregateType == AggregateType.CSV)
                            newValue = string.Join(", ", labels);
                        else
                            newValue = string.Join("|", labels);
                        newValue = ((string)newValue).Left(1000);
                        break;
                    }
            }
            if (newValue == null && NullAmount != null)
                newValue = NullAmount;
            return newValue;
        }

        private QueryExpression GetAggregatedRecordQueryForLookup(Guid id)
        {
            var query = new QueryExpression(RecordTypeAggregated);
            query.Criteria.AddCondition(LookupName, ConditionOperator.Equal, id);
            foreach (var condition in Filters)
                query.Criteria.AddCondition(condition);
            query.ColumnSet.AddColumn(LookupName);
            if (AggregatedField != null)
            {
                query.ColumnSet.AddColumn(AggregatedField);
                query.Criteria.AddCondition(new ConditionExpression(AggregatedField, ConditionOperator.NotNull));
            }
            if (LinkEntity != null)
                query.LinkEntities.Add(LinkEntity);
            return query;
        }

        public string GetLookupFetch(Guid id)
        {
            string aggregateFieldNode;
            switch (AggregateType)
            {
                case AggregateType.Exists:
                    {
                        aggregateFieldNode = "<attribute name='" + AggregatedField +
                                             "' aggregate='count' distinct = 'true' alias='" + FetchAlias + "'/>";
                        break;
                    }
                case AggregateType.Count:
                    {
                        aggregateFieldNode = "<attribute name='" + AggregatedField + "' aggregate='count' alias='" +
                                             FetchAlias + "'/>";
                        break;
                    }
                case AggregateType.Sum:
                    {
                        aggregateFieldNode = "<attribute name='" + AggregatedField + "' aggregate='sum' alias='" +
                                             FetchAlias + "'/>";
                        break;
                    }
                default:
                    {
                        throw new ArgumentOutOfRangeException("Fetch aggregate not implemented for " + AggregateType);
                    }
            }
            return
                "<fetch version='1.0' aggregate='true' output-format='xml-platform' mapping='logical' distinct='false'>"
                + "<entity name='" + RecordTypeAggregated + "'>"
                + aggregateFieldNode
                + "<filter type='and'>"
                + "<condition attribute='" + LookupName + "' operator='eq' uiname='' uitype='" + RecordTypeWithAggregate +
                "' value='" + id + "' />"
                + String.Join("", Filters.Select(GetConditionFetchNode))
                + "</filter>"
                + String.Join("", GetLinkedEntityFetchNode(LinkEntity))
                + "</entity>"
                + "</fetch>";
        }

        private static string GetLinkedEntityFetchNode(LinkEntity l)
        {
            return
                l == null
                    ? ""
                    : "<link-entity name='" + l.LinkToEntityName + "' from='" + l.LinkToAttributeName + "' to='" +
                      l.LinkFromAttributeName + "'>"
                      + "<filter type='and'>"
                      + String.Join("", l.LinkCriteria.Conditions.Select(GetConditionFetchNode))
                      + "</filter>"
                      + "</link-entity>";
        }

        public object NullAmount { get; set; }
    }
}