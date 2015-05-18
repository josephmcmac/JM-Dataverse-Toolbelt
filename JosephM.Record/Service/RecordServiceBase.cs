#region

using System;
using System.Collections.Generic;
using JosephM.Core.FieldType;
using JosephM.Core.Service;
using JosephM.Record.IService;
using JosephM.Record.Metadata;
using JosephM.Record.Query;

#endregion

namespace JosephM.Record.Service
{
    /// <summary>
    ///     Base Class For An Implementation Of IRecordService
    /// </summary>
    public abstract class RecordServiceBase : IRecordService
    {
        public virtual IEnumerable<IRecord> RetrieveAll(string recordType, IEnumerable<string> fields)
        {
            throw new NotImplementedException();
        }

        public string GetFieldLabel(string fieldName, string recordType)
        {
            return GetFieldMetadata(fieldName, recordType).DisplayName;
        }

        public virtual int GetMaxLength(string fieldName, string recordType)
        {
            return ((StringFieldMetadata) GetFieldMetadata(fieldName, recordType)).MaxLength;
        }

        public virtual IEnumerable<PicklistOption> GetPicklistKeyValues(string field, string entity)
        {
            return
                ((ComboBoxFieldMetadata) GetFieldMetadata(field, entity)).Items;
        }

        public bool IsMandatory(string field, string entity)
        {
            return GetFieldMetadata(field, entity).IsMandatory;
        }

        public RecordFieldType GetFieldType(string field, string entity)
        {
            return GetFieldMetadata(field, entity).FieldType;
        }

        public int GetMaxIntValue(string field, string entity)
        {
            return ((IntegerFieldMetadata) GetFieldMetadata(field, entity)).Maximum;
        }

        public int GetMinIntValue(string field, string entity)
        {
            return ((IntegerFieldMetadata) GetFieldMetadata(field, entity)).Minimum;
        }

        public virtual void Update(IRecord record)
        {
            throw new NotImplementedException();
        }

        public virtual IRecord NewRecord(string recordType)
        {
            throw new NotImplementedException();
        }


        public virtual string Create(IRecord record)
        {
            throw new NotImplementedException();
        }

        public virtual IEnumerable<IRecord> GetLinkedRecords(string linkedEntityType, string entityTypeFrom,
            string linkedEntityLookup, string entityFromId)
        {
            throw new NotImplementedException();
        }

