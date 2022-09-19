using JosephM.Core.Extentions;
using JosephM.Core.FieldType;
using JosephM.Core.Service;
using JosephM.Core.Sql;
using JosephM.Record.Extentions;
using JosephM.Record.IService;
using JosephM.Record.Metadata;
using JosephM.Record.Query;
using JosephM.Record.Service;
using JosephM.Record.Sql;
using JosephM.Spreadsheet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace JosephM.Record.Csv
{
    public class CsvRecordService : RecordServiceBase
    {
        private string CsvNamePartNoExtention
        {
            get { return Path.GetFileNameWithoutExtension(CsvNameQualified); }
        }

        private string CsvNameQualified
        {
            get { return FileReference.FileName; }
        }

        private FileReference FileReference { get; set; }

        public CsvRecordService(CsvFileConnection csvFileConnection)
            : this(csvFileConnection.File)
        {
        }

        public CsvRecordService(FileReference fileReference)
        {
            FileReference = fileReference;
        }

        public CsvRecordService(string csvName)
            : this( new FileReference(csvName))
        {
        }

        private object _lockObject = new object();
        public override void ClearCache()
        {
            lock (_lockObject)
            {
                _fieldMetadata = null;
            }
        }

        private IEnumerable<IFieldMetadata> _fieldMetadata;
        public override IEnumerable<IFieldMetadata> GetFieldMetadata(string recordType)
        {
            lock (_lockObject)
            {
                if (_fieldMetadata == null)
                {
                    var fields = new List<IFieldMetadata>();
                    var columns = CsvUtility.GetColumns(CsvNameQualified);
                    foreach (var column in columns)
                    {
                        fields.Add(new StringFieldMetadata(recordType, column, column?.Replace("#", ".")));
                    }
                    _fieldMetadata = fields;
                }
                return _fieldMetadata;
            }
        }

        public override IRecordTypeMetadata GetRecordTypeMetadata(string recordType)
        {
            var label = recordType;
            if (label.EndsWith(".csv"))
                label = label.Substring(0, label.Length - 4);
            return new RecordMetadata() { SchemaName = recordType, DisplayName = label, CollectionName = label };
        }

        public override IEnumerable<string> GetAllRecordTypes()
        {
            return new[] { CsvUtility.GetTableName(CsvNameQualified) };
        }

        private string GetRecordType()
        {
            return GetAllRecordTypes().First();
        }

        public override IsValidResponse VerifyConnection()
        {
            var response = new IsValidResponse();
            try
            {
                var recordType = GetRecordType();
                var fields = this.GetFields(recordType);
                var invalidFieldStarts = string.Format("{0}#csv#", CsvNamePartNoExtention);
                var invalidFields = fields.Where(s => s.StartsWith(invalidFieldStarts));
                if (invalidFields.Any())
                    response.AddInvalidReason(string.Format("Invalid field names were found. Check the file for duplicate columns or rename these fields. The invalid columns names were {0}", string.Join(",", invalidFields)));
            }
            catch (Exception ex)
            {
                response.AddInvalidReason("Error connecting to CSV file: " + ex.DisplayString());
            }
            return response;
        }

        public override IRecord NewRecord(string recordType)
        {
            throw new NotImplementedException();
        }

        public override IRecord Get(string recordType, string id)
        {
            throw new NotImplementedException();
        }

        public override void Update(IRecord record, IEnumerable<string> fieldToCommit)
        {
            throw new NotImplementedException();
        }

        public override string Create(IRecord record, IEnumerable<string> fieldToSet)
        {
            throw new NotImplementedException();
        }

        public override void Delete(string recordType, string id)
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<IRecord> GetFirstX(string type, int x, IEnumerable<string> fields, IEnumerable<Condition> conditions, IEnumerable<SortExpression> sort)
        {
            if (conditions != null && conditions.Any())
            {
                throw new NotImplementedException("conditions are not implemented for csv service queries");
            }
            if (sort != null && sort.Any())
            {
                throw new NotImplementedException("sorts are not implemented for csv service queries");
            }

            return CsvUtility.SelectAllRows(CsvNameQualified)
                .Select(qr => new CsvRecord(type, qr))
                .ToArray();
        }
    }
}