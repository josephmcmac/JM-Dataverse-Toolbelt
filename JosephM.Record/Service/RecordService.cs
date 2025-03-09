#region

using System;
using System.Collections.Generic;
using System.Linq;
using JosephM.Core.Extentions;
using JosephM.Core.FieldType;
using JosephM.ObjectMapping;
using JosephM.Record.Extentions;
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

        public override IRecordTypeMetadata GetRecordTypeMetadata(string recordType)
        {
            if (RecordMetadata.Any(r => r.SchemaName == recordType))
                return RecordMetadata.First(r => r.SchemaName == recordType);
            throw new ArgumentOutOfRangeException("recordType",
                string.Format("There Is No {0} Defined For Record Type: {1}", typeof(RecordMetadata), recordType));
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

        public override IEnumerable<IFieldMetadata> GetFieldMetadata(string recordType)
        {
            return ((RecordMetadata)GetRecordTypeMetadata(recordType)).Fields.Union(StandardFields);
        }

        private IEnumerable<IRecord> GetRecordsOfType(string recordType)
        {
            return _records.Where(r => r.Type == recordType);
        }

        private IEnumerable<IRecord> PopulateLookups(IEnumerable<IRecord> records)
        {
            foreach (var item in records)
            {
                this.PopulateLookups(new Dictionary<string, List<Lookup>>
                {
                    { item.Type, item.GetFields().Select(f => f.Value).Where(o => o is Lookup).Cast<Lookup>().ToList() }
                }, null);
            }
            return records;
        }

        public override IRecord NewRecord(string recordType)
        {
            return new RecordObject(recordType);
        }

        public void Associate(IMany2ManyRelationshipMetadata relationship, IRecord record1, IRecord record2)
        {
            var associationRecord = new RecordObject(relationship.IntersectEntityName);
            associationRecord.SetField(relationship.Entity1IntersectAttribute, record1.Id, this);
            associationRecord.SetField(relationship.Entity2IntersectAttribute, record2.Id, this);
            _records.Add(associationRecord);
        }

        public override string Create(IRecord record, IEnumerable<string> fields)
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
            var primaryKey = GetRecordTypeMetadata(record.Type).PrimaryKeyName;
            record.SetField(primaryKey, id, this);
            record.SetField("createdon", DateTime.Now, this);
            _records.Add((RecordObject)Clone(record, null));
            return id;
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

        public override void Update(IRecord record, IEnumerable<string> fieldsToUpdate, bool bypassWorkflowsAndPlugins = false)
        {
            var savedRecord = GetActualWithoutCloning(record.Type, record.Id);
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
                    var name = referencedRecord.GetStringField(GetRecordTypeMetadata(type).PrimaryFieldSchemaName);
                    lookup.Name = name;
                }
            }
        }

        private IRecord GetActualWithoutCloning(string recordType, string id)
        {
            var recordsOfType = GetRecordsOfType(recordType);
            if (recordsOfType.Any(r => r.Id == id))
                return recordsOfType.First(r => r.Id == id);

            throw new Exception(string.Format("No Record Exists Of Type '{0}' With Id = '{1}'", recordType, id));
        }

        public override IRecord Get(string recordType, string id)
        {
            return Clone(GetActualWithoutCloning(recordType, id), null);
        }

        public override IEnumerable<IMany2ManyRelationshipMetadata> GetManyToManyRelationships(string recordType = null)
        {
            return
                Many2ManyRelationships.Where(
                    r => r.RecordType1 == recordType || r.RecordType2 == recordType);
        }

        public override IEnumerable<IRecord> GetRelatedRecords(IRecord recordToExtract,
            IMany2ManyRelationshipMetadata many2ManyRelationshipMetadata,
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
                ? many2ManyRelationshipMetadata.RecordType2
                : many2ManyRelationshipMetadata.RecordType1;
            return
                Clone(GetRecordsOfType(matchType).Where(r => intersects.Select(i => i.GetStringField(matchField)).Contains(r.Id)), null);
        }

        public override IEnumerable<IOne2ManyRelationshipMetadata> GetOneToManyRelationships(string recordType, bool onlyValidForAdvancedFind = true)
        {
            return One2ManyRelationships.Where(r => r.ReferencedEntity == recordType);
        }

        public override IEnumerable<string> GetAllRecordTypes()
        {
            return RecordMetadata.Select(r => r.SchemaName);
        }

        public override IEnumerable<ViewMetadata> GetViews(string recordType)
        {
            return ((RecordMetadata)GetRecordTypeMetadata(recordType)).Views;
        }

        public override IEnumerable<IRecord> RetrieveAllAndClauses(string recordType,
            IEnumerable<Condition> andConditions, IEnumerable<string> fields)
        {
            return Clone(GetRecordsOfType(recordType).Where(r => andConditions.All(c => c.MeetsCondition(r))), fields);
        }

        public override IEnumerable<IRecord> RetrieveAllOrClauses(string recordType, IEnumerable<Condition> orConditions,
            IEnumerable<string> fields)
        {
            return Clone(GetRecordsOfType(recordType).Where(r => orConditions.Any(c => c.MeetsCondition(r))), fields);
        }

        public override IEnumerable<IRecord> RetreiveAll(QueryDefinition query)
        {
            var results = new List<IRecord>();
            var cloneResults = Clone(GetRecordsOfType(query.RecordType).Where(r => query.RootFilter.MeetsFilter(r)), query.Fields).ToList();
            var newSorts = new List<SortExpression>(query.Sorts);
            newSorts.Reverse();
            foreach (var sort in newSorts.Take(1))
            {
                var comparer = new RecordComparer(sort.FieldName);
                cloneResults.Sort(comparer);
                if (sort.SortType == SortType.Descending)
                    cloneResults.Reverse();
            }
            return cloneResults;
        }

        public override string GetFieldAsDisplayString(string recordType, string fieldName, object fieldValue, string currencyId = null)
        {
            if (fieldValue == null)
                return null;
            if (fieldValue is string)
                return (string)fieldValue;
            if (fieldValue is Lookup)
                return ((Lookup)fieldValue).Name;
            if (this.GetFieldMetadata(fieldName, recordType).IsActivityParty())
            {
                if (fieldValue is IRecord[])
                {
                    var namesToOutput = new List<string>();
                    foreach (var party in (IRecord[])fieldValue)
                    {
                        namesToOutput.Add(party.GetLookupName("partyid"));
                    }
                    return string.Join(", ", namesToOutput.Where(f => !f.IsNullOrWhiteSpace()));
                }
            }
            return fieldValue.ToString();
        }

        public override IEnumerable<IRecord> GetFirstX(string recordType, int x, IEnumerable<string> fields, IEnumerable<Condition> conditions, IEnumerable<SortExpression> sortExpressions)
        {
            if (conditions == null)
                conditions = new Condition[0];
            var records = x > 0
                ? GetRecordsOfType(recordType).Where(r => conditions.All(c => c.MeetsCondition(r))).Take(x)
                : GetRecordsOfType(recordType).Where(r => conditions.All(c => c.MeetsCondition(r)));
            return Clone(records, fields);
        }

        private IEnumerable<IRecord> Clone(IEnumerable<IRecord> records, IEnumerable<string> fields)
        {
            var cloned = records.Select(r => Clone(r, fields)).ToArray();
            PopulateLookups(cloned);
            return cloned;
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
                    var newValue = ((IRecord[])value).Select(r => Clone(r, null)).ToArray();
                    copyTo.SetField(field, newValue, this);
                }
                else
                {
                    var mapper = new ClassSelfMapper();
                    copyTo.SetField(field, mapper.Map(copyFrom.GetField(field)), this);
                }
            }
        }

        public override void Delete(string recordType, string id, bool bypassWorkflowsAndPlugins = false)
        {
            throw new NotImplementedException();
        }
    }
}