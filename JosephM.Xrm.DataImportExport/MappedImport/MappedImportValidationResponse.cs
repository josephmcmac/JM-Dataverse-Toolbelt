using JosephM.Core.Attributes;
using System;
using System.Collections.Generic;

namespace JosephM.Xrm.DataImportExport.MappedImport
{
    [Instruction("Warning - potential errors when validating the data for import. Please review before proceeding with the import")]
    public class MappedImportValidationResponse
    {
        private readonly List<MappedImportValidationResponseError> _errors = new List<MappedImportValidationResponseError>();

        public void AddResponseItem(MappedImportValidationResponseError responseItem)
        {
            _errors.Add(responseItem);
        }

        public void AddResponseItems(IEnumerable<MappedImportValidationResponseError> responseItems)
        {
            _errors.AddRange(responseItems);
        }

        [AllowDownload]
        public IEnumerable<MappedImportValidationResponseError> ResponseItems
        {
            get { return _errors; }
        }

        public class MappedImportValidationResponseError
        {
            [GridWidth(125)]
            [DisplayOrder(10)]
            public int? RowNumber { get; }
            [DisplayOrder(20)]
            public string TargetType { get; }
            [DisplayOrder(30)]
            public string TargetField { get; }
            [DisplayOrder(40)]
            [PropertyInContextByPropertyNotNull(nameof(Name))]
            public string Name { get; }
            [DisplayOrder(50)]
            public string StringValue { get; }
            [DisplayOrder(60)]
            [GridWidth(400)]
            public string Message { get; }
            [Hidden]
            public Exception Exception { get; }

            public MappedImportValidationResponseError(string message, Exception ex)
            {
                Message = message;
                Exception = ex;
            }

            public MappedImportValidationResponseError(int? rowNumber, string targetType, string targetField, string name, string stringValue, string message, Exception ex)
            {
                RowNumber = rowNumber;
                TargetType = targetType;
                TargetField = targetField;
                Name = name;
                StringValue = stringValue;
                Message = message;
                Exception = ex;
            }
        }
    }
}