using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JosephM.Core.Extentions;
using JosephM.Core.FieldType;
using JosephM.Core.Service;
using JosephM.Core.Sql;
using JosephM.Record.Extentions;
using JosephM.Record.IService;
using JosephM.Record.Metadata;
using JosephM.Record.Query;
using JosephM.Record.Service;

namespace JosephM.Record.Sql
{
    public abstract class SqlRecordServiceBase : RecordServiceBase
    {
        protected abstract IEnumerable<QueryRow> ExecuteSelect(string selectQuery);

        protected virtual IEnumerable<IRecord> ToRecords(IEnumerable<QueryRow> rows, string type)
        {
            return rows == null
                ? null
                : rows.Select(r => new SqlRecord(type, r)).ToArray();
        }

        public override IEnumerable<IRecord> GetFirstX(string recordType, int x, IEnumerable<string> fields, IEnumerable<Condition> conditions, IEnumerable<SortExpression> sort)
        {
            return GetFirstX(recordType, x, fields, conditions, sort, FilterOperator.And);
        }

        public IEnumerable<IRecord> GetFirstX(string recordType, int x, IEnumerable<string> fields, IEnumerable<Condition> conditions, IEnumerable<SortExpression> sort, FilterOperator conditionOperator)
        {
            var sql = GetFirstXQuery(recordType, x, fields, conditions, sort, conditionOperator);
            return ToRecords(ExecuteSelect(sql), recordType);
        }

        protected string GetFirstXQuery(string recordType, int x, IEnumerable<string> fields, IEnumerable<Condition> conditions, IEnumerable<SortExpression> sort,
            FilterOperator conditionOperator)
        {
            var sql = string.Format("select {0} {1} from {2} {3} where {4} order by {5}"
                , x > 0 ? " top " + x : null, CreateColumnSelect(fields, recordType), ToIdentifier(recordType),
                GetJoinClause(recordType, fields),
                conditionOperator == FilterOperator.And ? CreateAndClause(conditions) : CreateOrClause(conditions),
                CreateSortClause(sort));
            return sql;
        }

        protected virtual string GetJoinClause(string recordType, IEnumerable<string> fields)
        {
            return string.Empty;
        }

        public override IEnumerable<IRecord> RetrieveAllOrClauses(string recordType, IEnumerable<Condition> orConditions, IEnumerable<string> fields)
        {
            return GetFirstX(recordType, -1, fields, orConditions, null, FilterOperator.Or);
            //review for heap of conditions
        }

        protected static string ToIdentifier(string name)
        {
            return string.Format("[{0}]", name);
        }

        protected virtual string ToSqlValue(object clrValue)
        {
            return SqlProvider.ToSqlString(clrValue);
        }

        protected virtual string CreateColumnSelect(IEnumerable<string> fields, string recordType)
        {
            return fields == null || !fields.Any()
                ? "*"
                : string.Join(",", fields.Select(ToIdentifier));
        }

        private string CreateSortClause(IEnumerable<SortExpression> sort)
        {
            if (sort == null || !sort.Any())
                return " 1 ";
            return string.Join(",",
                sort.Select(
                    s =>
                        string.Format("{0} {1}", ToIdentifier(s.FieldName),
                            s.SortType == SortType.Descending ? " desc " : " ")));
        }

        protected string CreateAndClause(IEnumerable<Condition> conditions)
        {
            if (conditions == null || !conditions.Any())
                return " 1 = 1 ";
            return string.Format("({0})", string.Join(" and ",
                conditions.Select(ToConditionClause)));
        }

        protected string CreateOrClause(IEnumerable<Condition> conditions)
        {
            if (conditions == null || !conditions.Any())
                return " 1 = 1 ";
            return string.Format("({0})", string.Join(" or ",
                conditions.Select(ToConditionClause)));
        }

