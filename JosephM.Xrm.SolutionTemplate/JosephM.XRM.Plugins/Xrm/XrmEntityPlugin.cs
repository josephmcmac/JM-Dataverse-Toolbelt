using System;
using System.Collections.Generic;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace $safeprojectname$.Xrm
{
    /// <summary>
    ///     Class storing properties and methods common for plugins on all entity types
    ///     A specific entity may extend this class to implement plugins specific for that entity type
    /// </summary>
    public abstract class XrmEntityPlugin : XrmPlugin
    {
        #region instance properties

        /// <summary>
        ///     The new state for a set state message
        /// </summary>
        public int SetStateState
        {
            get { return ((OptionSetValue)Context.InputParameters["State"]).Value; }
        }

        /// <summary>
        /// The id of the target record
        /// </summary>
        public Guid TargetId
        {
            get
            {
                if (MessageName == PluginMessage.Create || MessageName == PluginMessage.Update)
                    return TargetEntity.Id;
                else if (MessageName == PluginMessage.SetStateDynamicEntity || MessageName == PluginMessage.Delete ||
                         MessageName == PluginMessage.QualifyLead || MessageName == PluginMessage.Assign || MessageName == PluginMessage.Merge)
                    return TargetEntityReference.Id;
                else if (Context.InputParameters.Contains("EmailId") && Context.InputParameters["EmailId"] is Guid)
                    return (Guid)Context.InputParameters["EmailId"];
                else if (MessageName == PluginMessage.Cancel)
                {
                    var orderClose = Context.InputParameters["OrderClose"] as Entity;
                    if (orderClose == null)
                        throw new NullReferenceException("Could not extract OrderClose in " + PluginMessage.Cancel);
                    var salesOrderId = orderClose.GetLookupGuid("salesorderid");
                    if (!salesOrderId.HasValue)
                        throw new NullReferenceException("Could not extract salesOrderId in OrderClose");
                    return salesOrderId.Value;
                }
                else if (MessageName == PluginMessage.Win || MessageName == PluginMessage.Lose)
                {
                    var oppClose = Context.InputParameters["OpportunityClose"] as Entity;
                    if (oppClose == null)
                        throw new NullReferenceException("Could not extract OpportunityClose in " + MessageName);
                    var opportunityId = oppClose.GetLookupGuid("opportunityid");
                    if (!opportunityId.HasValue)
                        throw new NullReferenceException("Could not extract opportunityid in OpportunityClose");
                    return opportunityId.Value;
                }
                else
                    throw new InvalidPluginExecutionException("Error Getting Target Id");
            }
        }

        /// <summary>
        ///     The type of the target record
        /// </summary>
        public override sealed string TargetType
        {
            get
            {
                if (MessageName == PluginMessage.Create || MessageName == PluginMessage.Update)
                    return TargetEntity.LogicalName;
                else if (MessageName == PluginMessage.SetStateDynamicEntity || MessageName == PluginMessage.Delete ||
                         MessageName == PluginMessage.QualifyLead || MessageName == PluginMessage.Assign)
                    return TargetEntityReference.LogicalName;
                else if (MessageName == PluginMessage.Send)
                    return "email";
                else if (MessageName == PluginMessage.Win || MessageName == PluginMessage.Lose)
                    return "opportunity";
                else
                    throw new InvalidPluginExecutionException("Error Getting TargetType");
            }
        }

        /// <summary>
        ///     The input entity reference if a valid message type
        /// </summary>
        public EntityReference TargetEntityReference
        {
            get
            {
                if (Context.InputParameters.Contains("Target") && Context.InputParameters["Target"] is EntityReference)
                    return (EntityReference)Context.InputParameters["Target"];
                if (Context.InputParameters.Contains("EntityMoniker") &&
                    Context.InputParameters["EntityMoniker"] is EntityReference)
                    return (EntityReference)Context.InputParameters["EntityMoniker"];
                if (Context.InputParameters.Contains("LeadId") && Context.InputParameters["LeadId"] is EntityReference)
                    return (EntityReference)Context.InputParameters["LeadId"];
                else
                    throw new InvalidPluginExecutionException("Error Extracting Target Entity Reference");
            }
        }

        /// <summary>
        ///     The target entity in a Create or Update message else null
        /// </summary>
        public Entity TargetEntity
        {
            get
            {
                if (Context.InputParameters.Contains("Target") && Context.InputParameters["Target"] is Entity)
                    return (Entity)Context.InputParameters["Target"];
                else
                    return null;
            }
        }

        /// <summary>
        ///     NULL IF NOT REGISTERED OR NAMED INCORRECTLY - the preimage entity registered against the plugin step. Note the
        ///     alias must be 'PreImage' for this to work
        /// </summary>
        public Entity PreImageEntity
        {
            get
            {
                if (Context.PreEntityImages.Contains("PreImage"))
                    return Context.PreEntityImages["PreImage"];
                else
                    return null;
            }
        }

        #endregion

        #region instance methods

        /// <summary>
        ///     MAYBE TRUE FOR DELETE! FIELD MUST BE IN PREIMAGE FOR UPDATE STEP! Returns if the fields value is logically changing
        ///     by inspecting the Target and Preimage
        /// </summary>
        public bool FieldChanging(string fieldName)
        {
            if (MessageName == PluginMessage.Create || MessageName == PluginMessage.Update)
                return TargetEntity.Contains(fieldName) &&
                       !XrmEntity.FieldsEqual(PreImageEntity.GetField(fieldName), TargetEntity.GetField(fieldName));
            else if (MessageName == PluginMessage.Delete)
                return GetFieldFromPreImage(fieldName) != null;
            else
                //not sure how to get status if a setstate message
                throw new InvalidPluginExecutionException("FieldChanging Not Implemented for plugin message " +
                                                          MessageName);
        }

        /// <summary>
        /// Return if the boolean fields value is changing to true
        /// </summary>
        public bool BooleanChangingToTrue(string field)
        {
            return FieldChanging(field) && GetBoolean(field);
        }

        /// <summary>
        /// Return if the boolean fields value is changing to not true
        /// </summary>
        public bool BooleanChangingToFalse(string field)
        {
            return !GetBoolean(field) && GetBooleanPreImage(field);
        }

        /// <summary>
        ///     MAYBE TRUE FOR DELETE! FIELD MUST BE IN PREIMAGE FOR UPDATE STEP! Returns if the either of the fields value is
        ///     logically changing by inspecting the Target and Preimage
        /// </summary>
        public bool FieldChanging(IEnumerable<string> fields)
        {
            foreach (var field in fields)
            {
                if (FieldChanging(field))
                    return true;
            }
            return false;
        }

        /// <summary>
        ///     MAYBE TRUE FOR DELETE! FIELD MUST BE IN PREIMAGE FOR UPDATE STEP! Returns if the either of the fields value is
        ///     logically changing by inspecting the Target and Preimage
        /// </summary>
        public bool FieldChanging(params string[] fields)
        {
            foreach (var field in fields)
            {
                if (FieldChanging(field))
                    return true;
            }
            return false;
        }

        /// <summary>
        ///     FIELD MUST BE IN PREIMAGE! Returns the effective value of the field in the context record (gets from the target
        ///     entity or if not in gets from the preimage)
        /// </summary>
        public object GetField(string fieldName)
        {
            if (TargetEntity != null && TargetEntity.Contains(fieldName))
                return TargetEntity[fieldName];
            else if (MessageName == PluginMessage.Create)
                return null;
            else if (MessageName == PluginMessage.Update)
                return PreImageEntity.GetField(fieldName);
            else if (MessageName == PluginMessage.Delete)
                return null;
            else
                //not sure how to get status if a setstate message
                throw new InvalidPluginExecutionException("GetFieldMethod Not Implemented for plugin message " +
                                                          MessageName);
        }

        /// <summary>
        ///     FIELD MUST BE IN PREIMAGE! Returns the effective value of the field in the context record (gets from the target
        ///     entity or if not in gets from the preimage)
        /// </summary>
        public object GetFieldFromPreImage(string fieldName)
        {
            return PreImageEntity.GetField(fieldName);
        }

        /// <summary>
        ///     FIELD MUST BE IN PREIMAGE! Returns the effective value of the field in the context record (gets from the target
        ///     entity or if not in gets from the preimage)
        /// </summary>
        public IEnumerable<Entity> GetActivityParties(string fieldName)
        {
            if (TargetEntity.Contains(fieldName))
                return TargetEntity.GetActivityParties(fieldName);
            else if (!IsMessage(PluginMessage.Create))
            {
                var lookThisUp = XrmService.Retrieve(TargetType, TargetId, new[] { fieldName });
                PreImageEntity.SetField(fieldName, lookThisUp.GetField(fieldName));
                return PreImageEntity.GetActivityParties(fieldName);
            }
            return new Entity[0];
        }

        /// <summary>
        ///     NULL IF NOTHING!! FIELD MUST BE IN PREIMAGE! Returns the effective id value of the lookup field in the context
        ///     record (gets from the target entity or if not in gets from the preimage)
        /// </summary>
        public Guid? GetLookupGuid(string fieldName)
        {
            return XrmEntity.GetLookupGuid(GetField(fieldName));
        }

        public Guid? GetLookupGuidPreImage(string fieldName)
        {
            return XrmEntity.GetLookupGuid(GetFieldFromPreImage(fieldName));
        }

        public decimal GetDecimalValue(string fieldName)
        {
            return XrmEntity.GetDecimalValue(GetField(fieldName));
        }

        /// <summary>
        ///     EMPTY STRING IF NOTHING!! FIELD MUST BE IN PREIMAGE! Returns the effective entitytype  of the lookup field in the
        ///     context record (gets from the target entity or if not in gets from the preimage)
        /// </summary>
        public string GetLookupType(string fieldName)
        {
            return XrmEntity.GetLookupType(GetField(fieldName));
        }

        /// <summary>
        ///     -1 IF NOTHING!! FIELD MUST BE IN PREIMAGE FOR UPDATE STEP! Returns the effective int value of the optionset field
        ///     in the context record (gets from the target entity or if not in gets from the preimage)
        /// </summary>
        public string GetStringField(string fieldName)
        {
            return (string)GetField(fieldName);
        }

        /// <summary>
        ///     0 if null
        /// </summary>
        public int GetIntField(string fieldName)
        {
            return XrmEntity.GetInt(GetField(fieldName));
        }

        public DateTime? GetDateTimeField(string fieldName)
        {
            return (DateTime?)GetField(fieldName);
        }

        /// <summary>
        ///     -1 IF NOTHING!! FIELD MUST BE IN PREIMAGE FOR UPDATE STEP! Returns the effective int value of the optionset field
        ///     in the context record (gets from the target entity or if not in gets from the preimage)
        /// </summary>
        public int GetOptionSet(string fieldName)
        {
            return XrmEntity.GetOptionSetValue(GetField(fieldName));
        }

        /// <summary>
        ///     -1 IF NOTHING!! FIELD MUST BE IN PREIMAGE FOR UPDATE STEP! Returns the effective int value of the optionset field
        ///     in the context record (gets from the target entity or if not in gets from the preimage)
        /// </summary>
        public int GetOptionSetPreImage(string fieldName)
        {
            return PreImageEntity.GetOptionSetValue(fieldName);
        }

        /// <summary>
        ///     FALSE IF NOTHING!! FIELD MUST BE IN PREIMAGE FOR UPDATE STEP! Returns the effective value of the boolean field in
        ///     the context record (gets from the target entity or if not in gets from the preimage)
        /// </summary>
        public bool GetBoolean(string fieldName)
        {
            return XrmEntity.GetBoolean(GetField(fieldName));
        }

        /// <summary>
        ///     FALSE IF NOTHING!! FIELD MUST BE IN PREIMAGE FOR UPDATE STEP! Returns the effective value of the boolean field in
        ///     the context record (gets from the target entity or if not in gets from the preimage)
        /// </summary>
        public bool GetBooleanPreImage(string fieldName)
        {
            return PreImageEntity.GetBoolean(fieldName);
        }

        /// <summary>
        ///     0 IF NOTHING!! FIELD MUST BE IN PREIMAGE FOR UPDATE STEP! Returns the effective value of the boolean field in the
        ///     context record (gets from the target entity or if not in gets from the preimage)
        /// </summary>
        public decimal GetMoneyValue(string fieldName)
        {
            return XrmEntity.GetMoneyValue(GetField(fieldName));
        }

        /// <summary>
        ///     ONLY USE IN PRE PIPELINE!! Sets the field to the value in the target entity
        /// </summary>
        public void SetField(string fieldName, object value)
        {
            TargetEntity.SetField(fieldName, value);
        }

        /// <summary>
        ///     ONLY USE IN PRE PIPELINE!! Sets the field to the value in the target entity
        /// </summary>
        public void SetLookupField(string fieldName, Guid guid, string entityType)
        {
            TargetEntity.SetLookupField(fieldName, guid, entityType);
        }

        /// <summary>
        ///     ONLY USE IN PRE PIPELINE!! Sets the field to the value in the target entity
        /// </summary>
        public void SetLookupField(string fieldName, Entity entity)
        {
            TargetEntity.SetLookupField(fieldName, entity);
        }

        public void SetOptionSetField(string fieldName, int index)
        {
            XrmEntity.SetOptionSetField(TargetEntity, fieldName, index);
        }

        public void SetMoneyField(string fieldName, decimal amount)
        {
            TargetEntity.SetMoneyField(fieldName, amount);
        }


        public bool MeetsConditionChanging(string fieldName, ConditionOperator conditionOperator, object value)
        {
            var conditions = new[] { new ConditionExpression(fieldName, conditionOperator, value) };
            return MeetsConditionsChanging(conditions);
        }

        /// <summary>
        ///     NOT IMPLEMENTED FOR SET STATE
        /// </summary>
        public bool MeetsConditionsChanging(IEnumerable<ConditionExpression> conditions)
        {
            var metPrePlugin = false;
            var metPostPlugin = false;
            switch (MessageName)
            {
                case PluginMessage.Create:
                    {
                        metPostPlugin = XrmEntity.MeetsConditions(GetField, conditions);
                        break;
                    }
                case PluginMessage.Update:
                    {
                        metPrePlugin = XrmEntity.MeetsConditions(PreImageEntity.GetField, conditions);
                        metPostPlugin = XrmEntity.MeetsConditions(GetField, conditions);
                        break;
                    }
                case PluginMessage.Delete:
                    {
                        metPrePlugin = XrmEntity.MeetsConditions(PreImageEntity.GetField, conditions);
                        break;
                    }
                default:
                    {
                        //not sure what to do for setstate with status
                        throw new InvalidPluginExecutionException(
                            "MeetsConditionsChanging Not Implemented for plugin message " + MessageName);
                    }
            }
            return metPostPlugin != metPrePlugin;
        }

        /// <summary>
        ///     NOT IMPLEMENTED FOR SET STATE
        /// </summary>
        public bool MeetsConditions(IEnumerable<ConditionExpression> conditions)
        {
            if (IsMessage(PluginMessage.Create, PluginMessage.Update))
                return XrmEntity.MeetsConditions(GetField, conditions);
            throw new NotSupportedException("Method Not Implemented For PLugin Message: " + MessageName);
        }

        public bool OptionSetChangedTo(string fieldName, int value)
        {
            return FieldChanging(fieldName) && GetOptionSet(fieldName) == value;
        }

        public string GetFieldLabel(string fieldName)
        {
            return XrmService.GetFieldLabel(fieldName, TargetType);
        }

        public string GetEntityLabel()
        {
            return XrmService.GetEntityDisplayName(TargetType);
        }

        public string GetEntityCollectionLabel()
        {
            return XrmService.GetEntityCollectionName(TargetType);
        }

        public string GetOptionLabel(int value, string field)
        {
            return XrmService.GetOptionLabel(value, field, TargetType);
        }

        #endregion
    }
}