        public virtual ParseFieldResponse ParseFieldRequest(ParseFieldRequest parseFieldRequest)
        {
            var fieldName = parseFieldRequest.FieldName;
            var entityType = parseFieldRequest.RecordType;
            var value = parseFieldRequest.Value;
            var parsedValue = value;
            var fieldType = GetFieldType(fieldName, entityType);
            if (value != null)
            {
                switch (fieldType)
                {
                    case RecordFieldType.String:
                    {
                        var maxLength = GetMaxLength(fieldName, entityType);
                        var temp = value.ToString();
                        if (temp.Length > maxLength)
                            return
                                new ParseFieldResponse(string.Concat("Field ", fieldName,
                                    " exceeded maximum length of " + maxLength));
                        parsedValue = temp;
                        break;
                    }
                    case RecordFieldType.Integer:
                    {
                        if (value is int)
                            parsedValue = (int) value;
                        else if (value is string && String.IsNullOrWhiteSpace((string) value))
                        {
                        }
                        else
                        {
                            int tempInt;
                            if (!int.TryParse(value.ToString(), out tempInt))
                                return
                                    new ParseFieldResponse(string.Concat("Error parsing integer from ",
                                        value.ToString()));
                            parsedValue = tempInt;
                        }
                        break;
                    }
                    case RecordFieldType.Date:
                    {
                        if (value is DateTime)
                            parsedValue = (DateTime) value;
                        else
                            return new ParseFieldResponse("value not of DateTime type");
                        break;
                    }
                    case RecordFieldType.RecordType:
                    {
                        if (value is RecordType)
                            parsedValue = value;
                        else
                            return
                                new ParseFieldResponse(
                                    string.Concat("Parse {0} for type not implemented: ", RecordFieldType.RecordType,
                                        value.GetType().Name));
                        break;
                    }
                    case RecordFieldType.Picklist:
                    {
                        if (value is KeyValuePair<int, string>)
                            parsedValue = value;
                        else if (value is PicklistOption)
                            parsedValue = value;
                        else
                            return new ParseFieldResponse("value not of keyvalue type");
                        break;
                    }
                    case RecordFieldType.Boolean:
                    {
                        if (value is bool)
                            parsedValue = value;
                        else
                            return new ParseFieldResponse("value not of boolean type");
                        break;
                    }
                    case RecordFieldType.ExcelFile:
                    {
                        if (value is ExcelFile)
                            parsedValue = value;
                        else
                            return new ParseFieldResponse("value not of excelfile type");
                        break;
                    }
                    case RecordFieldType.Folder:
                    {
                        if (value is Folder)
                            parsedValue = value;
                        else
                            return new ParseFieldResponse("value not of folder type");
                        break;
                    }
                    case RecordFieldType.Password:
                    {
                        if (value is Password)
                            parsedValue = value;
                        else if (value is string)
                        {
                            var s = (string) value;
                            if (string.IsNullOrEmpty(s))
                                parsedValue = null;
                            else
                                parsedValue = Password.CreateFromRawPassword(s);
                        }
                        else
                            return
                                new ParseFieldResponse("Input type not defined for parse field of type " +
                                                       typeof (Password).Name);
                        break;
                    }
                    case RecordFieldType.StringEnumerable:
                    {
                        if (value is IEnumerable<string>)
                            parsedValue = value;
                        else
                            return
                                new ParseFieldResponse(
                                    string.Concat("Parse string enumerable for type not implemented: ",
                                        value.GetType().Name));
                        break;
                    }
                    case RecordFieldType.Lookup:
                    {
                        if (value is Lookup)
                            parsedValue = value;
                        else
                            return
                                new ParseFieldResponse(
                                    string.Concat("Parse {0} for type not implemented: ", typeof (Lookup).Name,
                                        value.GetType().Name));
                        break;
                    }
                }
            }
            return new ParseFieldResponse(parsedValue);
        }

        public virtual IRecord GetFirst(string entityType, string fieldName, object fieldValue)
        {
            throw new NotImplementedException();
        }

        public virtual void Update(IRecord Record, IEnumerable<string> changedPersistentFields)
        {
            throw new NotImplementedException();
        }

        public virtual IEnumerable<IRecord> RetrieveMultiple(string recordType, string searchString, int maxCount)
        {
            throw new NotImplementedException();
        }

        public virtual FieldMetadata GetFieldMetadata(string field, string recordType)
        {
            throw new NotImplementedException();
        }


        public string GetLookupTargetType(string field, string recordType)
        {
            return ((LookupFieldMetadata) GetFieldMetadata(field, recordType)).ReferencedRecordType;
        }


        public virtual string GetPrimaryField(string recordType)
        {
            throw new NotImplementedException();
        }

        public int GetPicklistDefault(string field, string entity)
        {
            throw new NotImplementedException();
        }

        public bool FieldsEqual(object field1, object field2)
        {
            if (field1 == null && field2 == null)
                return true;
            else if (field1 == null || field2 == null)
            {
                if (field1 is string || field2 is string)
                    return String.IsNullOrEmpty((string) field1) && String.IsNullOrEmpty((string) field2);
                else
                    return false;
            }
            else if (field1 is DateTime && field2 is DateTime)
                return ((DateTime) field1).Equals((DateTime) field2);
            else if (field1 is Password && field2 is Password)
                return ((Password) field1).GetRawPassword() == ((Password) field2).GetRawPassword();
            else if (field1 is int)
            {
                if (field2 is int)
                    return ((int) field1).Equals(((int) field2));
                else
                    throw new ArgumentException("Mismatched Types");
            }
            else if (field1 is bool && field2 is bool)
                return ((bool) field1).Equals(((bool) field2));
            else if (field1 is Guid && field2 is Guid)
                return ((Guid) field1).Equals(((Guid) field2));
            else if (field1 is Double && field2 is Double)
                return ((Double) field1).Equals(((Double) field2));
            else if (field1 is string && field2 is string)
                return ((string) field1).Equals(((string) field2));
            else if (field1 is Lookup && field2 is Lookup)
                return (field1).Equals((field2));
            else
                throw new ArgumentException("FieldsEqual type not implemented for type " + field1.GetType().Name);
        }


