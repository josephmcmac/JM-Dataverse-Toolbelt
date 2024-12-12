using JosephM.Core.FieldType;
using JosephM.Core.Service;
using JosephM.Record.Extentions;
using JosephM.Record.Metadata;
using JosephM.Record.Xrm.XrmRecord;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JosephM.CodeGenerator
{
    public abstract class CodeGeneratorServiceBase<TReq, Tres, TResItem>
        : ServiceBase<TReq, Tres, TResItem>
        where TReq : ServiceRequestBase
        where Tres : ServiceResponseBase<TResItem>, new()
        where TResItem : ServiceResponseItem
    {
        public CodeGeneratorServiceBase(XrmRecordService service)
        {
            Service = service;
        }

        public XrmRecordService Service { get; set; }

        private static IEnumerable<RecordFieldType> OptionSetTypes
        {
            get { return new[] { RecordFieldType.Picklist, RecordFieldType.Status, RecordFieldType.State, }; }
        }

        private static IEnumerable<string> KeyWords
        {
            get { return new[] { "abstract", "event", "namespace", "equals", "class", "string", "int", "object", "decimal", "double", "float", "override", "virtual", "constant", "ref", "out", "true", "false", "is", "as", "default" }; }
        }

        private static string _basicAlphanumeric = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";

        protected static string CreateCodeLabel(string rawLabel)
        {
            var stringBuilder = new StringBuilder();
            if (!string.IsNullOrWhiteSpace(rawLabel))
            {
                for (var i = 0; i < rawLabel.Length; i++)
                {
                    var c = rawLabel.ElementAt(i);
                    if (c != '_' && !_basicAlphanumeric.Contains(c))
                    {
                        continue;
                    }
                    if (stringBuilder.Length == 0 && char.IsDigit(c))
                    {
                        stringBuilder.Append("_");
                        stringBuilder.Append(c);
                    }
                    else
                    {
                        stringBuilder.Append(c);
                    }
                }
            }
            var result = stringBuilder.ToString();
            if (KeyWords.Contains(result.ToLower()))
            {
                result = result + "_";
            }
            return result;
        }

        protected bool IsValidForCode(string recordType)
        {
            return
                !string.IsNullOrWhiteSpace(CreateCodeLabel(Service.GetDisplayName(recordType)))
                && Service.GetFields(recordType).Any(f => IsValidForOptionSetCode(f, recordType));
        }

        protected bool IsValidForOptionSetCode(string field, string recordType)
        {
            return
                OptionSetTypes.Contains(Service.GetFieldType(field, recordType))
                && !string.IsNullOrWhiteSpace(CreateCodeLabel(Service.GetFieldLabel(field, recordType)))
                && (Service.GetPicklistKeyValues(field, recordType)?.Any(IsValidForCode) ?? false);
        }

        protected bool IsValidForCode(PicklistOption option)
        {
            return
                !string.IsNullOrWhiteSpace(CreateCodeLabel(option.Value));
        }
    }
}