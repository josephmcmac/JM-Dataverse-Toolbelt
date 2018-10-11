using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using $safeprojectname$.Core;
using $safeprojectname$.Xrm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace $safeprojectname$.Rollups
{
    public abstract class RollupService
    {
        public XrmService XrmService { get; }

        public abstract IEnumerable<LookupRollup> AllRollups { get; }

        public RollupService(XrmService xrmService)
        {
            XrmService = xrmService;
        }

        public void SetInitialValues(XrmEntityPlugin plugin)
        {
            //a count Rollup should be initialised to zero when the record created
            if (plugin.MessageName == PluginMessage.Create
                && plugin.Stage == PluginStage.PreOperationEvent)
            {
                var RollupsToProcess = GetRollupsForRollupType(plugin.TargetType)
                    .Where(a => AllowsDifferenceChange(a.RollupType))
                    .ToArray();
                foreach (var Rollup in RollupsToProcess)
                {
                    if (Rollup.NullAmount != null)
                        plugin.SetField(Rollup.RollupField, Rollup.NullAmount);
                }
            }
        }

        private void ExecuteDependencyPluginRefresh(XrmEntityPlugin plugin, LookupRollup rollup)
        {
            var idsRequireRefresh = new List<Guid>();

            var isDependencyChanging = false;

            if (
                (plugin.MessageName == PluginMessage.Create || plugin.MessageName == PluginMessage.Update ||
                 plugin.MessageName == PluginMessage.Delete)
                && plugin.Stage == PluginStage.PostEvent
                && plugin.Mode == PluginMode.Synchronous
                )
            {
                switch (plugin.MessageName)
                {
                    case PluginMessage.Delete:
                        {
                            isDependencyChanging = plugin.PreImageEntity.Contains(rollup.LookupName) &&
                                                   plugin.MeetsConditionsChanging(rollup.Filters);
                            break;
                        }
                    case PluginMessage.Update:
                        {
                            if (plugin.FieldChanging(rollup.LookupName)
                                || (rollup.FieldRolledup != null && plugin.FieldChanging(rollup.FieldRolledup))
                                || (rollup.LinkEntity != null && plugin.FieldChanging(rollup.LinkEntity.LinkFromAttributeName)))
                                isDependencyChanging = true;
                            else
                                isDependencyChanging = plugin.MeetsConditionsChanging(rollup.Filters);
                            break;
                        }
                    case PluginMessage.Create:
                        {
                            isDependencyChanging =
                                plugin.TargetEntity.Contains(rollup.LookupName)
                                && (rollup.FieldRolledup == null || plugin.TargetEntity.Contains(rollup.FieldRolledup))
                                && plugin.MeetsConditionsChanging(rollup.Filters);
                            break;
                        }
                }
                if (isDependencyChanging)
                {
                    object preImageLookup = plugin.PreImageEntity.GetLookupGuid(rollup.LookupName);
                    object contextLookup = null;
                    if (plugin.MessageName == PluginMessage.Create || plugin.MessageName == PluginMessage.Update)
                        contextLookup = plugin.TargetEntity.GetLookupGuid(rollup.LookupName);
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
                        idsRequireRefresh.Add((Guid)preImageLookup);
                    if (processContextGuid && contextLookup != null)
                        idsRequireRefresh.Add((Guid)contextLookup);
                }
            }
            foreach (var id in idsRequireRefresh)
            {
                RefreshRollup(id, rollup);
            }
        }
        public void RefreshRollup(Guid id, LookupRollup rollup)
        {
            var newValue = GetRollup(rollup, id);
            XrmService.SetFieldIfChanging(rollup.RecordTypeWithRollup, id, rollup.RollupField, newValue);
        }

        private bool AllowsDifferenceChange(RollupType type)
        {
            return new[] { RollupType.Count, RollupType.Sum }.Contains(type);
        }

        private void ExecuteDependencyPluginRefreshForDifferenceNotSupported(XrmEntityPlugin plugin)
        {
            var dictionaryForDifferences = new Dictionary<string, Dictionary<Guid, List<KeyValuePair<string, object>>>>();

            var RollupsToProcess = GetRollupsForRolledupType(plugin.TargetType)
                .Where(a => !AllowsDifferenceChange(a.RollupType))
                .ToArray();
            foreach (var rollup in RollupsToProcess)
            {
                ExecuteDependencyPluginRefresh(plugin, rollup);
            }
        }

        /// <summary>
        /// Processes changes in the record type Rollup
        /// WARNING some types of Rollups have potential for deadlocks!
        /// </summary>
        /// <param name="plugin"></param>
        public void ExecuteDependencyPlugin(XrmEntityPlugin plugin)
        {
            ExecuteDependencyPluginDifferences(plugin);
            ExecuteDependencyPluginRefreshForDifferenceNotSupported(plugin);
        }

        private void ExecuteDependencyPluginDifferences(XrmEntityPlugin plugin)
        {
            var dictionaryForDifferences = new Dictionary<string, Dictionary<Guid, List<KeyValuePair<string, object>>>>();

            var rollupsToProcess = GetRollupsForRolledupType(plugin.TargetType)
                .Where(a => AllowsDifferenceChange(a.RollupType))
                .ToArray();

            if (plugin.IsMessage(PluginMessage.Create, PluginMessage.Update, PluginMessage.Delete)
                && plugin.IsStage(PluginStage.PostEvent)
                && plugin.IsMode(PluginMode.Synchronous))
            {
                //this dictionary will capture the changes we need to apply to parent records for all Rollups
                //type -> ids -> fields . values
                Action<string, Guid, string, object> addDifferenceToApply = (type, id, field, val) =>
                {
                    if (!dictionaryForDifferences.ContainsKey(type))
                        dictionaryForDifferences.Add(type, new Dictionary<Guid, List<KeyValuePair<string, object>>>());
                    if (!dictionaryForDifferences[type].ContainsKey(id))
                        dictionaryForDifferences[type].Add(id, new List<KeyValuePair<string, object>>());
                    dictionaryForDifferences[type][id].Add(new KeyValuePair<string, object>(field, val));
                };

                foreach (var rollup in rollupsToProcess)
                {
                    if (rollup.LinkEntity != null)
                        throw new NotImplementedException("Rollups with a LinkEntity are not implemented for the ProcessAsDifferences method");

                    //capture required facts in the plugin context to process our ifs and elses
                    var metConditionsBefore = XrmEntity.MeetsConditions(plugin.GetFieldFromPreImage, rollup.Filters);
                    var meetsConditionsAfter = plugin.MessageName == PluginMessage.Delete
                        ? false
                        : XrmEntity.MeetsConditions(plugin.GetField, rollup.Filters);
                    var linkedIdBefore = XrmEntity.GetLookupType(plugin.GetFieldFromPreImage(rollup.LookupName)) == rollup.RecordTypeWithRollup
                        ? plugin.GetLookupGuidPreImage(rollup.LookupName)
                        : (Guid?)null;
                    var linkedIdAfter = plugin.MessageName == PluginMessage.Delete || XrmEntity.GetLookupType(plugin.GetField(rollup.LookupName)) != rollup.RecordTypeWithRollup
                        ? (Guid?)null
                        : plugin.GetLookupGuid(rollup.LookupName);
                    var isValueChanging = plugin.FieldChanging(rollup.FieldRolledup);

                    //this covers all scenarios I thought of which require changing the value in a parent record
                    if (linkedIdBefore.HasValue && linkedIdBefore == linkedIdAfter)
                    {
                        //the same record linked before and after
                        if (metConditionsBefore && meetsConditionsAfter)
                        {
                            //if part of Rollup before and after
                            if (isValueChanging)
                            {
                                //and the value is changing then apply difference
                                if (rollup.RollupType == RollupType.Sum)
                                {
                                    addDifferenceToApply(rollup.RecordTypeWithRollup, linkedIdAfter.Value, rollup.RollupField, GetDifferenceToApply(plugin.GetFieldFromPreImage(rollup.FieldRolledup), plugin.GetField(rollup.FieldRolledup)));
                                }
                                else if (rollup.RollupType == RollupType.Count)
                                {
                                    //for count only adjust if changing between null and not null
                                    if (plugin.GetFieldFromPreImage(rollup.FieldRolledup) == null)
                                    {
                                        addDifferenceToApply(rollup.RecordTypeWithRollup, linkedIdAfter.Value, rollup.RollupField, 1);
                                    }
                                    else if (plugin.GetField(rollup.FieldRolledup) == null)
                                    {
                                        addDifferenceToApply(rollup.RecordTypeWithRollup, linkedIdAfter.Value, rollup.RollupField, -1);
                                    }
                                }
                            }
                        }
                        if (!metConditionsBefore && meetsConditionsAfter)
                        {
                            //if was not part of Rollup before but is now apply the entire value
                            if (rollup.RollupType == RollupType.Sum)
                            {
                                addDifferenceToApply(rollup.RecordTypeWithRollup, linkedIdAfter.Value, rollup.RollupField, plugin.GetField(rollup.FieldRolledup));
                            }
                            else if (rollup.RollupType == RollupType.Count)
                            {
                                addDifferenceToApply(rollup.RecordTypeWithRollup, linkedIdAfter.Value, rollup.RollupField, 1);
                            }
                        }
                        if (metConditionsBefore && !meetsConditionsAfter)
                        {
                            //if was part of Rollup before but not now apply the entire value negative
                            if (rollup.RollupType == RollupType.Sum)
                            {
                                addDifferenceToApply(rollup.RecordTypeWithRollup, linkedIdAfter.Value, rollup.RollupField, GetNegative(plugin.GetFieldFromPreImage(rollup.FieldRolledup)));
                            }
                            else if (rollup.RollupType == RollupType.Count)
                            {
                                addDifferenceToApply(rollup.RecordTypeWithRollup, linkedIdAfter.Value, rollup.RollupField, -1);
                            }
                        }
                    }
                    else
                    {
                        //different parent linked before and after
                        if (linkedIdBefore.HasValue && metConditionsBefore)
                        {
                            //if was part of previous linked records Rollup then negate the previous value
                            if (rollup.RollupType == RollupType.Sum)
                            {
                                addDifferenceToApply(rollup.RecordTypeWithRollup, linkedIdBefore.Value, rollup.RollupField, GetNegative(plugin.GetFieldFromPreImage(rollup.FieldRolledup)));
                            }
                            else if (rollup.RollupType == RollupType.Count)
                            {
                                addDifferenceToApply(rollup.RecordTypeWithRollup, linkedIdBefore.Value, rollup.RollupField, -1);
                            }
                        }
                        if (linkedIdAfter.HasValue && meetsConditionsAfter)
                        {
                            //if part of new linked records Rollup then apply the entire value
                            if (rollup.RollupType == RollupType.Sum)
                            {
                                addDifferenceToApply(rollup.RecordTypeWithRollup, linkedIdAfter.Value, rollup.RollupField, plugin.GetField(rollup.FieldRolledup));
                            }
                            else if (rollup.RollupType == RollupType.Count)
                            {
                                addDifferenceToApply(rollup.RecordTypeWithRollup, linkedIdAfter.Value, rollup.RollupField, 1);
                            }
                        }
                    }
                }
            }
            if (dictionaryForDifferences.Any())
            {
                plugin.Trace("Updating Rollup Differences");
                foreach (var item in dictionaryForDifferences)
                {
                    foreach (var field in item.Value)
                    {
                        foreach (var value in field.Value)
                        {
                            plugin.Trace("Updating " + item.Key + " " + field.Key + " " + value.Key + " " + value.Value + (value.Value == null ? "(null)" : (" (" + value.Value.GetType().Name + ")")));
                        }
                    }
                }
            }
            //apply all required changes to parents we captured
            //type -> ids -> fields . values
            foreach (var item in dictionaryForDifferences)
            {
                var targetType = item.Key;
                foreach (var idUpdates in item.Value)
                {
                    var id = idUpdates.Key;
                    //lock the parent record then retreive it
                    plugin.XrmService.SetField(targetType, id, "modifiedon", DateTime.UtcNow);
                    var fieldsForUpdating = idUpdates.Value.Select(kv => kv.Key).ToArray();
                    var targetRecord = plugin.XrmService.Retrieve(targetType, id, idUpdates.Value.Select(kv => kv.Key));
                    //update the fields
                    foreach (var fieldUpdate in idUpdates.Value)
                    {
                        targetRecord.SetField(fieldUpdate.Key, XrmEntity.SumFields(new[] { fieldUpdate.Value, targetRecord.GetField(fieldUpdate.Key) }));
                    }
                    plugin.XrmService.Update(targetRecord, fieldsForUpdating);
                }
            }
        }

        public IEnumerable<LookupRollup> GetRollupsForRolledupType(string entityType)
        {
            return AllRollups
                .Where(a => a.RecordTypeRolledup == entityType)
                .ToArray();
        }

        public IEnumerable<LookupRollup> GetRollupsForRollupType(string entityType)
        {
            return AllRollups
                .Where(a => a.RecordTypeWithRollup == entityType)
                .ToArray();
        }

        protected string FetchAlias
        {
            get { return "Rollupvalue"; }
        }

        public object GetRollup(LookupRollup rollup, Guid id)
        {
            object newValue = null;

            switch (rollup.RollupType)
            {
                case RollupType.Exists:
                    {
                        //if the Rollup returns a result > 0 then one exists
                        var fetch = GetLookupFetch(rollup, id);
                        var result = XrmService.Fetch(fetch);
                        newValue = result.Any() &&
                               XrmEntity.GetInt(result.First().GetField(FetchAlias)) > 0;
                        break;
                    }
                case RollupType.Count:
                    {
                        var result = XrmService.Fetch(GetLookupFetch(rollup, id));
                        if (result.Any())
                            newValue = result.ElementAt(0).GetField(FetchAlias);
                        break;
                    }
                case RollupType.Sum:
                    {
                        var result = XrmService.Fetch(GetLookupFetch(rollup, id));
                        if (result.Any())
                            newValue = result.ElementAt(0).GetField(FetchAlias);
                        break;
                    }
                case RollupType.Min:
                    {
                        var query = GetRollupQueryForLookup(rollup, id);
                        query.AddOrder(rollup.FieldRolledup, OrderType.Ascending);
                        var minRecord = XrmService.RetrieveFirst(query);
                        newValue = minRecord.GetField(rollup.FieldRolledup);
                        break;
                    }
                case RollupType.CSV:
                case RollupType.PSV:
                    {
                        var query = GetRollupQueryForLookup(rollup, id);

                        query.AddOrder(rollup.FieldRolledup, OrderType.Ascending);
                        var records = XrmService.RetrieveAll(query);
                        var labels =
                            records.Select(e => e.GetField(rollup.FieldRolledup)).
                                ToArray();
                        if (rollup.RollupType == RollupType.CSV)
                            newValue = string.Join(", ", labels);
                        else
                            newValue = string.Join("|", labels);
                        newValue = ((string)newValue).Left(1000);
                        break;
                    }
            }
            if (newValue == null && rollup.NullAmount != null)
                newValue = rollup.NullAmount;
            if (newValue != null && rollup.ObjectType != null)
            {
                if (rollup.ObjectType == typeof(decimal))
                {
                    newValue = Convert.ToDecimal(newValue.ToString());
                }
            }
            return newValue;
        }

        private QueryExpression GetRollupQueryForLookup(LookupRollup rollup, Guid id)
        {
            var query = new QueryExpression(rollup.RecordTypeRolledup);
            query.Criteria.AddCondition(rollup.LookupName, ConditionOperator.Equal, id);
            foreach (var condition in rollup.Filters)
                query.Criteria.AddCondition(condition);
            query.ColumnSet.AddColumn(rollup.LookupName);
            if (rollup.FieldRolledup != null)
            {
                query.ColumnSet.AddColumn(rollup.FieldRolledup);
                query.Criteria.AddCondition(new ConditionExpression(rollup.FieldRolledup, ConditionOperator.NotNull));
            }
            if (rollup.LinkEntity != null)
                query.LinkEntities.Add(rollup.LinkEntity);
            return query;
        }

        public string GetLookupFetch(LookupRollup rollup, Guid id)
        {
            string RollupFieldNode;
            switch (rollup.RollupType)
            {
                case RollupType.Exists:
                    {
                        RollupFieldNode = "<attribute name='" + rollup.FieldRolledup +
                                             "' aggregate='count' distinct = 'true' alias='" + FetchAlias + "'/>";
                        break;
                    }
                case RollupType.Count:
                    {
                        RollupFieldNode = "<attribute name='" + rollup.FieldRolledup + "' aggregate='count' alias='" +
                                             FetchAlias + "'/>";
                        break;
                    }
                case RollupType.Sum:
                    {
                        RollupFieldNode = "<attribute name='" + rollup.FieldRolledup + "' aggregate='sum' alias='" +
                                             FetchAlias + "'/>";
                        break;
                    }
                default:
                    {
                        throw new ArgumentOutOfRangeException("Fetch Rollup not implemented for " + rollup.RollupType);
                    }
            }
            return
                "<fetch version='1.0' aggregate='true' output-format='xml-platform' mapping='logical' distinct='false'>"
                + "<entity name='" + rollup.RecordTypeRolledup + "'>"
                + RollupFieldNode
                + "<filter type='and'>"
                + "<condition attribute='" + rollup.LookupName + "' operator='eq' uiname='' uitype='" + rollup.RecordTypeWithRollup +
                "' value='" + id + "' />"
                + (rollup.FieldRolledup != null ? GetConditionFetchNode(new ConditionExpression(rollup.FieldRolledup, ConditionOperator.NotNull)) : null)
                + String.Join("", rollup.Filters.Select(GetConditionFetchNode))
                + "</filter>"
                + String.Join("", GetLinkedEntityFetchNode(rollup.LinkEntity))
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

        protected static string GetConditionFetchNode(ConditionExpression condition)
        {
            var conditionOperatorString = "";
            switch (condition.Operator)
            {
                case ConditionOperator.Equal:
                    {
                        conditionOperatorString = "eq";
                        break;
                    }
                case ConditionOperator.NotEqual:
                    {
                        conditionOperatorString = "ne";
                        break;
                    }
                case ConditionOperator.In:
                    {
                        conditionOperatorString = "in";

                        break;
                    }
                case ConditionOperator.OnOrBefore:
                    {
                        conditionOperatorString = "on-or-before";
                        break;
                    }
                case ConditionOperator.OnOrAfter:
                    {
                        conditionOperatorString = "on-or-after";
                        break;
                    }
                case ConditionOperator.NotNull:
                    {
                        conditionOperatorString = "not-null";
                        break;
                    }
            }
            if (string.IsNullOrWhiteSpace(conditionOperatorString))
                throw new InvalidPluginExecutionException(
                    string.Format("Error Getting Condition Operator String For Operator Type {0}",
                        condition.Operator));
            if (condition.Operator == ConditionOperator.In)
            {
                var stringBuilder = new StringBuilder();
                stringBuilder.Append(string.Format("<condition attribute='{0}' operator='{1}' >",
                    condition.AttributeName,
                    conditionOperatorString));
                if (condition.Values != null)
                {
                    foreach (var value in condition.Values)
                    {
                        if (value is IEnumerable<object>)
                        {
                            foreach (var nestValue in (IEnumerable<object>)value)
                            {
                                stringBuilder.Append(string.Format("<value>{0}</value>", nestValue));
                            }
                        }
                        else
                            stringBuilder.Append(string.Format("<value>{0}</value>", value));
                    }
                }
                stringBuilder.Append("</condition>");
                return stringBuilder.ToString();
            }
            if (condition.Values == null || condition.Values.Count == 0)
            {
                return string.Format("<condition attribute='{0}' operator='{1}' />", condition.AttributeName, conditionOperatorString);
            }
            else
            {
                var fetchValue = condition.Values[0];
                if (fetchValue is DateTime)
                {
                    if (condition.Operator == ConditionOperator.OnOrAfter
                        || condition.Operator == ConditionOperator.OnOrBefore)
                    {
                        fetchValue = ((DateTime)fetchValue).ToString("yyyy-MM-dd");
                    }
                    else
                    {
                        fetchValue = ((DateTime)fetchValue).ToString("yyyy-MM-dd HH:mm:ss.fff");
                    }
                }

                return string.Format("<condition attribute='{0}' operator='{1}' value='{2}' />", condition.AttributeName,
                    conditionOperatorString, fetchValue);
            }
        }

        private static object GetDifferenceToApply(object oldValue, object newValue)
        {
            if (oldValue == null && newValue == null)
            {
                return null;
            }
            if (oldValue is int || newValue is int)
            {
                var differenceCalc = newValue == null ? 0 : (int)newValue;
                if (oldValue != null)
                    differenceCalc = differenceCalc - (int)oldValue;
                return differenceCalc;
            }
            if (oldValue is decimal || newValue is decimal)
            {
                var differenceCalc = newValue == null ? 0 : (decimal)newValue;
                if (oldValue != null)
                    differenceCalc = differenceCalc - (decimal)oldValue;
                return differenceCalc;
            }
            if (oldValue is Money || newValue is Money)
            {
                var differenceCalc = newValue == null ? (decimal)0 : ((Money)newValue).Value;
                if (oldValue != null)
                    differenceCalc = differenceCalc - ((Money)oldValue).Value;
                return new Money(differenceCalc);
            }
            throw new NotImplementedException("The GetDifferenceToApply Method Is Not Implemented For The Type " + oldValue == null ? newValue.GetType().Name : oldValue.GetType().Name);
        }
        private static object GetNegative(object value)
        {
            if (value == null)
            {
                return null;
            }
            if (value is int)
            {
                return -1 * (int)value;
            }
            if (value is decimal)
            {
                return -1 * (decimal)value;
            }
            if (value is Money)
            {
                return new Money(-1 * ((Money)value).Value);
            }
            throw new NotImplementedException("The GetNegative Method Is Not Implemented For The Type " + value.GetType().Name);
        }
    }
}