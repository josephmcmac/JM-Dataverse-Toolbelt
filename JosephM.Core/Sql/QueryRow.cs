﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using JosephM.Core.Extentions;

namespace JosephM.Core.Sql
{
    public class QueryRow
    {
        private readonly DataRow _row;

        public QueryRow(DataRow row)
        {
            _row = row;
        }

        public object GetField(string fieldName)
        {
            if (!IsDbNull(fieldName))
            {
                return _row[fieldName];
            }
            return null;
        }

        public bool GetFieldAsBoolean(string fieldName)
        {
            if (!IsDbNull(fieldName))
            {
                var value = _row[fieldName];
                if (value is bool && (bool) value)
                    return true;

                var toString = value.ToString().ToLower();
                if (toString == "1" || toString == "true")
                    return true;
            }
            return false;
        }

        public string GetFieldAsString(string fieldName)
        {
            if (!IsDbNull(fieldName))
                return _row[fieldName].ToString();

            return null;
        }

        public int GetFieldAsInteger(string fieldName)
        {
            if (!IsDbNull(fieldName))
            {
                var rawValue = GetFieldAsString(fieldName);
                double output = 0;
                if (double.TryParse(GetFieldAsString(fieldName), out output))
                {
                    return Convert.ToInt32(output);
                }
                throw new Exception($"Could not parse int for value {rawValue} in field {fieldName}");
            }

            throw new NotSupportedException("cannot parse integer from null: " + fieldName);
        }

        public decimal GetFieldAsDecimal(string fieldName)
        {
            if (!IsDbNull(fieldName))
                return Decimal.Parse(GetFieldAsString(fieldName));

            throw new NotSupportedException("cannot parse decimal from null: " + fieldName);
        }

        private bool IsDbNull(string fieldName)
        {
            return _row[fieldName] is DBNull;
        }

        public IEnumerable<KeyValuePair<string, string>> GetStringToKeyValues(string fieldName)
        {
            var response = new List<KeyValuePair<string, string>>();
            if (!IsDbNull(fieldName))
            {
                var asString = GetFieldAsString(fieldName);
                var stringParse1 = asString.Split(';');
                foreach (var s in stringParse1)
                {
                    var stringparse2 = s.Split(':');
                    if (stringparse2.Length == 2)
                    {
                        response.Add(new KeyValuePair<string, string>(stringparse2[0], stringparse2[1]));
                    }
                }
            }
            return response;
        }

        public T GetFieldAsEnum<T>(string fieldName)
            where T : struct
        {
            if (!IsDbNull(fieldName))
            {
                var stringValue = GetFieldAsString(fieldName);
                try
                {
                    return stringValue.ParseEnum<T>();
                }
                catch (Exception)
                {
                    var items = new List<string>();
                    foreach(var item in typeof(T).GetEnumValues())
                    {
                        items.Add(item.ToString());
                    }
                    throw new Exception($"The Value {stringValue} Could Not Be Parsed To {typeof(T).Name}. Valid Values Are {items.OrderBy(s => s).JoinGrammarAnd()}");
                }
            }
            else
                return default(T);
        }

        public double GetFieldAsDouble(string fieldName)
        {
            if (!IsDbNull(fieldName))
                return Double.Parse(GetFieldAsString(fieldName));

            throw new NotSupportedException("cannot parse double from null: " + fieldName);
        }

        public IEnumerable<string> GetColumnNames()
        {
            var columns = new List<string>();
            foreach (DataColumn column in _row.Table.Columns)
            {
                columns.Add(column.ColumnName);
            }
            return columns;
        }

        public int Index
        {
            get { return _row.Table.Rows.IndexOf(_row); }
        }
    }
}