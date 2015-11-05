using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JosephM.Core.Extentions;
using JosephM.Core.Sql;
using JosephM.Record.Extentions;
using JosephM.Record.IService;
using JosephM.Record.Metadata;
using JosephM.Record.Query;
using JosephM.Core.FieldType;
using JosephM.Record.Attributes;
using System.Collections;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization.Json;

namespace JosephM.Record.Sql
{
    public class SqlRecordMetadataService : SqlRecordService, ISqlRecordMetadataService
    {
        private SqlServerAndDbSettings Settings { get; set; }

        public IEnumerable<RecordMetadata> RecordMetadata { get; set; }

        public SqlRecordMetadataService(SqlServerAndDbSettings service, IEnumerable<RecordMetadata> recordMetadata)
            : base(service)
        {
            Settings = service;
            RecordMetadata = recordMetadata;
        }

        public void RefreshSource()
        {
            //the nonmetadata sql service checks fields in database instead of metadata
            var nonMetaDataService = new SqlRecordService(Settings);
            if (!DatabaseExists())
                CreateDatabase(Settings);

            DropAllTriggers(RecordMetadata.Select(m => m.SchemaName));
            DropAllForeignKeyConstraints(RecordMetadata.Select(m => m.SchemaName));
            DropAllIndexes(RecordMetadata.Select(m => m.SchemaName));
            //create tables with fields
            foreach (var type in RecordMetadata)
            {
                if (!nonMetaDataService.RecordTypeExists(type.SchemaName))
                {
                    nonMetaDataService.CreateOrUpdate(type);
                }
                foreach (var field in type.Fields)
                {
                    nonMetaDataService.CreateOrUpdate(field, type.SchemaName);
                }
            }

            //PRIMARY KEY INDEX
            foreach (var type in RecordMetadata)
            {
                var primaryKey = this.GetPrimaryKey(type.SchemaName);
                if (!primaryKey.IsNullOrWhiteSpace())
                {
                    var sql =
                        string.Format(
                            "CREATE UNIQUE CLUSTERED INDEX [idx_{0}_Clustered] ON {0} ( {1} ASC )", type.SchemaName, primaryKey);
                    ExecuteSql(sql);
                }
            }
            //FOREIGN KEYS
            foreach (var type in RecordMetadata)
            {
                foreach (var field in type.Fields.Where(f => f is LookupFieldMetadata).Cast<LookupFieldMetadata>())
                {
                    var foreignKeyName = string.Format("FK_{0}_{1}", type.SchemaName, field.SchemaName);
                    var sql = new StringBuilder();
                    sql.AppendLine(
                        string.Format(
                            "ALTER TABLE {0}  WITH CHECK ADD  CONSTRAINT {1} FOREIGN KEY({2})"
                            , ToIdentifier(type.SchemaName), ToIdentifier(foreignKeyName), ToIdentifier(field.SchemaName)));
                    sql.AppendLine(string.Format("REFERENCES {0} ({1})", ToIdentifier(field.ReferencedRecordType),
                        ToIdentifier(this.GetPrimaryKey(field.ReferencedRecordType))));

                    ExecuteSql(sql.ToString());

                    var postSql =
                        string.Format(
                            "ALTER TABLE {0} CHECK CONSTRAINT {1}",
                            ToIdentifier(type.SchemaName), ToIdentifier(foreignKeyName));
                    ExecuteSql(postSql);
                }
            }
            //DELETE TRIGGERS
            foreach (var type in RecordMetadata)
            {
                var keyTypes = RecordMetadata
                    .ToDictionary(r => r.SchemaName, r => r.Fields
                    .Where(f => f is LookupFieldMetadata)
                    .Cast<LookupFieldMetadata>()
                    .Where(f => f.ReferencedRecordType == type.SchemaName)
                    .Where(f => f.OnDeleteBehaviour != CascadeBehaviour.None))
                    .ToArray();
                if (keyTypes.SelectMany(r => r.Value).Any())
                {
                    var sql = new StringBuilder();
                    sql.AppendLine(string.Format("CREATE TRIGGER [trg_{0}_Delete]", type.SchemaName));
                    sql.AppendLine(string.Format("ON {0}", ToIdentifier(type.SchemaName)));
                    sql.AppendLine(string.Format("INSTEAD OF DELETE AS BEGIN SET NOCOUNT ON;"));
                    foreach (var keyType in keyTypes)
                    {
                        foreach (var key in keyType.Value)
                        {
                            if (key.OnDeleteBehaviour == CascadeBehaviour.Cascade)
                                sql.AppendLine(string.Format("DELETE FROM {0} WHERE {1} IN (SELECT Id FROM DELETED)",
                                    ToIdentifier(keyType.Key), ToIdentifier(key.SchemaName)));
                            else if (key.OnDeleteBehaviour == CascadeBehaviour.SetNull)
                                sql.AppendLine(string.Format("UPDATE {0} SET {1} = null WHERE {1} IN (SELECT Id FROM DELETED)",
                                    ToIdentifier(keyType.Key), ToIdentifier(key.SchemaName)));
                        }
                    }
                    sql.AppendLine(string.Format("DELETE FROM {0} WHERE {1} IN (SELECT Id FROM DELETED)",
                        ToIdentifier(type.SchemaName), ToIdentifier(this.GetPrimaryKey(type.SchemaName))));
                    sql.AppendLine("END");
                    ExecuteSql(sql.ToString());
                }
            }
        }

