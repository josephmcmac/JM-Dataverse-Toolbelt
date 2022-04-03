﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using JosephM.Core.Constants;
using JosephM.Xrm.Schema;

namespace JosephM.Xrm
{
    /// <summary>
    ///     Class with re-usable generic methods on a Entity
    /// </summary>
    public static class XrmEntity
    {
        /// <summary>
        ///     If entity is not null and contains the field returns the field value else returns null
        /// </summary>
        public static object GetField(this Entity entity, string fieldName)
        {
            if (string.IsNullOrEmpty(fieldName))
                throw new ArgumentNullException("fieldName");
            if (entity != null && entity.Contains(fieldName))
            {
                return entity[fieldName];
            }
            else
                return null;
        }

        /// <summary>
        ///     If alias returns alias value - if entity is not null and contains the field returns the field value else returns
        ///     null
        /// </summary>
        public static object GetFieldValue(this Entity entity, string fieldName)
        {
            if (string.IsNullOrEmpty(fieldName))
                throw new ArgumentNullException("fieldName");
            if (entity != null && entity.Contains(fieldName))
            {
                var fieldObject = entity[fieldName];
                if (fieldObject is AliasedValue)
                    return ((AliasedValue)fieldObject).Value;
                else
                    return fieldObject;
            }
            else
                return null;
        }

        public static string GetStringField(this Entity entity, string fieldName)
        {
            return (string)GetField(entity, fieldName);
        }

        public static IEnumerable<string> GetFieldsInEntity(this Entity entity)
        {
            return entity.Attributes.Select(a => a.Key).ToArray();
        }

        /// <summary>
        ///     Returns a function which will call the GetFieldCrmTarget method for the input record
        /// </summary>
        public static Func<string, object> GetFieldDelegate(this Entity entity)
        {
            return entity.GetField;
        }

        /// <summary>
        ///     -1 if the entity is null, does not contain the field or the field is null
        /// </summary>
        public static int GetOptionSetValue(this Entity entity, string fieldName)
        {
            return GetOptionSetValue(GetField(entity, fieldName));
        }

        /// <summary>
        ///     -1 if fieldValue is null
        /// </summary>
        public static int GetOptionSetValue(object fieldValue)
        {
            if (fieldValue != null)
                return ((OptionSetValue)fieldValue).Value;
            else
                return -1;
        }

        /// <summary>
        ///     false if the entity is null, does not contain the field or the field is null
        /// </summary>
        public static bool GetBoolean(this Entity entity, string fieldName)
        {
            return GetBoolean(GetField(entity, fieldName));
        }

        /// <summary>
        ///     false if fieldValue is null
        /// </summary>
        public static bool GetBoolean(object fieldValue)
        {
            if (fieldValue != null)
                return (bool)fieldValue;
            else
                return false;
        }

        /// <summary>
        ///     null if the entity is null, does not contain the field or the field is null
        /// </summary>
        public static Guid? GetLookupGuid(this Entity entity, string fieldName)
        {
            return GetLookupGuid(GetField(entity, fieldName));
        }

        public static string GetLookupName(this Entity entity, string fieldName)
        {
            return GetLookupName(GetField(entity, fieldName));
        }


        /// <summary>
        ///     null if fieldValue is null
        /// </summary>
        public static Guid? GetLookupGuid(object fieldValue)
        {
            if (fieldValue != null)
                return ((EntityReference)fieldValue).Id;
            else
                return null;
        }

        /// <summary>
        ///     null if fieldValue is null
        /// </summary>
        public static Guid GetGuidField(this Entity entity, string fieldName)
        {
            var fieldValue = GetField(entity, fieldName);
            if (fieldValue != null)
                return (Guid)fieldValue;
            else
                return Guid.Empty;
        }

        public static string GetLookupName(object fieldValue)
        {
            if (fieldValue != null)
                return ((EntityReference)fieldValue).Name;
            else
                return "";
        }

