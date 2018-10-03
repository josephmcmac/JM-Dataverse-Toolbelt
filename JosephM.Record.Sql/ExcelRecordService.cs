using JosephM.Core.Extentions;
using JosephM.Core.FieldType;
using JosephM.Core.Service;
using JosephM.Core.Sql;
using JosephM.Record.Extentions;
using JosephM.Record.IService;
using JosephM.Record.Metadata;
using JosephM.Spreadsheet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace JosephM.Record.Sql
{
    public class ExcelRecordService : SqlRecordServiceBase
    {
        private string ExcelFolder
        {
            get { return Path.GetDirectoryName(ExcelNameQualified); }
        }

        private string ExcelNamePart
        {
            get { return Path.GetFileName(ExcelNameQualified); }
        }

        private string ExcelNamePartNoExtention
        {
            get { return Path.GetFileNameWithoutExtension(ExcelNameQualified); }
        }

        private string ExcelNameQualified
        {
            get { return FileReference.FileName; }
        }

        private FileReference FileReference { get; set; }

        public ExcelRecordService(ExcelFileConnection ExcelFileConnection)
            : this(ExcelFileConnection.File)
        {
        }

        public ExcelRecordService(FileReference fileReference)
        {
            FileReference = fileReference;
        }

        public ExcelRecordService(string ExcelName)
            : this( new FileReference(ExcelName))
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
                    var columns = ExcelUtility.GetExcelColumnNames(ExcelNameQualified);
                    foreach(var typeFields in columns)
                    {
                        foreach(var column in typeFields.Value)
                        {
                            fields.Add(new StringFieldMetadata(typeFields.Key, column, column?.Replace("#",".")));
                        }
                    }
                    _fieldMetadata = fields;
                }
                return _fieldMetadata.Where(f => f.RecordType == recordType).ToArray();
            }
        }
        private IEnumerable<string> _recordTypes;
        public override IEnumerable<string> GetAllRecordTypes()
        {
            if (_recordTypes == null)
                _recordTypes = ExcelUtility.GetExcelTabNames(ExcelNameQualified);
            return _recordTypes;
        }

        public override IRecordTypeMetadata GetRecordTypeMetadata(string recordType)
        {
            var label = recordType;
            if (label.Contains(" ") && label.StartsWith("'") && label.EndsWith("'"))
                label = label.Substring(1, label.Length - 2);
            label = label.EndsWith("$") ? label.Substring(0, label.Length - 1) : label;
            return new RecordMetadata() { SchemaName = recordType, DisplayName = label, CollectionName = label };
        }

        protected override IEnumerable<QueryRow> ExecuteSelect(string selectQuery)
        {
            return ExcelUtility.SelectFromExcel(ExcelNameQualified, selectQuery);
        }

        public override void ExecuteSql(string sql)
        {
            ExcelUtility.SelectFromExcel(ExcelNameQualified, sql);
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
                var invalidFieldStarts = string.Format("{0}#Excel#", ExcelNamePartNoExtention);
                var invalidFields = fields.Where(s => s.StartsWith(invalidFieldStarts));
                if (invalidFields.Any())
                    response.AddInvalidReason(string.Format("Invalid field names were found. Check the file for duplicate columns or rename these fields. The invalid columns names were {0}", string.Join(",", invalidFields)));
            }
            catch (Exception ex)
            {
                response.AddInvalidReason("Error connecting to Excel file: " + ex.DisplayString());
            }
            return response;
        }
    }
}