        public virtual IEnumerable<string> GetFields(string recordType)
        {
            throw new NotImplementedException();
        }


        public virtual string GetCollectionName(string recordType)
        {
            throw new NotImplementedException();
        }

        public virtual string GetDisplayName(string recordType)
        {
            throw new NotImplementedException();
        }

        public bool IsAuditOn(string p)
        {
            throw new NotImplementedException();
        }


        public string GetFieldDescription(string fieldName, string recordType)
        {
            throw new NotImplementedException();
        }

        public bool IsFieldAuditOn(string fieldName, string recordType)
        {
            throw new NotImplementedException();
        }

        public bool IsFieldSearchable(string fieldName, string recordType)
        {
            throw new NotImplementedException();
        }

        public TextFormat GetTextFormat(string fieldName, string recordType)
        {
            return TextFormat.Text;
        }

        public decimal GetMinDecimalValue(string p1, string p2)
        {
            throw new NotImplementedException();
        }

        public decimal GetMaxDecimalValue(string p1, string p2)
        {
            throw new NotImplementedException();
        }

        public bool IsDateIncludeTime(string p1, string p2)
        {
            throw new NotImplementedException();
        }

        public virtual IRecord GetFirst(string recordType)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<IRecord> GetWhere(string p1, string p2, string p3)
        {
            throw new NotImplementedException();
        }


        public bool IsReadOnlyField(string fieldName, string recordType)
        {
            throw new NotImplementedException();
        }


        public IntegerType GetIntegerType(string fieldName, string recordType)
        {
            throw new NotImplementedException();
        }


        public virtual string GetPrimaryKey(string recordType)
        {
            throw new NotImplementedException();
        }


        public virtual IRecord Get(string recordType, string id)
        {
            throw new NotImplementedException();
        }


        public string GetFieldAsMatchString(string recordType, string fieldName, object fieldValue)
        {
            throw new NotImplementedException();
        }

        public IDictionary<string, IRecord> IndexRecordsByField(string recordType, string matchField)
        {
            throw new NotImplementedException();
        }

        public object ParseField(string fieldName, string recordType, object value)
        {
            throw new NotImplementedException();
        }

        public IDictionary<string, IEnumerable<string>> IndexAssociatedIds(string recordType, string relationshipName,
            string otherSideId)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<string> GetAssociatedIds(string recordType, string id, string relationshipName,
            string otherSideId)
        {
            throw new NotImplementedException();
        }


        public IDictionary<string, string> IndexGuidsByValue(string recordType, string fieldName)
        {
            throw new NotImplementedException();
        }


        public IDictionary<string, string> IndexMatchingGuids(string recordType, string fieldName,
            IEnumerable<string> unmatchedStrings)
        {
            throw new NotImplementedException();
        }


        public bool IsLookup(string fieldName, string recordType)
        {
            throw new NotImplementedException();
        }

        public virtual IEnumerable<IRecord> RetrieveAllOrClauses(string recordType, IEnumerable<Condition> orConditions)
        {
            throw new NotImplementedException();
        }

        public virtual IEnumerable<IRecord> RetrieveAllAndClauses(string recordType,
            IEnumerable<Condition> andConditions)
        {
            throw new NotImplementedException();
        }

        public void Create(IRecord record, IEnumerable<string> fieldToSet)
        {
            throw new NotImplementedException();
        }

        public void Delete(IRecord record)
        {
            throw new NotImplementedException();
        }

        public void Delete(string recordType, string id)
        {
            throw new NotImplementedException();
        }

        public virtual IEnumerable<IRecord> GetFirstX(string recordType, int x)
        {
            throw new NotImplementedException();
        }


        public void CreateOrUpdate(RecordMetadata recordMetadata)
        {
            throw new NotImplementedException();
        }

        public void CreateOrUpdate(RelationshipMetadata relationshipMetadata)
        {
            throw new NotImplementedException();
        }

        public void CreateOrUpdate(FieldMetadata fieldMetadata, string recordType)
        {
            throw new NotImplementedException();
        }