        private string ToConditionClause(Condition condition)
        {
            switch (condition.ConditionType)
            {
                case ConditionType.Equal:
                    {
                        if (condition.Value == null)
                            return string.Format(" {0} is null ", ToIdentifier(condition.FieldName));
                        return string.Format(" {0} = {1} ", ToIdentifier(condition.FieldName), ToSqlValue(condition.Value));
                    }
                case ConditionType.Null:
                    {
                        return string.Format(" {0} is null ", ToIdentifier(condition.FieldName));
                    }
                case ConditionType.NotNull:
                    {
                        return string.Format(" {0} is not null ", ToIdentifier(condition.FieldName));
                    }
                case ConditionType.BeginsWith:
                    {
                        return string.Format(" {0} like {1} ", ToIdentifier(condition.FieldName), ToSqlValue(condition.Value + "%"));
                    }
                case ConditionType.In:
                    {
                        if (!(condition.Value is IEnumerable))
                            throw new NotSupportedException(string.Format("{0} {1} was specified but the value is not of type {2}", typeof(ConditionType).Name, ConditionType.In, typeof(Enumerable).Name));
                        var objects = ((IEnumerable)condition.Value).Cast<object>().ToList();
                        return string.Format(" {0} in ({1}) ", ToIdentifier(condition.FieldName), string.Join(",", objects.Select(ToSqlValue)));
                    }
                case ConditionType.GreaterEqual:
                    {
                        return string.Format(" {0} >= {1} ", ToIdentifier(condition.FieldName), ToSqlValue(condition.Value));
                    }
                case ConditionType.LessEqual:
                    {
                        return string.Format(" {0} <= {1} ", ToIdentifier(condition.FieldName), ToSqlValue(condition.Value));
                    }
                case ConditionType.NotEqual:
                    {
                        return string.Format(" {0} <> {1} ", ToIdentifier(condition.FieldName), ToSqlValue(condition.Value));
                    }
            }
            throw new NotImplementedException(string.Format("{0} not implemented", condition.ConditionType));
        }

        public override IRecordTypeMetadata GetRecordTypeMetadata(string recordType)
        {
            return new RecordMetadata() { SchemaName = recordType, DisplayName = recordType, CollectionName = recordType + "s" };
        }

        public override void Update(IRecord record, IEnumerable<string> changedPersistentFields)
        {
            var sql = GetUpdateString(record, changedPersistentFields);
            ExecuteSql(sql);
        }

        protected string GetUpdateString(IRecord record, IEnumerable<string> changedPersistentFields)
        {
            if (record.Id.IsNullOrWhiteSpace())
                throw new NullReferenceException("Id is empty");
            var idColumn = record.IdTargetFieldOverride;
            if (idColumn.IsNullOrWhiteSpace())
                idColumn = this.GetPrimaryKey(record.Type);
            if (idColumn.IsNullOrWhiteSpace())
                throw new NullReferenceException(string.Format("Error determining primary key for type {0}. idColumn is empty",
                    record.Type));
            var updateFields = GetCreateOrUpdateFields(record, changedPersistentFields);
            var sql = SqlProvider.GetUpdateString(record.Type, updateFields, idColumn,
                record.Id);
            return sql;
        }

        public string Create(IRecord record)
        {
            return Create(record, null);
        }

        public override string Create(IRecord record, IEnumerable<string> fieldsToSet)
        {
            var updateFields = GetCreateOrUpdateFields(record, fieldsToSet);
            var sql = SqlProvider.GetInsertString(record.Type, updateFields, false);
            ExecuteSql(sql);
            return null;
        }

        protected virtual IDictionary<string, object> GetCreateOrUpdateFields(IRecord record, IEnumerable<string> fieldToUpdate)
        {
            return fieldToUpdate == null
                ? record.GetFields()
                : record.GetFields().Where(kv => fieldToUpdate.Contains(kv.Key)).ToDictionary(kv => kv.Key, kv => kv.Value);
        }

        public override IRecord NewRecord(string recordType)
        {
            return new SqlRecord(recordType);
        }

        public abstract void ExecuteSql(string sql);

        public override IEnumerable<IRecord> GetLinkedRecords(string linkedRecordType, string recordTypeFrom, string linkedRecordLookup,
            string recordFromId)
        {
            return GetFirstX(linkedRecordType, -1, null, new[]
            {
                new Condition(linkedRecordLookup, ConditionType.Equal, recordFromId),
            }, null);
        }

        public override void Delete(string recordType, string id)
        {
            var sql = GetDeleteString(recordType, id);
            ExecuteSql(sql);
        }

        protected string GetDeleteString(string recordType, string id)
        {
            if (id.IsNullOrWhiteSpace())
                throw new ArgumentNullException("id");
            var sql = string.Format("delete from {0} where {1} = {2}", ToIdentifier(recordType),
                ToIdentifier(this.GetPrimaryKey(recordType)), ToSqlValue(id));
            return sql;
        }

        public override IRecord Get(string recordType, string id)
        {
            throw new NotImplementedException();
        }
    }
}