        /// <summary>
        ///     empty string if the entity is null, does not contain the field or the field is null
        /// </summary>
        public static string GetLookupType(this Entity entity, string fieldName)
        {
            try
            {
                return GetLookupType(GetField(entity, fieldName));
            }
            catch (Exception ex)
            {
                throw new Exception($"Unexpected Error Getting Lookup Type For Field {fieldName} in type {entity?.LogicalName}", ex);
            }
        }

        /// <summary>
        ///     empty string if fieldValue is null
        /// </summary>
        public static string GetLookupType(object fieldValue)
        {
            if (fieldValue != null)
            {
                return ((EntityReference)fieldValue).LogicalName;
            }
            else
                return "";
        }

        /// <summary>
        ///     0 if the entity is null, does not contain the field or the field is null
        /// </summary>
        public static decimal GetDecimalValue(this Entity entity, string fieldName)
        {
            return GetDecimalValue(GetField(entity, fieldName));
        }

        /// <summary>
        ///     0 if fieldValue is null
        /// </summary>
        public static decimal GetDecimalValue(object fieldValue)
        {
            if (fieldValue != null)
                return (Decimal)fieldValue;
            else
                return new Decimal(0);
        }

        /// <summary>
        ///     0 if the entity is null, does not contain the field or the field is null
        /// </summary>
        public static double GetDoubleValue(this Entity entity, string fieldName)
        {
            return GetDoubleValue(GetField(entity, fieldName));
        }

        /// <summary>
        ///     0 if fieldValue is null
        /// </summary>
        public static double GetDoubleValue(object fieldValue)
        {
            if (fieldValue != null)
                return (double)fieldValue;
            else
                return (double)0;
        }

        /// <summary>
        ///     0 if the entity is null, does not contain the field or the field is null
        /// </summary>
        public static decimal? GetMoneyValue(this Entity entity, string fieldName)
        {
            return GetMoneyValue(GetField(entity, fieldName));
        }

        /// <summary>
        ///     0 if the fieldValue is null
        /// </summary>
        public static decimal GetMoneyValue(object fieldValue)
        {
            if (fieldValue is decimal)
                return (decimal)fieldValue;
            if (fieldValue is Money)
                return ((Money)fieldValue).Value;
            return new Decimal(0);
        }

        /// <summary>
        ///     0 if the fieldValue is null
        /// </summary>
        public static int GetInt(this Entity entity, string fieldName)
        {
            return GetInt(GetField(entity, fieldName));
        }

        /// <summary>
        ///     0 if the fieldValue is null
        /// </summary>
        public static int GetInt(object fieldValue)
        {
            if (fieldValue != null)
                return ((int)fieldValue);
            else
                return 0;
        }

        public static string GetFieldAsDisplayString(object fieldValue)
        {
            string displayString;
            if (fieldValue == null)
                displayString = "";
            else if (fieldValue is decimal)
            {
                const string format = StringFormats.DecimalFormat;
                displayString = (decimal.Parse(fieldValue.ToString())).ToString(format);
            }
            else if (fieldValue is Money)
            {
                var amount = GetMoneyValue(fieldValue);
                displayString = amount.ToString(StringFormats.MoneyFormat);
            }
            else
                displayString = fieldValue.ToString();

            return displayString;
        }

        public static string GetFieldAsDisplayString(Entity entity, string fieldName)
        {
            return GetFieldAsDisplayString(GetField(entity, fieldName));
        }

        /// <summary>
        ///     Sets the field in the entity
        /// </summary>
        public static void SetField(this Entity entity, string fieldName, object value)
        {
            entity[fieldName] = value;
        }

        /// <summary>
        ///     Parses and sets the field in the entity
        /// </summary>
        public static void SetField(this Entity entity, string fieldName, object value, XrmService service)
        {
            entity[fieldName] = service.ParseField(fieldName, entity.LogicalName, value);
        }

        /// <summary>
        ///     Sets the field in the entity to an option set of the given index
        /// </summary>
        public static void SetOptionSetField(Entity entity, string fieldName, int value)
        {
            SetField(entity, fieldName, new OptionSetValue(value));
        }

