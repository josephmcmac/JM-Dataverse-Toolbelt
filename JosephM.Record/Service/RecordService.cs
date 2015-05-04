#region

using System;
using System.Collections.Generic;
using System.Linq;
using JosephM.Core.Extentions;
using JosephM.Core.FieldType;
using JosephM.ObjectMapping;
using JosephM.Record.IService;
using JosephM.Record.Metadata;
using JosephM.Record.Query;

#endregion

namespace JosephM.Record.Service
{
    /// <summary>
    ///     General Use Implementation Of IRecordService For Late Bound Property Bag Records
    /// </summary>
    public abstract class RecordService : RecordServiceBase
    {
        private readonly List<RecordObject> _records = new List<RecordObject>();

        protected RecordService(IEnumerable<RecordMetadata> recordMetadata,
            IEnumerable<One2ManyRelationshipMetadata> one2ManyRelationships,
            IEnumerable<Many2ManyRelationshipMetadata> many2ManyRelationships)
        {
            RecordMetadata = recordMetadata;
            One2ManyRelationships = one2ManyRelationships;
            Many2ManyRelationships = many2ManyRelationships;
        }

        public IEnumerable<RecordMetadata> RecordMetadata { get; private set; }
        public IEnumerable<One2ManyRelationshipMetadata> One2ManyRelationships { get; private set; }
        public IEnumerable<Many2ManyRelationshipMetadata> Many2ManyRelationships { get; private set; }

        public RecordMetadata GetRecordMetadata(string recordType)
        {
            if (RecordMetadata.Any(r => r.SchemaName == recordType))
                return RecordMetadata.First(r => r.SchemaName == recordType);
            throw new ArgumentOutOfRangeException("recordType",
                string.Format("There Is No {0} Defined For Record Type: {1}", typeof (RecordMetadata), recordType));
        }

        private IEnumerable<FieldMetadata> StandardFields
        {
            get
            {
                return new[]
                {
                    new DateFieldMetadata("createdon", "Created On") { IncludeTime=true}
                };
            }
        }

        public IEnumerable<FieldMetadata> GetFieldMetadata(string recordType)
        {
            return GetRecordMetadata(recordType).Fields.Union(StandardFields);
        }

        public override IRecord GetFirst(string recordType, string fieldName, object fieldValue)
        {
            foreach (var record in GetRecordsOfType(recordType))
            {
                if (FieldsEqual(record.GetField(fieldName), fieldValue))
                    return Clone(record, null);
            }
            return null;
        }

        private IEnumerable<IRecord> GetRecordsOfType(string recordType)
        {
            return _records.Where(r => r.Type == recordType);
        }

        public override void Update(IRecord record)
        {
            Update(record, null);
        }


        public override IRecord NewRecord(string recordType)
        {
            return new RecordObject(recordType);
        }

        public override void Associate(Many2ManyRelationshipMetadata relationship, IRecord record1, IRecord record2)
        {
            var associationRecord = new RecordObject(relationship.IntersectEntityName);
            associationRecord.SetField(relationship.Entity1IntersectAttribute, record1.Id, this);
            associationRecord.SetField(relationship.Entity2IntersectAttribute, record2.Id, this);
            _records.Add(associationRecord);
        }

        public override string Create(IRecord record)
        {
            string id;
            if (string.IsNullOrEmpty(record.Id))
            {
                id = new Guid().ToString();
                record.Id = id;
            }
            else
            {
                id = record.Id;
            }
            var primaryKey = GetPrimaryKey(record.Type);
            record.SetField(primaryKey, id, this);
            record.SetField("createdon", DateTime.Now, this);
            _records.Add((RecordObject)Clone(record, null));
            return id;
        }

        public override FieldMetadata GetFieldMetadata(string field, string recordType)
        {
            var items = GetFieldMetadata(recordType).Where(mt => mt.SchemaName == field);
            if (items.Any())
                return items.ElementAt(0);
            throw new ArgumentOutOfRangeException("No field of name " + field + " is defined in the metadata");
        }

        public override IEnumerable<IRecord> GetLinkedRecords(string linkedEntityType, string entityTypeFrom,
            string linkedEntityLookup, string entityFromId)
        {
            return
                Clone(GetRecordsOfType(linkedEntityType).Where(
                    r =>
                    {
                        var lookup = r.GetLookupField(linkedEntityLookup);
                        return lookup != null && lookup.Id == entityFromId && lookup.RecordType == entityTypeFrom;
                    }), null);
        }

