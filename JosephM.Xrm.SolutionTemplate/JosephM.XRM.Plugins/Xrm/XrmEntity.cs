using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using Schema;

namespace $safeprojectname$.Xrm
{
    /// <summary>
    /// Class with static and extension methods to simplify operations on an Entity object
    /// </summary>
    public static class XrmEntity
    {
        /// <summary>
        /// Gets the value of the field in an entity. If not populated in the entity returns null, if an alaissed value unboxes it
        /// </summary>
        /// <param name="entity">Entity object containing the field</param>
        /// <param name="fieldName">Name of the field</param>
        /// <returns>The value of the field in an entity</returns>
        public static object GetField(this Entity entity, string fieldName)
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

        /// <summary>
        /// Gets the value of a string field cast as a string
        /// </summary>
        /// <param name="entity">Entity object containing the field</param>
        /// <param name="stringFieldName">Name of the string field</param>
        /// <returns>The field value cast as string</returns>
        public static string GetStringField(this Entity entity, string fieldName)
        {
            return (string)GetField(entity, fieldName);
        }

        /// <summary>
        ///  Gets the value of an option set field in the entity. If null returns -1
        /// </summary>
        /// <param name="entity">Entity object containing the field</param>
        /// <param name="optionSetFieldName">Name of the option set field</param>
        /// <returns>The picklist option key. If null -1</returns>
        public static int GetOptionSetValue(this Entity entity, string optionSetFieldName)
        {
            return GetOptionSetValue(GetField(entity, optionSetFieldName));
        }

        /// <summary>
        /// Unboxes the picklist value of a option set field. If null returns -1
        /// </summary>
        /// <param name="fieldValue">The option set field value</param>
        /// <returns>The picklist option key. If null -1</returns>
        public static int GetOptionSetValue(object fieldValue)
        {
            if (fieldValue != null)
                return ((OptionSetValue)fieldValue).Value;
            else
                return -1;
        }

        /// <summary>
        /// Returns the field value as a boolean. False is returned if not populated
        /// </summary>
        /// <param name="entity">Entity object containing the field</param>
        /// <param name="booleanFieldName">Name of the boolean field<</param>
        /// <returns>The field value as a boolean. False is returned if not populated</returns>
        public static bool GetBoolean(this Entity entity, string booleanFieldName)
        {
            return GetBoolean(GetField(entity, booleanFieldName));
        }

        /// <summary>
        /// Returns the field value as a boolean. False is returned if not populated
        /// </summary>
        /// <param name="fieldValue">Value of the boolean field</param>
        /// <returns>The field value as a boolean. False is returned if not populated</returns>
        public static bool GetBoolean(object fieldValue)
        {
            if (fieldValue != null)
                return (bool)fieldValue;
            else
                return false;
        }

        /// <summary>
        /// Get the id in a lookup field value if it is populated otherwise null
        /// </summary>
        /// <param name="entity">Entity object containing the field</param>
        /// <param name="lookupFieldName">Name of the lookup field<</param>
        /// <returns>The id in a lookup field value if it is populated otherwise null</returns>
        public static Guid? GetLookupGuid(this Entity entity, string fieldName)
        {
            return GetLookupGuid(GetField(entity, fieldName));
        }

        /// <summary>
        /// Get the name populated in a lookup field value if it is populated otherwise null
        /// </summary>
        /// <param name="entity">Entity object containing the field</param>
        /// <param name="lookupFieldName">Name of the lookup field<</param>
        /// <returns>The name in lookup field value if it is populated otherwise null</returns>
        public static string GetLookupName(this Entity entity, string lookupFieldName)
        {
            return GetLookupName(GetField(entity, lookupFieldName));
        }


        /// <summary>
        /// Get the target tyoe of a lookup field value if it is populated otherwise null
        /// </summary>
        /// <param name="fieldValue">Value of the lookup field</param>
        /// <returns>The id of a lookup field value if it is populated otherwise null</returns>
        public static Guid? GetLookupGuid(object fieldValue)
        {
            if (fieldValue != null)
                return ((EntityReference)fieldValue).Id;
            else
                return null;
        }

        /// <summary>
        /// Get the target tyoe of a lookup field value if it is populated otherwise null
        /// </summary>
        /// <param name="fieldValue">Value of the lookup field</param>
        /// <returns>The target tyoe of a lookup field value if it is populated otherwise null</returns>
        public static string GetLookupName(object fieldValue)
        {
            if (fieldValue != null)
                return ((EntityReference)fieldValue).Name;
            else
                return "";
        }