        public void DropAllForeignKeyConstraints(IEnumerable<string> tables)
        {
            var sql =
                string.Format(
                    "SELECT fkey.name as KeyName, t.name as TableName FROM sys.foreign_keys fkey INNER JOIN sys.tables t ON fkey.parent_object_id = t.object_id where t.name in ({0})",
                    string.Join(",", tables.Select(ToSqlValue)));
            var results = ExecuteSelect(sql);
            foreach (var result in results)
            {
                var keyName = result.GetFieldAsString("KeyName");
                var tableName = result.GetFieldAsString("TableName");
                if (!keyName.IsNullOrWhiteSpace() && !keyName.IsNullOrWhiteSpace())
                {
                    var dropSql = string.Format("ALTER TABLE {0} DROP CONSTRAINT {1} ", ToIdentifier(tableName), ToIdentifier(keyName));
                    ExecuteSql(dropSql);
                }
            }
        }

        public void DropAllTriggers(IEnumerable<string> tables)
        {
            var sql =
                string.Format(
                    "SELECT trg.name as TriggerName, t.name as TableName FROM sys.triggers trg INNER JOIN sys.tables t ON trg.parent_id = t.object_id where t.name in ({0})",
                    string.Join(",", tables.Select(ToSqlValue)));
            var results = ExecuteSelect(sql);
            foreach (var result in results)
            {
                var triggerName = result.GetFieldAsString("TriggerName");
                if (!triggerName.IsNullOrWhiteSpace() && !triggerName.IsNullOrWhiteSpace())
                {
                    var dropSql = string.Format("DROP TRIGGER {0} ", ToIdentifier(triggerName));
                    ExecuteSql(dropSql);
                }
            }
        }

        public void DropAllIndexes(IEnumerable<string> tables)
        {
            var sql =
                string.Format(
                    "SELECT ind.name as IndexName, t.name as TableName FROM sys.indexes ind INNER JOIN sys.tables t ON ind.object_id = t.object_id where t.name in ({0})",
                    string.Join(",", tables.Select(ToSqlValue)));
            var results = ExecuteSelect(sql);
            foreach (var result in results)
            {
                var indexName = result.GetFieldAsString("IndexName");
                var tableName = result.GetFieldAsString("TableName");
                if (!indexName.IsNullOrWhiteSpace() && !indexName.IsNullOrWhiteSpace())
                {
                    var dropSql = string.Format("DROP INDEX {0} ON {1}", ToIdentifier(indexName),
                        ToIdentifier(tableName));
                    ExecuteSql(dropSql);
                }
            }
        }

        private void CreateDatabase(SqlServerAndDbSettings settings)
        {
            SqlProvider.CreateDatabase(settings.SqlServer, settings.Database);
        }

        private bool DatabaseExists()
        {
            return SqlProvider.DatabaseExists(Settings.SqlServer, Settings.Database);
        }

        public override string Create(IRecord record, IEnumerable<string> fieldsToSet)
        {
            return Create(record, fieldsToSet, null, null);
        }

        public string Create(IRecord record, IEnumerable<string> fieldsToSet, SqlProvider sqlService, SqlTransaction transaction)
        {

            var updateFields = GetCreateOrUpdateFields(record, fieldsToSet);
            var sql = SqlProvider.GetInsertString(record.Type, updateFields, true);
            var idRow = ExecuteSelect(sql, sqlService, transaction);
            if (!idRow.Any())
                throw new NullReferenceException("Cannot determine id no identity row was returned");
            var id = idRow.First().GetFieldAsString("ID");
            return id;
        }