        /// <summary>
        ///     Sets the field in the entity to an entity reference of the input type and id
        /// </summary>
        public static void SetLookupField(this Entity entity, string fieldName, Guid guid, string entityType)
        {
            SetField(entity, fieldName, new EntityReference(entityType, guid));
        }

        /// <summary>
        ///     Sets the field in the entity to an entity reference to the input record
        /// </summary>
        public static void SetLookupField(this Entity entity, string fieldName, Entity lookupTo)
        {
            SetLookupField(entity, fieldName, lookupTo.Id, lookupTo.LogicalName);
        }

        /// <summary>
        ///     Sets the money field to the input value
        /// </summary>
        public static void SetMoneyField(this Entity entity, string fieldName, Decimal value)
        {
            SetField(entity, fieldName, new Money(value));
        }

        /// <summary>
        ///     Returns if two fields are logically equal. Note an int may be equal to an option set
        /// </summary>
        public static bool FieldsEqual(object field1, object field2)
        {
            if (field1 == null && field2 == null)
            {
                return true;
            }
            else if (field1 == null || field2 == null)
            {
                if (field1 is string || field2 is string)
                {
                    return string.IsNullOrEmpty((string)field1) && String.IsNullOrEmpty((string)field2);
                }
                else
                {
                    return false;
                }
            }
            else if (field1 is EntityReference er1 && field2 is EntityReference er2)
            {
                return er1.Id.Equals(er2.Id) &&
                       er1.LogicalName.Equals(er2.LogicalName);
            }
            else if (field1 is DateTime dt1 && field2 is DateTime dt2)
            {
                if (dt1.Kind == DateTimeKind.Utc && (dt2.Kind == DateTimeKind.Local || dt2.Kind == DateTimeKind.Unspecified))
                {
                    dt2 = dt2.ToUniversalTime();
                }
                if ((dt1.Kind == DateTimeKind.Local || dt1.Kind == DateTimeKind.Unspecified) && dt2.Kind == DateTimeKind.Utc)
                {
                    dt1 = dt1.ToUniversalTime();
                }
                return dt1.Equals(dt2);
            }
            else if (field1 is OptionSetValue osv1)
            {
                if (field2 is OptionSetValue osv2)
                {
                    return osv1.Value.Equals(osv2.Value);
                }
                else if (field2 is int osvInt2)
                {
                    return osv1.Value.Equals(osvInt2);
                }
                else
                {
                    throw new InvalidPluginExecutionException($"Mismatched Types {field1.GetType().Name} & {field2.GetType().Name}");
                }
            }
            else if (field1 is int int1)
            {
                if (field2 is int int2)
                {
                    return int1.Equals(int2);
                }
                else if (field2 is OptionSetValue intOsv2)
                {
                    return int1.Equals(intOsv2.Value);
                }
                else
                {
                    throw new InvalidPluginExecutionException($"Mismatched Types {field1.GetType().Name} & {field2.GetType().Name}");
                }
            }
            else if (field1 is bool b1 && field2 is bool b2)
            {
                return b1.Equals(b2);
            }
            else if (field1 is Guid g1 && field2 is Guid g2)
            {
                return g1.Equals(g2);
            }
            else if (field1 is double db1 && field2 is double db2)
            {
                return db1.Equals(db2);
            }
            else if (field1 is string s1 && field2 is string s2)
            {
                return s1.Equals(s2);
            }
            else if (field1 is Money m1 && field2 is Money m2)
            {
                return m1.Equals(m2);
            }
            else if (field1 is decimal dc1 && field2 is decimal dc2)
            {
                return dc1.Equals(dc2);
            }
            else if (field1 is Entity[] || field1 is EntityCollection)
            {
                return EntityListFieldEqual(field1, field2);
            }
            else if (field1 is OptionSetValueCollection osvc1 && field1 is OptionSetValueCollection osvc2)
            {
                var ints1 = osvc1.Select(x => x.Value).OrderBy(i => i).ToArray();
                var ints2 = osvc2.Select(x => x.Value).OrderBy(i => i).ToArray();
                return ints1.SequenceEqual(ints2);
            }
            else
            {
                throw new InvalidPluginExecutionException($"Not implemented for types {field1.GetType().Name} & {field2.GetType().Name}");
            }
        }