        /// <summary>
        /// Get the target tyoe of a lookup field value if it is populated otherwise null
        /// </summary>
        /// <param name="entity">Entity object containing the field</param>
        /// <param name="lookupFieldName">Name of the lookup field<</param>
        /// <returns>The target tyoe of a lookup field value if it is populated otherwise null</returns>
        public static string GetLookupType(this Entity entity, string lookupFieldName)
        {
            return GetLookupType(GetField(entity, lookupFieldName));
        }

        /// <summary>
        /// Get the target tyoe of a lookup field value if it is populated otherwise null
        /// </summary>
        /// <param name="fieldValue">Value of the lookup field</param>
        /// <returns>The target tyoe of a lookup field value if it is populated otherwise null</returns>
        public static string GetLookupType(object fieldValue)
        {
            if (fieldValue != null)
                return ((EntityReference)fieldValue).LogicalName;
            else
                return "";
        }

        /// <summary>
        /// Gets the value of a decimal field in the entity. If null return zero
        /// </summary>
        /// <param name="entity">Entity object containing the field</param>
        /// <param name="decimalFieldName">Name of the decimal field</param>
        /// <returns>The field value as decimal</returns>
        public static decimal GetDecimalValue(this Entity entity, string decimalFieldName)
        {
            return GetDecimalValue(GetField(entity, decimalFieldName));
        }

        /// <summary>
        /// Returns a field value as decimal. If null return decimal zero<
        /// </summary>
        /// <param name="fieldValue">Value of the decimal field</param>
        /// <returns>fieldValue as decimal. If null return decimal zero</returns>
        public static decimal GetDecimalValue(object fieldValue)
        {
            if (fieldValue != null)
                return (Decimal)fieldValue;
            else
                return new Decimal(0);
        }

        /// <summary>
        ///  Gets the value of a money field in the entity. If null return zero
        /// </summary>
        /// <param name="entity">Entity object containing the field</param>
        /// <param name="moneyFieldName">Name of the money field</param>
        /// <returns>The money value as decimal</returns>
        public static decimal GetMoneyValue(this Entity entity, string moneyFieldName)
        {
            return GetMoneyValue(GetField(entity, moneyFieldName));
        }

        /// <summary>
        /// Unboxes the decimal value of a money field. If null returns decimal zero
        /// </summary>
        /// <param name="fieldValue">The money field value</param>
        /// <returns>Decimal value of a money field. If null returns decimal zero</returns>
        public static decimal GetMoneyValue(object fieldValue)
        {
            if (fieldValue != null)
                return ((Money)fieldValue).Value;
            else
                return new Decimal(0);
        }

        /// <summary>
        ///  Gets the value of a int field in the entity. If null return zero
        /// </summary>
        /// <param name="entity">Entity object containing the field</param>
        /// <param name="intFieldName">Name of the int field</param>
        /// <returns>The field value as integer</returns>
        public static int GetInt(this Entity entity, string intFieldName)
        {
            return GetInt(GetField(entity, intFieldName));
        }

        /// <summary>
        /// Returns a field value as int. If null return int zero<
        /// </summary>
        /// <param name="fieldValue">Value of the int field</param>
        /// <returns>fieldValue as int. If null return int zero</returns>
        public static int GetInt(object fieldValue)
        {
            if (fieldValue != null)
                return ((int)fieldValue);
            else
                return 0;
        }


        /// <summary>
        /// Sets a field value in the entity
        /// </summary>
        /// <param name="entity">Entity object to set the field value in</param>
        /// <param name="fieldName">Name of the field</param>
        /// <param name="value">Value to set the field to</param>
        public static void SetField(this Entity entity, string fieldName, object value)
        {
            entity[fieldName] = value;
        }

        /// <summary>
        ///  Sets a option set field to value for the index
        /// </summary>
        /// <param name="entity">Entity object to set the field value in</param>
        /// <param name="fieldName">Name of the lookup field</param>
        /// <param name="value">Index of the option set value</param>
        public static void SetOptionSetField(this Entity entity, string fieldName, int value)
        {
            SetField(entity, fieldName, new OptionSetValue(value));
        }