        public override void Update(IRecord record, IEnumerable<string> fieldsToUpdate)
        {
            var savedRecord = Get(record.Type, record.Id);
            CopyFieldsTo(record, savedRecord, fieldsToUpdate);
            foreach (var field in savedRecord.GetFieldsInEntity())
            {
                var value = record.GetField(field);
                if (value is Lookup)
                {
                    var lookup = ((Lookup)value);
                    var id = lookup.Id;
                    var type = lookup.RecordType;
                    var referencedRecord = Get(type, id);
                    var name = referencedRecord.GetStringField(GetPrimaryField(type));
                    lookup.Name = name;
                }
            }
        }

        public override IEnumerable<IRecord> RetrieveMultiple(string recordType, string searchString, int maxCount)
        {
            return Clone(GetRecordsOfType(recordType).Where(r =>
            {
                var stringValue = (string) r.GetField(GetPrimaryField(recordType));
                return stringValue != null && stringValue.ToLower().StartsWith(searchString.ToLower());
            }), null);
        }

        public override string GetPrimaryField(string recordType)
        {
            return GetRecordMetadata(recordType).GetPrimaryFieldMetadata().SchemaName;
        }

        public override IRecord Get(string recordType, string id)
        {
            var recordsOfType = GetRecordsOfType(recordType);
            if (recordsOfType.Any(r => r.Id == id))
                return Clone(recordsOfType.First(r => r.Id == id), null);

            throw new Exception(string.Format("No Record Exists Of Type '{0}' With Id = '{1}'", recordType, id));
        }

        public override string GetDisplayName(string recordType)
        {
            return GetRecordMetadata(recordType).DisplayName;
        }

        public override IEnumerable<Many2ManyRelationshipMetadata> GetManyToManyRelationships(string recordType)
        {
            return
                Many2ManyRelationships.Where(
                    r => r.Entity1LogicalName == recordType || r.Entity2LogicalName == recordType);
        }

        public override IEnumerable<IRecord> GetRelatedRecords(IRecord recordToExtract,
            Many2ManyRelationshipMetadata many2ManyRelationshipMetadata,
            bool from1)
        {
            var intersects =
                _records.Where(
                    r =>
                        r.Type == many2ManyRelationshipMetadata.IntersectEntityName &&
                        r.GetStringField(from1
                            ? many2ManyRelationshipMetadata.Entity1IntersectAttribute
                            : many2ManyRelationshipMetadata.Entity2IntersectAttribute) == recordToExtract.Id);
            var matchField = from1
                ? many2ManyRelationshipMetadata.Entity2IntersectAttribute
                : many2ManyRelationshipMetadata.Entity1IntersectAttribute;
            var matchType = from1
                ? many2ManyRelationshipMetadata.Entity2LogicalName
                : many2ManyRelationshipMetadata.Entity1LogicalName;
            return
                Clone(GetRecordsOfType(matchType).Where(r => intersects.Select(i => i.GetStringField(matchField)).Contains(r.Id)), null);
        }

        public override string GetRelationshipLabel(Many2ManyRelationshipMetadata many2ManyRelationshipMetadata,
            bool from1)
        {
            return from1
                ? GetCollectionName(many2ManyRelationshipMetadata.Entity2LogicalName)
                : GetCollectionName(many2ManyRelationshipMetadata.Entity1LogicalName);
        }

        public override IEnumerable<One2ManyRelationshipMetadata> GetOneToManyRelationships(string recordType)
        {
            return One2ManyRelationships.Where(r => r.ReferencedEntity == recordType);
        }

        public override IEnumerable<IRecord> GetRelatedRecords(IRecord recordToExtract,
            One2ManyRelationshipMetadata one2ManyRelationshipMetadata)
        {
            return
                _records.Where(
                    r =>
                        r.Type == one2ManyRelationshipMetadata.ReferencingEntity &&
                        r.GetLookupId(one2ManyRelationshipMetadata.ReferencingAttribute) == recordToExtract.Id);
        }

        public override string GetRelationshipLabel(One2ManyRelationshipMetadata one2ManyRelationshipMetadata)
        {
            return GetCollectionName(one2ManyRelationshipMetadata.ReferencingEntity);
        }

        public override string GetCollectionName(string recordType)
        {
            return GetRecordMetadata(recordType).DisplayCollectionName;
        }

        public override string GetPrimaryKey(string recordType)
        {
            return GetRecordMetadata(recordType).PrimaryKeyName;
        }

        public override IEnumerable<string> GetAllRecordTypes()
        {
            return RecordMetadata.Select(r => r.SchemaName);
        }

        public override IEnumerable<IRecord> GetFirstX(string recordType, int x)
        {
            return GetRecordsOfType(recordType).Take(x);
        }

        public override IEnumerable<ViewMetadata> GetViews(string recordType)
        {
            return GetRecordMetadata(recordType).Views;
        }