        private static bool EntityListFieldEqual(object field1, object field2)
        {
            Entity[] fieldList1 = new Entity[0];
            Entity[] fieldList2 = new Entity[0];
            if (field1 is EntityCollection ec1)
                fieldList1 = ec1.Entities.ToArray();
            else if (field1 is Entity[])
                fieldList1 = (Entity[])field1;
            if (field2 is EntityCollection ec2)
                fieldList2 = ec2.Entities.ToArray();
            else if (field2 is Entity[])
                fieldList2 = (Entity[])field2;

            if (!fieldList1.Any() && !fieldList2.Any())
                return true;
            else if (!fieldList1.Any() || !fieldList2.Any())
                return false;
            else if (fieldList1.Count() != fieldList2.Count())
                return false;

            var entityType = fieldList1.First().LogicalName;
            if (entityType != Entities.activityparty)
                throw new NotImplementedException($"EntityListFieldEqual not implemented for entity type {entityType}");

            var fieldToMatch = new[] { Fields.activityparty_.partyid, Fields.activityparty_.addressused };

            foreach (var entity in fieldList1)
            {
                var matchingIn1 = fieldList1.Where(e => fieldToMatch.All(f => FieldsEqual(e.GetField(f), entity.GetField(f)))).ToArray();
                var matchingIn2 = fieldList2.Where(e => fieldToMatch.All(f => FieldsEqual(e.GetField(f), entity.GetField(f)))).ToArray();
                if (matchingIn1.Count() != matchingIn2.Count())
                    return false;
            }
            return true;
        }

        /// <summary>
        ///     Returns if the field is logically equal in the two input entities matching
        /// </summary>
        public static bool FieldsEqualCrmTarget(Entity entity1, Entity entity2, string fieldName)
        {
            var field1 = entity1.GetField(fieldName);
            var field2 = entity2.GetField(fieldName);
            return FieldsEqual(field1, field2);
        }

        /// <summary>
        ///     Returns the primary key field name of the input entity type
        /// </summary>
        public static string GetPrimaryKeyName(string entityType)
        {
            if (entityType == "phonecall" || entityType == "appointment")
                return "activityid";
            return entityType + "id";
        }

        /// <summary>
        ///     Returns the sum of an array of nullable decimal objects. Will return null if no Decimal values are found in the
        ///     array
        /// </summary>
        public static object SumDecimalFields(object[] fields)
        {
            object result = null;
            foreach (var item in fields)
            {
                if (item != null)
                {
                    if (result != null)
                        result = Decimal.Add((Decimal)item, (Decimal)result);
                    else
                        result = (Decimal)item;
                }
            }
            return result;
        }

        public static object SumFields(object[] fieldValues)
        {
            object result = null;
            foreach (var fieldValue in fieldValues)
            {
                if (result == null)
                    result = fieldValue;
                else if (fieldValue != null)
                {
                    //need to add the the result we are carrying based on the type
                    if (fieldValue is Decimal)
                        result = Decimal.Add((Decimal)result, (Decimal)fieldValue);
                    else if (fieldValue is Double)
                        result = (Double)result + (Double)fieldValue;
                    else if (fieldValue is Money)
                        result = new Money(((Money)result).Value + ((Money)fieldValue).Value);
                    else
                        throw new InvalidPluginExecutionException("SumField function not implemented for field type " +
                                                                  result.GetType());
                }
            }
            return result;
        }