        public bool RecordTypeExists(string recordType)
        {
            throw new NotImplementedException();
        }

        public bool FieldExists(string fieldName, string recordType)
        {
            throw new NotImplementedException();
        }

        public bool RelationshipExists(string relationship)
        {
            throw new NotImplementedException();
        }

        public IsValidResponse VerifyConnection()
        {
            throw new NotImplementedException();
        }

        public void PublishAll()
        {
            throw new NotImplementedException();
        }

        public void UpdateViews(RecordMetadata recordMetadata)
        {
            throw new NotImplementedException();
        }

        public bool SharedOptionSetExists(string optionSetName)
        {
            throw new NotImplementedException();
        }

        public void CreateOrUpdateSharedOptionSet(PicklistOptionSet sharedOptionSet)
        {
            throw new NotImplementedException();
        }

        public void UpdateFieldOptionSet(string entityType, string fieldName, PicklistOptionSet optionSet)
        {
            throw new NotImplementedException();
        }

        public virtual IEnumerable<string> GetAllRecordTypes()
        {
            throw new NotImplementedException();
        }

        public virtual string GetFieldAsDisplayString(IRecord Record, string fieldName)
        {
            var fieldValue = Record.GetField(fieldName);
            return fieldValue == null ? "" : fieldValue.ToString();
        }

        public string GetSqlViewName(string recordType)
        {
            throw new NotImplementedException();
        }

        public string GetDatabaseName()
        {
            throw new NotImplementedException();
        }


        public virtual IRecordService LookupService
        {
            get { return this; }
        }

        public virtual IEnumerable<One2ManyRelationshipMetadata> GetOneToManyRelationships(string recordType)
        {
            throw new NotImplementedException();
        }


        public virtual IEnumerable<IRecord> GetRelatedRecords(IRecord recordToExtract,
            One2ManyRelationshipMetadata one2ManyRelationshipMetadata)
        {
            throw new NotImplementedException();
        }

        public virtual string GetRelationshipLabel(One2ManyRelationshipMetadata one2ManyRelationshipMetadata)
        {
            throw new NotImplementedException();
        }

        public virtual IEnumerable<Many2ManyRelationshipMetadata> GetManyToManyRelationships(string recordType)
        {
            throw new NotImplementedException();
        }

        public virtual IEnumerable<IRecord> GetRelatedRecords(IRecord recordToExtract,
            Many2ManyRelationshipMetadata many2ManyRelationshipMetadata, bool from1)
        {
            throw new NotImplementedException();
        }

        public virtual string GetRelationshipLabel(Many2ManyRelationshipMetadata many2ManyRelationshipMetadata,
            bool from1)
        {
            throw new NotImplementedException();
        }

        public bool IsActivityParty(string fieldName, string recordType)
        {
            return GetFieldType(fieldName, recordType) == RecordFieldType.ActivityParty;
        }

        public virtual void Associate(Many2ManyRelationshipMetadata relationship, IRecord record1, IRecord record2)
        {
            throw new NotImplementedException();
        }

        public virtual string GetOptionLabel(string optionKey, string field, string recordType)
        {
            throw new NotImplementedException();
        }


        public virtual IEnumerable<ViewMetadata> GetViews(string recordType)
        {
            throw new NotImplementedException();
        }


        public virtual bool IsString(string fieldName, string recordType)
        {
            throw new NotImplementedException();
        }

        public virtual IEnumerable<string> GetStringFields(string recordType)
        {
            throw new NotImplementedException();
        }


        public virtual IEnumerable<IRecord> RetrieveAllOrClauses(string entityType, IEnumerable<Condition> orConditions,
            IEnumerable<string> fields)
        {
            throw new NotImplementedException();
        }


        public virtual IEnumerable<IRecord> RetrieveAllAndClauses(string entityType,
            IEnumerable<Condition> andConditions, IEnumerable<string> fields)
        {
            throw new NotImplementedException();
        }


        public virtual IEnumerable<string> GetAllRecordTypesForSearch()
        {
            throw new NotImplementedException();
        }


        public virtual bool IsActivityType(string recordType)
        {
            throw new NotImplementedException();
        }