        /// <summary>
        /// Sets a lookup field to a entity reference value for the type and id
        /// </summary>
        /// <param name="entity">Entity object to set the field value in</param>
        /// <param name="fieldName">Name of the lookup field</param>
        /// <param name="guid">Id of the entity to reference</param>
        /// <param name="entityType">Type of the entity to reference</param>
        public static void SetLookupField(this Entity entity, string fieldName, Guid guid, string entityType)
        {
            SetField(entity, fieldName, new EntityReference(entityType, guid));
        }

        /// <summary>
        /// Sets a lookup field to a entity reference value for the entity
        /// </summary>
        /// <param name="entity">Entity object to set the field value in</param>
        /// <param name="fieldName">Name of the lookup field</param>
        /// <param name="lookupTo">Entity to reference</param>
        public static void SetLookupField(this Entity entity, string fieldName, Entity lookupTo)
        {
            if (lookupTo == null)
                SetField(entity, fieldName, null);
            else
                SetLookupField(entity, fieldName, lookupTo.Id, lookupTo.LogicalName);
        }

        /// <summary>
        /// Sets a money field to a money value for the decimal amount
        /// </summary>
        /// <param name="entity">Entity object to set the field value in</param>
        /// <param name="fieldName">Name of the money field</param>
        /// <param name="value">Decimal amount of the money value</param>
        public static void SetMoneyField(this Entity entity, string fieldName, Decimal value)
        {
            SetField(entity, fieldName, new Money(value));
        }

        /// <summary>
        /// Returns if two fields are logically equal. Note an int may be equal to an option set
        /// </summary>
        /// <param name="field1">One value to compare</param>
        /// <param name="field2">The other value to compare</param>
        /// <returns>True if the values are logically equal</returns>
        public static bool FieldsEqual(object field1, object field2)
        {
            if (field1 == null && field2 == null)
                return true;
            if (field1 == null || field2 == null)
            {
                if (field1 is string || field2 is string)
                    return String.IsNullOrEmpty((string)field1) && String.IsNullOrEmpty((string)field2);
                else
                    return false;
            }
            if (field1 is EntityReference er1)
            {
                if (field2 is EntityReference er2)
                {
                    return er1.Id.Equals(er2.Id) &&
                        er1.LogicalName.Equals(er2.LogicalName);
                }
                if (field2 is Guid erg2)
                {
                    return er1.Id.Equals(erg2);
                }
            }
            if (field1 is DateTime dt1 && field2 is DateTime dt2)
            {
                return (dt1).Equals(dt2);
            }
            if (field1 is OptionSetValue osv1)
            {
                if (field2 is OptionSetValue osv2)
                    return osv1.Value.Equals(osv2.Value);
                if (field2 is int i2)
                    return osv1.Value.Equals(i2);
                throw new InvalidPluginExecutionException("Mismatched Types");
            }
            if (field1 is int i1)
            {
                if (field2 is int i2)
                    return i1.Equals(i2);
                if (field2 is OptionSetValue osv2)
                    return i1.Equals(osv2.Value);
                throw new InvalidPluginExecutionException("Mismatched Types");
            }
            if (field1 is bool b1 && field2 is bool b2)
            {
                return b1.Equals(b2);
            }
            if (field1 is Guid g1 && field2 is Guid g2)
            {
                return g1.Equals(g2);
            }
            if (field1 is Double d1 && field2 is Double d2)
            {
                return d1.Equals(d2);
            }
            if (field1 is string st1 && field2 is string st2)
            {
                return st1.Equals(st2);
            }
            if (field1 is Money m1 && field2 is Money m2)
            {
                return m1.Value.Equals(m2.Value);
            }
            if (field1 is Decimal dc1 && field2 is Decimal dc2)
            {
                return dc1.Equals(dc2);
            }
            throw new InvalidPluginExecutionException(
                        string.Concat("FieldsEqualCrmTarget type not implemented for types ", field1.GetType(), " and ",
                            field2.GetType()));
        }