        /// <summary>
        ///     Returns the sum of a field in an array of entities. Will return null if no non-null values are found in the array
        /// </summary>
        public static object SumField(string fieldName, IEnumerable<Entity> recordsToSum)
        {
            object result = null;
            foreach (var item in recordsToSum)
            {
                if (item != null && item.Contains(fieldName))
                {
                    var temp = item.GetField(fieldName);
                    if (result == null)
                        result = temp;
                    else if (temp != null)
                    {
                        //need to add the the result we are carrying based on the type
                        if (temp is Decimal)
                            result = Decimal.Add((Decimal)result, (Decimal)temp);
                        else if (temp is Double)
                            result = (Double)result + (Double)temp;
                        else if (temp is Money)
                            result = new Money(((Money)result).Value + ((Money)temp).Value);
                        else
                            throw new InvalidPluginExecutionException(
                                "SumField function not implemented for field type " + result.GetType());
                    }
                }
            }
            return result;
        }

        /// <summary>
        ///     Removes the given field from then entity if it exists
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="field"></param>
        public static void RemoveField(Entity entity, string field)
        {
            if (entity.Contains(field))
                entity.Attributes.Remove(field);
        }

        /// <summary>
        ///     Warning!! Creates a new Entity object containing all fields with the same value from the input entity. Currently
        ///     keeps field object references and doesn't replicate state/status
        /// </summary>
        public static Entity ReplicateToNewEntity(Entity entity)
        {
            var result = new Entity(entity.LogicalName);
            foreach (var field in entity.Attributes)
            {
                if (field.Key != "statuscode" && field.Key != "statecode" &&
                    field.Key != GetPrimaryKeyName(entity.LogicalName))
                    SetField(result, field.Key, field.Value);
            }
            return result;
        }

        /// <summary>
        ///     Returns true if all conditions resolve to true with the value returned by the getFieldDelegate function
        /// </summary>
        public static bool MeetsConditions(Func<string, object> getFieldDelegate, IEnumerable<ConditionExpression> conditions)
        {
            var result = true;
            foreach (var condition in conditions)
            {
                var fieldValue = getFieldDelegate(condition.AttributeName);
                if (!MeetsCondition(fieldValue, condition))
                {
                    result = false;
                    break;
                }
            }
            return result;
        }

        /// <summary>
        ///     Returns true if the condition resolves to true for the fieldValue
        /// </summary>
        public static bool MeetsCondition(object fieldValue, ConditionExpression condition)
        {
            if (condition.Operator == ConditionOperator.Equal && condition.Values.Count == 1)
                return FieldsEqual(fieldValue, condition.Values[0]);
            if (condition.Operator == ConditionOperator.NotEqual && condition.Values.Count == 1)
                return !FieldsEqual(fieldValue, condition.Values[0]);
            else
                throw new InvalidPluginExecutionException("MeetsCondition not implemented for condition type");
        }

        /// <summary>
        ///     If the input condtions include a condition for the statecode field returns ite nothing
        /// </summary>
        public static ConditionExpression GetStateCondition(ConditionExpression[] conditions)
        {
            ConditionExpression result = null;
            foreach (var condition in conditions)
            {
                if (condition.AttributeName == "statecode")
                    result = condition;
            }
            return result;
        }

        /// <summary>
        ///     Converts an array of Entities to an early bound array of T Entities
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entities"></param>
        /// <returns></returns>
        public static T[] ConvertEntitiesTo<T>(Entity[] entities) where T : Entity
        {
            var convertedEntities = new T[entities.Length];

            for (var i = 0; i < entities.Length; i++)
            {
                convertedEntities[i] = entities[i].ToEntity<T>();
            }
            return convertedEntities;
        }

        public static EntityReference CreateLookup(Entity entity)
        {
            return new EntityReference(entity.LogicalName, entity.Id);
        }

        public static EntityReference CreateLookup(string entityType, Guid id)
        {
            return new EntityReference(entityType, id);
        }

        public static OptionSetValue CreateOptionSet(int value)
        {
            return new OptionSetValue(value);
        }

