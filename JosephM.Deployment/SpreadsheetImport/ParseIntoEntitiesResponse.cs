using JosephM.Core.Attributes;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;

namespace JosephM.Deployment.SpreadsheetImport
{
    [Instruction("Errors Were Encountered Parsing The Spreadsheet Data. Import Of This Data Will Fail Unless These Errors Are Fixed")]
    public class ParseIntoEntitiesResponse
    {
        private List<Entity> _parsedEntities = new List<Entity>();
        public void AddEntities(IEnumerable<Entity> entitiesToAdd)
        {
            _parsedEntities.AddRange(entitiesToAdd);
        }

        public IEnumerable<Entity> GetParsedEntities()
        { return _parsedEntities; }

        private readonly List<ParseIntoEntitiesError> _errors = new List<ParseIntoEntitiesError>();

        public void AddResponseItem(ParseIntoEntitiesError responseItem)
        {
            _errors.Add(responseItem);
        }

        public void AddResponseItems(IEnumerable<ParseIntoEntitiesError> responseItems)
        {
            _errors.AddRange(responseItems);
        }

        public IEnumerable<ParseIntoEntitiesError> ResponseItems
        {
            get { return _errors; }
        }

        public class ParseIntoEntitiesError
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

            public ParseIntoEntitiesError(string message, Exception ex)
            {
                Message = message;
                Exception = ex;
            }

            public ParseIntoEntitiesError(int? rowNumber, string targetType, string targetField, string name, string stringValue, string message, Exception ex)
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