        /// <summary>
        /// Returns the sum of an array of field values. Will return null if no non-null values are found in the array
        /// </summary>
        /// <param name="fieldValues">Field values to sum</param>
        /// <returns>The sum of the field values with a matching type, otherwise null if no non null vlaues are found</returns>
        public static object SumFields(IEnumerable<object> fieldValues)
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
                    else if (fieldValue is int)
                        result = (int)result + (int)fieldValue;
                    else
                        throw new InvalidPluginExecutionException("SumField function not implemented for field type " +
                                                                  fieldValue.GetType());
                }
            }
            return result;
        }

        /// <summary>
        /// Returns the sum of a field in an array of entities. Will return null if no non-null values are found in the array
        /// </summary>
        /// <param name="fieldName">Name of the field to sum</param>
        /// <param name="recordsToSum">Entity records containing the field to sum</param>
        /// <returns>The sum of the field values with a matching type, otherwise null if no non null vlaues are found</returns>
        public static object SumField(string fieldName, IEnumerable<Entity> recordsToSum)
        {
            object result = null;
            foreach (var item in recordsToSum)
            {
                if (item != null && item.Contains(fieldName))
                {
                    var temp = GetField(item, fieldName);
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
                        else if (temp is int)
                            result = (int)result + (int)temp;
                        else
                            throw new InvalidPluginExecutionException(
                                "SumField function not implemented for field type " + temp.GetType());
                    }
                }
            }
            return result;
        }

        /// <summary>
        ///  Returns true if all conditions resolve to true for the field values returned by the getFieldDelegate function
        /// </summary>
        /// <param name="getFieldDelegate">Func to get a field value</param>
        /// <param name="conditions">Conditions to resolve</param>
        /// <returns>True if all conditions are met</returns>
        public static bool MeetsConditions(Func<string, object> getFieldDelegate,
            IEnumerable<ConditionExpression> conditions)
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
        ///  Returns true if the condition resolves to true for the field value
        /// </summary>
        /// <param name="fieldValue">Value of the field</param>
        /// <param name="condition">The condition to check</param>
        /// <returns>True of the condition is true for the field value</returns>
        public static bool MeetsCondition(object fieldValue, ConditionExpression condition)
        {
            switch (condition.Operator)
            {
                case ConditionOperator.Equal:
                    {
                        if (condition.Values.Count == 1)
                            return FieldsEqual(fieldValue, condition.Values[0]);
                        throw new InvalidPluginExecutionException(
                            "Error expected one value in the condition values for operator " + condition.Operator);
                    }
                case ConditionOperator.NotEqual:
                    {
                        if (condition.Values.Count == 1)
                            return !FieldsEqual(fieldValue, condition.Values[0]);
                        throw new InvalidPluginExecutionException(
                            "Error expected one value in the condition values for operator " + condition.Operator);
                    }
                case ConditionOperator.Null:
                    {
                        return FieldsEqual(fieldValue, null);
                    }
                case ConditionOperator.NotNull:
                    {
                        return !FieldsEqual(fieldValue, null);
                    }
                case ConditionOperator.LessThan:
                    {
                        if (condition.Values.Count == 1)
                        {
                            var compareValue = condition.Values[0];
                            if (fieldValue == null)
                                return false;
                            if (fieldValue is IComparable)
                                return ((IComparable)fieldValue).CompareTo(compareValue) < 0;
                            throw new InvalidPluginExecutionException(
                                string.Format("Type {0} not implemented for condition operator {1}",
                                    fieldValue.GetType().Name, condition.Operator));
                        }
                        throw new InvalidPluginExecutionException(
                            "Error expected one value in the condition values for operator " + condition.Operator);
                    }
                case ConditionOperator.LessEqual:
                    {
                        if (condition.Values.Count == 1)
                        {
                            var compareValue = condition.Values[0];
                            if (fieldValue == null)
                                return false;
                            if (fieldValue is IComparable)
                                return ((IComparable)fieldValue).CompareTo(compareValue) <= 0;
                            throw new InvalidPluginExecutionException(
                                string.Format("Type {0} not implemented for condition operator {1}",
                                    fieldValue.GetType().Name, condition.Operator));
                        }
                        throw new InvalidPluginExecutionException(
                            "Error expected one value in the condition values for operator " + condition.Operator);
                    }
                case ConditionOperator.GreaterThan:
                    {
                        if (condition.Values.Count == 1)
                        {
                            var compareValue = condition.Values[0];
                            if (fieldValue == null)
                                return false;
                            if (fieldValue is IComparable)
                                return ((IComparable)fieldValue).CompareTo(compareValue) > 0;
                            throw new InvalidPluginExecutionException(
                                string.Format("Type {0} not implemented for condition operator {1}",
                                    fieldValue.GetType().Name, condition.Operator));
                        }
                        throw new InvalidPluginExecutionException(
                            "Error expected one value in the condition values for operator " + condition.Operator);
                    }
                case ConditionOperator.GreaterEqual:
                    {
                        if (condition.Values.Count == 1)
                        {
                            var compareValue = condition.Values[0];
                            if (fieldValue == null)
                                return false;
                            if (fieldValue is IComparable)
                                return ((IComparable)fieldValue).CompareTo(compareValue) >= 0;
                            throw new InvalidPluginExecutionException(
                                string.Format("Type {0} not implemented for condition operator {1}",
                                    fieldValue.GetType().Name, condition.Operator));
                        }
                        throw new InvalidPluginExecutionException(
                            "Error expected one value in the condition values for operator " + condition.Operator);
                    }
                case ConditionOperator.In:
                    {
                        if (fieldValue == null)
                            return false;
                        if (fieldValue is OptionSetValue)
                        {
                            var intValue = ((OptionSetValue)fieldValue).Value;
                            foreach (var value in condition.Values)
                            {
                                if (value.Equals(intValue))
                                    return true;
                                if (value is IEnumerable<object> && ((IEnumerable<object>)value).Any(
                                    v => v.Equals((intValue))))
                                    return true;
                            }
                            return false;
                        }
                        throw new InvalidPluginExecutionException(
                            "Error Type Of " + fieldValue.GetType().Name + " not implemented for operator " +
                            condition.Operator);
                    }
            }
            if (condition.Operator == ConditionOperator.Equal && condition.Values.Count == 1)
                return FieldsEqual(fieldValue, condition.Values[0]);
            throw new InvalidPluginExecutionException("MeetsCondition not implemented for condition type");
        }

        /// <summary>
        ///  Gets the value of a date field in the entity
        /// </summary>
        /// <param name="entity">Entity object containing the field</param>
        /// <param name="dateFieldName">Name of the date field</param>
        /// <returns>The field value cast as nullable date</returns>
        public static DateTime? GetDateTimeField(this Entity entity, string dateFieldName)
        {
            return (DateTime?)GetField(entity, dateFieldName);
        }

        /// <summary>
        /// Returns a list of all the activity party entities in an activity party field
        /// </summary>
        /// <param name="entity">Entity object containing the field</param>
        /// <param name="fieldName">Name of the activity party field</param>
        /// <returns>IEnumerable of activity party entities</returns>
        public static IEnumerable<Entity> GetActivityParties(this Entity entity, string fieldName)
        {
            var fieldValue = GetField(entity, fieldName);
            if (fieldValue == null)
                return new Entity[] { };
            else
            {
                return ((EntityCollection)fieldValue).Entities;
            }
        }

        /// <summary>
        /// Adds a new party to a field of type activity party
        /// </summary>
        /// <param name="entity">Entity object containing the field to add the party to</param>
        /// <param name="fieldName">Name of the activity party field being added to</param>
        /// <param name="type">The type of the entity which is a party to the activity</param>
        /// <param name="id">The id of the entity which is a party to the activity</param>
        public static void AddActivityParty(this Entity entity, string fieldName, string type, Guid id)
        {
            var parties = new List<Entity>();
            var currentParties = GetField(entity, fieldName);
            if (currentParties != null)
            {
                if (currentParties is Entity[])
                    parties.AddRange((Entity[])currentParties);
                else if (currentParties is EntityCollection)
                    parties.AddRange(((EntityCollection)currentParties).Entities);
                else throw new NotImplementedException("Not Implemented Where Existing Value Of Type " + currentParties.GetType().Name);
            }
            var entityReference = new EntityReference(type, id);
            var partyEntity = new Entity(Entities.activityparty);
            SetField(partyEntity, Fields.activityparty_.partyid, entityReference);
            parties.Add(partyEntity);
            SetField(entity, fieldName, parties.ToArray());
        }

        public static IEnumerable<Entity> GetEntitiesField(this Entity entity, string fieldName)
        {
            var value = entity.GetField(fieldName);
            if (value == null)
                return new Entity[0];
            if (value is EntityCollection)
            {
                var collection = (EntityCollection)value;
                if (collection.Entities.Any())
                {
                    return collection.Entities.ToList();
                }
                return null;
            }
            throw new Exception(string.Format("Field {0} Of Unexpected Type For Method {1}", fieldName,
                value.GetType().Name));
        }
    }
}