        protected void Update(IRecord record, IEnumerable<string> fieldsToSubmit, SqlProvider service, SqlTransaction transaction)
        {
            var sql = GetUpdateString(record, fieldsToSubmit);
            ExecuteSql(sql, service, transaction);
        }

        protected void Delete(string recordType, string id, SqlProvider service, SqlTransaction transaction)
        {
            var sql = GetDeleteString(recordType, id);
            ExecuteSql(sql, service, transaction);
        }

        public override IEnumerable<IFieldMetadata> GetFieldMetadata(string recordType)
        {
            return GetRecordMetadata(recordType).Fields;
        }

        public override IRecordTypeMetadata GetRecordTypeMetadata(string recordType)
        {
            var mt = RecordMetadata.Where(m => m.SchemaName == recordType);
            if (!mt.Any())
                throw new NullReferenceException(string.Format("No {0} has name {1}", typeof(RecordMetadata).Name, recordType));
            return mt.First();
        }

        public RecordMetadata GetRecordMetadata(string recordType)
        {
            var mt = RecordMetadata.Where(m => m.SchemaName == recordType);
            if (!mt.Any())
                throw new NullReferenceException(string.Format("No {0} has name {1}", typeof(RecordMetadata).Name, recordType));
            return mt.First();
        }

        public override IEnumerable<ViewMetadata> GetViews(string recordType)
        {
            return GetRecordMetadata(recordType).Views;
        }

        public override IRecord Get(string recordType, string id)
        {
            var primaryKey = this.GetPrimaryKey(recordType);
            if (primaryKey.IsNullOrWhiteSpace())
                throw new NullReferenceException("Error no primary key found");
            var result = GetFirstX(recordType, 1, null, new[] { new Condition(primaryKey, ConditionType.Equal, id) }, null);
            if (!result.Any())
                throw new NullReferenceException(string.Format("No {0} Found With {1} = {2}", recordType, primaryKey, id));
            return result.First();
        }



        protected override string CreateColumnSelect(IEnumerable<string> fields, string recordType)
        {
            //NOTE THIS RELIES ON THE GetJoinClause METHOD
            //need to replace select for aggregate fields
            if (fields == null)
                fields = this.GetFields(recordType);
            fields = fields.Union(new[] { this.GetPrimaryKey(recordType) });

            var fieldList = fields.Select(ToIdentifier).ToList();
            foreach (var field in GetAggregateFieldsFor(recordType, fields))
            {
                fieldList.Remove(ToIdentifier(field.SchemaName));
                fieldList.Add(string.Format("{0}.{0}", ToIdentifier(field.SchemaName)));
            }
            return string.Join(",", fieldList);
        }

        protected override string GetJoinClause(string recordType, IEnumerable<string> fields)
        {
            //NOTE THIS RELIES ON THE CreateColumnSelect METHOD
            var joinSql = new StringBuilder();
            joinSql.AppendLine(base.GetJoinClause(recordType, fields));
            var aggregateFields = GetAggregateFieldsFor(recordType, fields);

            foreach (var field in aggregateFields)
            {
                joinSql.AppendLine("left join");
                joinSql.AppendLine("(");
                joinSql.AppendLine(string.Format("    select count(*) as {0}, {1}", ToIdentifier(field.SchemaName), ToIdentifier(field.LinkedLookup)));
                joinSql.AppendLine(string.Format("    from {0}", ToIdentifier(field.LinkedType)));
                joinSql.AppendLine(string.Format("    where {0}", CreateAndClause(field.Conditions)));
                joinSql.AppendLine(string.Format("    group by {0}", ToIdentifier(field.LinkedLookup)));
                joinSql.AppendLine(string.Format(") {0} on {1}.{2} = {0}.{3}", ToIdentifier(field.SchemaName), ToIdentifier(recordType), ToIdentifier(this.GetPrimaryKey(recordType)), ToIdentifier(field.LinkedLookup)));
            }
            return joinSql.ToString();
        }

        private IEnumerable<AggregateFieldMetadata> GetAggregateFieldsFor(string recordType, IEnumerable<string> fields)
        {
            var aggregateFields = GetFieldMetadata(recordType)
                .Where(f => f is AggregateFieldMetadata)
                .Cast<AggregateFieldMetadata>();
            if (fields != null)
                aggregateFields = aggregateFields.Where(f => fields.Contains(f.SchemaName));
            return aggregateFields;
        }