        public virtual IEnumerable<string> GetActivityParticipantTypes()
        {
            throw new NotImplementedException();
        }


        public virtual bool IsActivityPartyParticipant(string recordType)
        {
            throw new NotImplementedException();
        }

        public virtual IEnumerable<string> GetActivityTypes()
        {
            throw new NotImplementedException();
        }


        public virtual IEnumerable<IRecord> GetFirstX(string type, int x, IEnumerable<string> fields,
            IEnumerable<Condition> conditions, IEnumerable<SortExpression> sort)
        {
            throw new NotImplementedException();
        }


        public virtual IEnumerable<PicklistOption> GetPicklistKeyValues(string field, string recordType,
            string dependentValue)
        {
            throw new NotImplementedException();
        }


        public virtual bool IsWritable(string fieldName, string recordType)
        {
            return true;
        }

        public bool IsCreateable(string fieldName, string recordType)
        {
            throw new NotImplementedException();
        }

        public virtual bool IsReadable(string fieldName, string recordType)
        {
            return true;
        }

        public virtual bool IsCustomField(string fieldName, string recordType)
        {
            return false;
        }

        public virtual bool IsCustomType(string recordType)
        {
            return false;
        }

        public bool IsCustomRelationship(string relationshipName)
        {
            return false;
        }

        public string GetRecordTypeCode(string thisType)
        {
            return thisType;
        }

        public bool IsDisplayRelated(Many2ManyRelationshipMetadata relationship, bool from1)
        {
            return true;
        }

        public bool IsCustomLabel(Many2ManyRelationshipMetadata relationship, bool from1)
        {
            throw new NotImplementedException();
        }

        public int DisplayOrder(Many2ManyRelationshipMetadata relationship, bool from1)
        {
            throw new NotImplementedException();
        }

        public int GetDisplayOrder(Many2ManyRelationshipMetadata relationship, bool from1)
        {
            return 0;
        }

        public bool IsDisplayRelated(One2ManyRelationshipMetadata relationship)
        {
            return true;
        }

        public int GetDisplayOrder(One2ManyRelationshipMetadata relationship)
        {
            return 0;
        }

        public bool IsCustomLabel(One2ManyRelationshipMetadata relationship)
        {
            throw new NotImplementedException();
        }

        public double GetMinDoubleValue(string field, string recordType)
        {
            throw new NotImplementedException();
        }

        public double GetMaxDoubleValue(string field, string recordType)
        {
            throw new NotImplementedException();
        }

        public decimal GetMinMoneyValue(string field, string recordType)
        {
            throw new NotImplementedException();
        }

        public decimal GetMaxMoneyValue(string field, string recordType)
        {
            throw new NotImplementedException();
        }

        public bool IsFieldDisplayRelated(string field, string recordType)
        {
            return true;
        }

        public bool IsSharedPicklist(string field, string recordType)
        {
            throw new NotImplementedException();
        }

        public string GetSharedPicklistDisplayName(string field, string recordType)
        {
            throw new NotImplementedException();
        }

        public string GetSharedPicklistDisplayName(string optionSetName)
        {
            throw new NotImplementedException();
        }

        public bool HasNotes(string recordType)
        {
            throw new NotImplementedException();
        }

        public bool HasActivities(string recordType)
        {
            throw new NotImplementedException();
        }

        public bool HasConnections(string recordType)
        {
            throw new NotImplementedException();
        }

        public bool HasMailMerge(string recordType)
        {
            throw new NotImplementedException();
        }

        public bool HasQueues(string recordType)
        {
            throw new NotImplementedException();
        }

        public string GetDescription(string recordType)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<string> GetSharedOptionSetNames()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<KeyValuePair<string, string>> GetSharedOptionSetKeyValues(string optionSetName)
        {
            throw new NotImplementedException();
        }

        public virtual IEnumerable<IRecord> GetLinkedRecordsThroughBridge(string linkedRecordType, string recordTypeThrough, string recordTypeFrom, string linkedThroughLookupFrom, string linkedThroughLookupTo, string recordFromId)
        {
            throw new NotImplementedException();
        }

        public bool IsCustomer(string recordType, string field)
        {
            throw new NotImplementedException();
        }
    }
}