        /// <summary>
        ///     return field names where newitem contains the field and it is not (crm) logically equal to the field in
        ///     existingItem
        /// </summary>
        public static string[] GetChangingFields(Entity newItem, Entity existingItem)
        {
            return newItem.Attributes.Keys
                .Where(
                    field => newItem.Contains(field) &&
                             !FieldsEqual(newItem.GetField(field), existingItem.GetField(field))).ToArray();
        }

        public static DateTime? GetDateTimeField(this Entity updatedEntity, string fieldName)
        {
            return (DateTime?)GetField(updatedEntity, fieldName);
        }

        public static Entity New(string entityType, Guid id)
        {
            return new Entity(entityType) { Id = id };
        }

        public static object CreateMoney(decimal value)
        {
            return new Money(value);
        }

        /// <summary>
        ///     If entity is not null and contains the field returns the field value else returns null
        /// </summary>
        public static void ClearFields(this Entity entity)
        {
            entity.Attributes.Clear();
        }

        public static IEnumerable<Entity> GetActivityParties(this Entity entity, string fieldName)
        {
            var fieldValue = GetField(entity, fieldName);
            if (fieldValue == null)
            {
                return new Entity[] { };
            }
            else if (fieldValue is EntityCollection ec)
            {
                return ec.Entities;
            }
            else if (fieldValue is Entity[] ar)
            {
                return ar;
            }
            else if (fieldValue is IEnumerable<Entity> en)
            {
                return en;
            }
            else
            {
                return new Entity[] { };
            }
        }

        public static void AddFromParty(this Entity entity, string type, Guid id)
        {
            entity.AddActivityParty("from", type, id);
        }

        public static void AddToParty(this Entity entity, string type, Guid id)
        {
            entity.AddActivityParty("to", type, id);
        }

        public static void AddActivityParty(this Entity entity, string fieldName, string emailAddress)
        {
            var parties = new List<Entity>();
            var currentParties = entity.GetField(fieldName);
            if (currentParties != null)
            {
                if (currentParties is Entity[])
                    parties.AddRange((Entity[])currentParties);
                else if (currentParties is EntityCollection)
                    parties.AddRange(((EntityCollection)currentParties).Entities);
                else throw new Exception("No Add Logic Implemented For Type " + currentParties.GetType().Name);
            }
            var partyEntity = new Entity(Entities.activityparty);
            partyEntity.SetField(Fields.activityparty_.addressused, emailAddress);
            parties.Add(partyEntity);
            entity.SetField(fieldName, parties.ToArray());
        }

        public static void AddActivityParty(this Entity entity, string fieldName, string type, Guid id)
        {
            var parties = new List<Entity>();
            var currentParties = entity.GetField(fieldName);
            if (currentParties != null)
            {
                if (currentParties is Entity[])
                    parties.AddRange((Entity[])currentParties);
                else if (currentParties is EntityCollection)
                    parties.AddRange(((EntityCollection)currentParties).Entities);
                else throw new Exception("No Add Logic Implemented For Type " + currentParties.GetType().Name);
            }
            var entityReference = new EntityReference(type, id);
            var partyEntity = CreatePartyEntity(entityReference);
            parties.Add(partyEntity);
            entity.SetField(fieldName, parties.ToArray());
        }

        public static Entity CreatePartyEntity(EntityReference entityReference)
        {
            var partyEntity = new Entity(Entities.activityparty);
            partyEntity.SetField(Fields.activityparty_.partyid, entityReference);
            return partyEntity;
        }

        public static IEnumerable<EntityReference> GetActivityPartyReferences(this Entity entity, string fieldName)
        {
            var parties = entity.GetActivityParties(fieldName);
            return parties.Select(p => (EntityReference)p.GetField(Fields.activityparty_.partyid));
        }

        /// <summary>
        ///     If entity is not null and contains the field returns the field value else returns null
        /// </summary>
        public static void RemoveFields(this Entity entity, IEnumerable<string> fieldNames)
        {
            if (fieldNames != null)
            {
                foreach (var field in fieldNames)
                {
                    if (entity.Contains(field))
                        entity.Attributes.Remove(field);
                }
            }
        }
    }
}