        protected override IEnumerable<IRecord> ToRecords(IEnumerable<QueryRow> rows, string type)
        {
            var primaryKey = this.GetPrimaryKey(type);
            var temp = base.ToRecords(rows, type);
            foreach (var item in temp)
            {
                item.Id = item.GetStringField(primaryKey);
            }
            var dictionary = new Dictionary<string, List<Lookup>>();
            foreach (var record in temp)
            {
                foreach (var field in record.GetFieldsInEntity().ToArray())
                {
                    var value = record.GetField(field);
                    var fieldType = this.GetFieldType(field, type);
                    if (value != null)
                    {
                        if (value is DateTime)
                            record.SetField(field, ((DateTime)value).ToLocalTime(), this);
                        if (fieldType == RecordFieldType.Lookup)
                        {
                            if (!dictionary.ContainsKey(field))
                                dictionary.Add(field, new List<Lookup>());
                            var lookup = new Lookup(GetLookupTargetType(field, type), value.ToString(),
                                "Name Not Populated");
                            record.SetField(field, lookup, this);
                            dictionary[field].Add(lookup);
                        }
                        if (fieldType == RecordFieldType.Picklist)
                        {
                            record.SetField(field,
                                new PicklistOption(value.ToString(), GetOptionLabel(value.ToString(), field, type)),
                                this);
                        }
                    }
                }
            }
            this.PopulateLookups(dictionary, null);
            return temp;
        }

        public override string GetLookupTargetType(string fieldName, string recordType)
        {
            var fieldMetadata = this.GetFieldMetadata(fieldName, recordType);
            if (!(fieldMetadata is LookupFieldMetadata))
                throw new Exception(string.Format("Field {0} in type {1} is not of type {2}", fieldName, recordType, typeof(LookupFieldMetadata).Name));
            return ((LookupFieldMetadata)fieldMetadata).ReferencedRecordType;
        }

