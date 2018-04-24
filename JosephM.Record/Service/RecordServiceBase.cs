#region

using System;
using System.Collections.Generic;
using System.Linq;
using JosephM.Core.FieldType;
using JosephM.Core.Service;
using JosephM.Record.Extentions;
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
        #region retain

        public abstract IRecord NewRecord(string recordType);

        public abstract IRecord Get(string recordType, string id);

        public abstract void Update(IRecord record, IEnumerable<string> fieldToCommit);

        public abstract string Create(IRecord record, IEnumerable<string> fieldToSet);

        public abstract void Delete(string recordType, string id);

        public abstract IEnumerable<IFieldMetadata> GetFieldMetadata(string recordType);

        public abstract IRecordTypeMetadata GetRecordTypeMetadata(string recordType);

        public abstract IsValidResponse VerifyConnection();

        public string GetFieldAsMatchString(string recordType, string fieldName, object fieldValue)
        {
            return fieldValue == null ? null : fieldValue.ToString();
        }

        public virtual object ParseField(string fieldName, string recordType, object value)
        {
            var parsedValue = value;
            var fieldType = this.GetFieldType(fieldName, recordType);
            if (value != null)
            {
                switch (fieldType)
                {
                    case RecordFieldType.String:
                        {
                            var maxLength = this.GetFieldMetadata(fieldName, recordType).MaxLength;
                            var temp = value.ToString();
                            if (temp.Length > maxLength)
                                throw new Exception(string.Concat("Field ", fieldName,
                                        " exceeded maximum length of " + maxLength));
                            parsedValue = temp;
                            break;
                        }
                    case RecordFieldType.Integer:
                        {
                            if (value is int)
                                parsedValue = (int)value;
                            else if (value is string && String.IsNullOrWhiteSpace((string)value))
                            {
                            }
                            else
                            {
                                int tempInt;
                                if (!int.TryParse(value.ToString(), out tempInt))
                                    throw new Exception(string.Concat("Error parsing integer from ",
                                            value.ToString()));
                                parsedValue = tempInt;
                            }
                            break;
                        }
                    case RecordFieldType.Date:
                        {
                            if (value is DateTime)
                                parsedValue = (DateTime)value;
                            else
                                throw new Exception("value not of DateTime type");
                            break;
                        }
                    case RecordFieldType.RecordType:
                        {
                            if (value is RecordType)
                                parsedValue = value;
                            else
                                throw new Exception(
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
                                throw new Exception("value not of keyvalue type");
                            break;
                        }
                    case RecordFieldType.Boolean:
                        {
                            if (value is bool)
                                parsedValue = value;
                            else
                                throw new Exception("value not of boolean type");
                            break;
                        }
                    case RecordFieldType.Folder:
                        {
                            if (value is Folder)
                                parsedValue = value;
                            else
                                throw new Exception("value not of folder type");
                            break;
                        }
                    case RecordFieldType.Password:
                        {
                            if (value is Password)
                                parsedValue = value;
                            else if (value is string)
                            {
                                var s = (string)value;
                                if (string.IsNullOrEmpty(s))
                                    parsedValue = null;
                                else
                                    parsedValue = Password.CreateFromRawPassword(s);
                            }
                            else
                                throw new Exception("Input type not defined for parse field of type " +
                                                           typeof(Password).Name);
                            break;
                        }
                    case RecordFieldType.StringEnumerable:
                        {
                            if (value is IEnumerable<string>)
                                parsedValue = value;
                            else
                                throw new Exception(
                                        string.Concat("Parse string enumerable for type not implemented: ",
                                            value.GetType().Name));
                            break;
                        }
                    case RecordFieldType.Lookup:
                        {
                            if (value is Lookup)
                                parsedValue = value;
                            else
                                throw new Exception(
                                        string.Concat("Parse {0} for type not implemented: ", typeof(Lookup).Name,
                                            value.GetType().Name));
                            break;
                        }
                }
            }
            return parsedValue;
        }

        public virtual string GetFieldAsDisplayString(IRecord record, string fieldName)
        {
            var fieldValue = record.GetField(fieldName);
            return fieldValue == null ? "" : fieldValue.ToString();
        }

        public virtual IRecordService LookupService
        {
            get { return this; }
        }

        public abstract IEnumerable<IRecord> GetFirstX(string type, int x, IEnumerable<string> fields,
    IEnumerable<Condition> conditions, IEnumerable<SortExpression> sort);

        public virtual void ClearCache()
        {
        }

        public virtual IRecordService GetLookupService(string fieldName, string recordType, string reference, IRecord record)
        {
            return LookupService;
        }

        #endregion retain

        #region unsure

        public virtual IEnumerable<string> GetAllRecordTypes()
        {
            throw new NotImplementedException();
        }

        public virtual IEnumerable<PicklistOption> GetPicklistKeyValues(string field, string recordType, string dependentValue, IRecord record)
        {
            var metadata = this.GetFieldMetadata(field, recordType) as PicklistFieldMetadata;
            return metadata?.PicklistOptions;
        }

        public virtual IEnumerable<IRecord> GetLinkedRecords(string linkedEntityType, string entityTypeFrom,
            string linkedEntityLookup, string entityFromId)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<IPicklistSet> GetSharedPicklists()
        {
            throw new NotImplementedException();
        }

        #endregion unsure

        #region see remove

        public virtual string GetLookupTargetType(string field, string recordType)
        {
            return ((LookupFieldMetadata)this.GetFieldMetadata(field, recordType)).ReferencedRecordType;
        }

        public virtual IEnumerable<IOne2ManyRelationshipMetadata> GetOneToManyRelationships(string recordType)
        {
            throw new NotImplementedException();
        }

        public virtual IEnumerable<IMany2ManyRelationshipMetadata> GetManyToManyRelationships(string recordType)
        {
            throw new NotImplementedException();
        }

        public virtual IEnumerable<IRecord> GetRelatedRecords(IRecord recordToExtract, IMany2ManyRelationshipMetadata many2ManyRelationshipMetadata, bool from1)
        {
            throw new NotImplementedException();
        }

        public virtual IEnumerable<ViewMetadata> GetViews(string recordType)
        {
            return new ViewMetadata[0];
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

        #endregion see remove

        #region metadata changes

        public virtual IsValidResponse IsValidForNewType(string typeName)
        {
            var response = new IsValidResponse();
            response.AddInvalidReason(string.Format("New type name validations have not been implemented for the type {0}", GetType().Name));
            return response;
        }

        public virtual void CreateOrUpdate(RecordMetadata recordMetadata)
        {
            throw new NotImplementedException();
        }

        public virtual void CreateOrUpdate(IMany2ManyRelationshipMetadata relationshipMetadata)
        {
            throw new NotImplementedException();
        }

        public virtual void CreateOrUpdate(FieldMetadata fieldMetadata, string recordType)
        {
            throw new NotImplementedException();
        }

        public virtual void Publish(string xml = null)
        {
        }

        public void UpdateViews(RecordMetadata recordMetadata)
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

        #endregion metadata changes

        public virtual IEnumerable<IRecord> GetLinkedRecordsThroughBridge(string linkedRecordType, string recordTypeThrough, string recordTypeFrom, string linkedThroughLookupFrom, string linkedThroughLookupTo, string recordFromId)
        {
            throw new NotImplementedException();
        }

        public string GetDisplayNameField(string recordType)
        {
            return GetRecordTypeMetadata(recordType).PrimaryFieldSchemaName;
        }

        public virtual IEnumerable<IRecord> RetreiveAll(QueryDefinition query)
        {
            throw new NotImplementedException();
        }

        public virtual IFormService GetFormService()
        {
            return null;
        }

        public string GetWebUrl(string recordType, string id, string additionalparams = null)
        {
            return null;
        }
    }
}