        public override IEnumerable<IRecord> RetrieveAllAndClauses(string recordType,
            IEnumerable<Condition> andConditions)
        {
            return RetrieveAllAndClauses(recordType, andConditions, null);
        }

        public override IEnumerable<IRecord> RetrieveAllAndClauses(string recordType,
            IEnumerable<Condition> andConditions, IEnumerable<string> fields)
        {
            return Clone(GetRecordsOfType(recordType).Where(r => andConditions.All(c => c.MeetsCondition(r))), fields);
        }

        public override bool IsString(string fieldName, string recordType)
        {
            var fieldType = GetFieldType(fieldName, recordType);
            return fieldType == RecordFieldType.String || fieldType == RecordFieldType.Memo;
        }

        public override IEnumerable<IRecord> RetrieveAllOrClauses(string recordType, IEnumerable<Condition> orConditions,
            IEnumerable<string> fields)
        {
            return Clone(GetRecordsOfType(recordType).Where(r => orConditions.Any(c => c.MeetsCondition(r))), fields);
        }

        public override IEnumerable<IRecord> RetrieveAllOrClauses(string recordType, IEnumerable<Condition> orConditions)
        {
            return RetrieveAllOrClauses(recordType, orConditions, null);
        }

        public override IEnumerable<string> GetFields(string recordType)
        {
            return GetFieldMetadata(recordType).Select(m => m.SchemaName);
        }

        public override IEnumerable<string> GetAllRecordTypesForSearch()
        {
            return GetAllRecordTypes();
        }

        public override IEnumerable<string> GetStringFields(string recordType)
        {
            return GetFieldMetadata(recordType)
                .Where(f => IsString(f.SchemaName, recordType))
                .Select(f => f.SchemaName);
        }

        public override bool IsActivityType(string recordType)
        {
            return GetRecordMetadata(recordType).IsActivityType;
        }

        public override bool IsActivityPartyParticipant(string recordType)
        {
            return GetRecordMetadata(recordType).IsActivityParticipant;
        }

        public override IEnumerable<string> GetActivityTypes()
        {
            return RecordMetadata.Where(m => m.IsActivityType).Select(m => m.SchemaName);
        }

        public override string GetFieldAsDisplayString(IRecord record, string fieldName)
        {
            var fieldValue = record.GetField(fieldName);
            if (fieldValue == null)
                return null;
            if (fieldValue is string)
                return (string) fieldValue;
            if (fieldValue is Lookup)
                return ((Lookup) fieldValue).Name;
            if (IsActivityParty(fieldName, record.Type))
            {
                if (fieldValue is IRecord[])
                {
                    var namesToOutput = new List<string>();
                    foreach (var party in (IRecord[]) fieldValue)
                    {
                        namesToOutput.Add(party.GetLookupName("partyid"));
                    }
                    return string.Join(", ", namesToOutput.Where(f => !f.IsNullOrWhiteSpace()));
                }
            }
            return fieldValue.ToString();
        }

        public override IRecord GetFirst(string recordType)
        {
            return
                _records.Any(r => r.Type == recordType)
                    ? Clone(_records.First(r => r.Type == recordType), null)
                    : null;
        }

        public override IEnumerable<IRecord> GetFirstX(string recordType, int x, IEnumerable<string> fields, IEnumerable<Condition> conditions, IEnumerable<SortExpression> sortExpressions)
        {
            return Clone(GetRecordsOfType(recordType).Where(r => conditions.All(c => c.MeetsCondition(r))), fields);
        }

        private IEnumerable<IRecord> Clone(IEnumerable<IRecord> records, IEnumerable<string> fields)
        {
            return records.Select(r => Clone(r, fields));
        }

        private IRecord Clone(IRecord record, IEnumerable<string> fields)
        {
            var newRecord = NewRecord(record.Type);
            newRecord.Id = record.Id;
            CopyFieldsTo(record, newRecord, fields);
            return newRecord;
        }

        private void CopyFieldsTo(IRecord copyFrom, IRecord copyTo, IEnumerable<string> fields)
        {
            if (fields == null)
                fields = copyFrom.GetFieldsInEntity();
            foreach (var field in fields)
            {
                var value = copyFrom.GetField(field);
                if (value == null
                    || value is string
                    || value is int
                    || value is decimal
                    || value is DateTime
                    || value is bool)
                    copyTo.SetField(field, value, this);
                else if (value is IRecord[])
                {
                    var newValue = ((IRecord[]) value).Select(r => Clone(r, null)).ToArray();
                    copyTo.SetField(field, newValue, this);
                }
                else
                {
                    var mapper = new ClassSelfMapper();
                    copyTo.SetField(field, mapper.Map(copyFrom.GetField(field)), this);
                }
            }
        }
    }
}