        public string SaveObject(object instance)
        {
            using (var service = CreateSqlService())
            {
                var record = MapIntoRecord(instance);
                using (var transaction = service.BeginTransacton())
                {
                    try
                    {
                        if (record.Id.IsNullOrWhiteSpace())
                            record.Id = Create(record, null, service, transaction);
                        else
                            Update(record, null, service, transaction);

                        SaveEnumerableProperties(instance, record, service, transaction);
                        transaction.Commit();
                    }
                    catch (Exception)
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
                return record.Id;
            }
        }

        private void SaveEnumerableProperties(object instance, IRecord record, SqlProvider service, SqlTransaction transaction)
        {
            var type = instance.GetType();
            foreach (var property in type.GetProperties())
            {
                var enumerableAttributes = property.GetCustomAttributes<EnumerableRecordFieldMap>(true);
                if (enumerableAttributes.Any())
                {
                    foreach (var fieldAttribute in enumerableAttributes)
                    {
                        var propertyType = property.PropertyType.GenericTypeArguments.ElementAt(0);
                        var propertyValue = instance.GetPropertyValue(property.Name) as IEnumerable;
                        if (propertyValue == null)
                            throw new NullReferenceException(
                                string.Format("Enumerable property {0} is null and required to save", property.Name));
                        var linkedType = GetRecordType(propertyType);
                        var newLinkedRecords = new Dictionary<object, IRecord>();
                        foreach (var item in propertyValue)
                        {
                            var linkedRecord = MapIntoRecord(item);
                            linkedRecord.SetLookup(fieldAttribute.LookupField, record.Id, record.Type);
                            newLinkedRecords.Add(item, linkedRecord);
                        }
                        var conditions = new[] { new Condition(fieldAttribute.LookupField, ConditionType.Equal, record.Id) };
                        var query = GetFirstXQuery(linkedType, -1, new string[0], conditions, null, FilterOperator.And);
                        var oldLinkedIds = ToRecords(ExecuteSelect(query, service, transaction), linkedType).Select(r => r.Id);
                        var newIds = newLinkedRecords.Select(r => r.Value.Id).ToArray();
                        var toDelete = oldLinkedIds.Except(newIds);
                        foreach (var linkedRecord in newLinkedRecords)
                        {
                            if (linkedRecord.Value.Id.IsNullOrWhiteSpace())
                                linkedRecord.Value.Id = Create(linkedRecord.Value, null, service, transaction);
                            else
                                Update(linkedRecord.Value, null, service, transaction);

                            SaveEnumerableProperties(linkedRecord.Key, linkedRecord.Value, service, transaction);
                        }
                        foreach (var deleteMe in toDelete)
                        {
                            Delete(linkedType, deleteMe, service, transaction);
                        }
                    }
                }
            }
        }

        public string SaveTypedObject(object instance, string id, IStoredObjectFields fieldConfig)
        {
            if (instance == null)
                throw new ArgumentNullException("instance");

            var record = NewRecord(fieldConfig.RecordType);
            //think will serialise it
            var type = instance.GetType();
            var serializer = new DataContractJsonSerializer(type);
            string jsonString = null;
            using (var stream = new MemoryStream())
            {
                serializer.WriteObject(stream, instance);
                jsonString = Encoding.Default.GetString(stream.ToArray());
            }
            var nameField = this.GetPrimaryField(fieldConfig.RecordType);
            if (!nameField.IsNullOrWhiteSpace())
                record.SetField(nameField, instance.ToString(), this);
            record.SetField(fieldConfig.ValueField, jsonString, this);
            record.SetField(fieldConfig.AssemblyField, type.Assembly.GetName().Name, this);
            record.SetField(fieldConfig.TypeQualfiedNameField, type.FullName, this);
            record.SetField(fieldConfig.TypeField, type.Name, this);
            if (id.IsNullOrWhiteSpace())
                return Create(record);
            else
            {
                record.Id = id;
                Update(record, null);
                return id;
            }
        }

        private IRecord MapIntoRecord(object instance)
        {
            if (instance == null)
                throw new ArgumentNullException("instance");
            var type = instance.GetType();
            var recordType = GetRecordType(type);
            var record = NewRecord(recordType);
            var primaryKey = this.GetPrimaryKey(recordType);
            var primaryField = this.GetPrimaryField(recordType);
            foreach (var property in type.GetProperties())
            {
                var fieldAttributes = property.GetCustomAttributes<RecordFieldMap>(true);
                if (fieldAttributes.Any())
                {
                    foreach (var fieldAttribute in fieldAttributes)
                    {
                        var fieldValue = instance.GetPropertyValue(property.Name);
                        if (fieldValue == null)
                            record.SetField(fieldAttribute.FieldName, null, this);
                        else if (fieldValue is PicklistOption)
                            record.SetField(fieldAttribute.FieldName, ((PicklistOption)fieldValue).Key, this);
                        else if (fieldValue is Lookup)
                            record.SetField(fieldAttribute.FieldName, ((Lookup)fieldValue).Id, this);
                        else if (fieldValue.GetType().IsEnum)
                            record.SetField(fieldAttribute.FieldName, (int)fieldValue, this);
                        else if (fieldValue is DateTime)
                            record.SetField(fieldAttribute.FieldName, (DateTime)fieldValue, this);
                        else
                            record.SetField(fieldAttribute.FieldName, fieldValue.ToString(), this);
                        if (fieldAttribute.FieldName == primaryKey)
                            record.Id = fieldValue != null ? fieldValue.ToString() : null;
                    }

                }
            }
            if (!primaryField.IsNullOrWhiteSpace() && !record.ContainsField(primaryField))
                record.SetField(primaryField, instance.ToString(), this);
            return record;
        }

        public T LoadToObject<T>(string id)
        {
            return (T)LoadToObject(id, typeof(T));
        }

        public object LoadToObject(string id, Type type)
        {
            var recordType = GetRecordType(type);
            var record = Get(recordType, id);
            var instances = LoadToObjects(type, new[] { record });
            var enumerator = ((IEnumerable)instances).GetEnumerator();
            enumerator.MoveNext();
            return enumerator.Current;
        }

        private static string GetRecordType(Type type)
        {
            var recordTypeAttribute = type.GetCustomAttribute<RecordTypeMap>(true);
            if (recordTypeAttribute == null)
                throw new NullReferenceException(string.Format("Type {0} does not have a {1} attribute", type.Name,
                    typeof(RecordTypeMap).Name));
            var recordType = recordTypeAttribute.RecordType;
            return recordType;
        }

        private object LoadToObjects(Type type, IEnumerable<IRecord> records)
        {
            if (!type.HasParameterlessConstructor())
                throw new NullReferenceException(string.Format("Type {0} is required to have a parameterless constructor", type.Name));
            var instances = new List<object>();
            foreach (var record in records)
            {
                var instance = type.CreateFromParameterlessConstructor();
                instances.Add(instance);
                foreach (var property in type.GetProperties())
                {
                    var fieldAttributes = property.GetCustomAttributes<RecordPropertyMap>();
                    if (fieldAttributes.Any())
                    {
                        foreach (var fieldAttribute in fieldAttributes)
                        {
                            if (fieldAttribute is EnumerableRecordFieldMap)
                            {
                                var enumerableMap = (EnumerableRecordFieldMap)fieldAttribute;
                                var linkedType = property.PropertyType.GenericTypeArguments.ElementAt(0);
                                var linkedRecordType = GetRecordType(linkedType);
                                var linkedRecords = GetFirstX(linkedRecordType, -1, null, new[] { new Condition(enumerableMap.LookupField, ConditionType.Equal, record.Id) }, null);
                                var linkedObjects = LoadToObjects(linkedType, linkedRecords);
                                instance.SetPropertyValue(property.Name, linkedObjects);
                            }
                            else if (fieldAttribute is RecordFieldMap)
                            {
                                var fieldMap = (RecordFieldMap)fieldAttribute;
                                var fieldValue = record.GetField(fieldMap.FieldName);
                                if (fieldValue == null)
                                    instance.SetPropertyValue(property.Name, fieldValue);
                                else
                                {
                                    var propertyType = property.PropertyType;
                                    if (propertyType.Name == "Nullable`1")
                                        propertyType = propertyType.GetGenericArguments()[0];
                                    if (propertyType == typeof(string))
                                        instance.SetPropertyValue(property.Name, fieldValue.ToString());
                                    else if (propertyType == typeof(Lookup))
                                    {
                                        instance.SetPropertyValue(property.Name, fieldValue);
                                    }
                                    else if (propertyType == typeof(RecordType))
                                    {
                                        var lookup = new RecordType(fieldValue.ToString(), fieldValue.ToString());
                                        instance.SetPropertyValue(property.Name, lookup);
                                    }
                                    else if (propertyType == typeof(RecordField))
                                    {
                                        var field = new RecordField(fieldValue.ToString(), fieldValue.ToString());
                                        instance.SetPropertyValue(property.Name, field);
                                    }
                                    else if (propertyType.IsEnum)
                                    {
                                        var field = Enum.Parse(propertyType, fieldValue.ToString());
                                        instance.SetPropertyValue(property.Name, field);
                                    }
                                    else if (propertyType == typeof(int))
                                    {
                                        instance.SetPropertyValue(property.Name, Convert.ToInt32(fieldValue));
                                    }
                                    else
                                        instance.SetPropertyValue(property.Name, fieldValue);

                                }
                            }
                            else
                                throw new NotImplementedException(string.Format("Map for type {0} is not implemented", fieldAttribute.GetType().Name));
                        }
                    }
                }
            }
            return type.ToNewTypedEnumerable(instances);
        }

        protected override IDictionary<string, object> GetCreateOrUpdateFields(IRecord record, IEnumerable<string> fieldToUpdate)
        {
            var fields = base.GetCreateOrUpdateFields(record, fieldToUpdate);
            var primaryKey = this.GetPrimaryKey(record.Type);
            if (fields.ContainsKey(primaryKey))
                fields.Remove(primaryKey);
            foreach (var field in fields.Keys.ToArray())
            {
                var value = fields[field];
                if (value is DateTime)
                    fields[field] = ((DateTime)value).ToUniversalTime();
            }
            return fields;
        }

        protected override string ToSqlValue(object clrValue)
        {
            return base.ToSqlValue(clrValue is DateTime ? ((DateTime)clrValue).ToUniversalTime() : clrValue);
        }

        public override IEnumerable<PicklistOption> GetPicklistKeyValues(string fieldName, string recordType, string dependentValue, IRecord record)
        {
            var fieldMetadata = this.GetFieldMetadata(fieldName, recordType);
            var picklist = fieldMetadata as PicklistFieldMetadata;
            if (picklist == null)
                throw new NullReferenceException(string.Format("Error {0} metadata object for type {1} not of type {2}", fieldName, recordType, typeof(PicklistFieldMetadata)));
            return picklist.PicklistOptions;
        }

        private string GetOptionLabel(string optionKey, string fieldName, string recordType)
        {
            var options = this.GetPicklistKeyValues(fieldName, recordType).Where(kv => kv.Key == optionKey);
            return options.Any() ? options.First().Value : string.Format("Unmatched Option! ({0})", optionKey);
        }
    }
}
