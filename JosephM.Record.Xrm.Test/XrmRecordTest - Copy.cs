#region

using System;
using System.Linq;
using JosephM.Record.IService;
using JosephM.Record.Metadata;
using JosephM.Record.Xrm.XrmRecord;
using JosephM.Xrm.Test;
using JosephM.Record.Extentions;

#endregion

namespace JosephM.Record.Xrm.Test
{
    public abstract class XrmRecordTest : XrmTest
    {
        protected XrmRecordTest()
        {
            XrmRecordService = new XrmRecordService(XrmService);
        }

        public XrmRecordService XrmRecordService { get; private set; }

        public void ClearCache()
        {
            XrmRecordService.ClearCache();
        }

        public object CreateNewFieldValue(string fieldName, string recordType, IRecordService recordService,
            IRecord currentRecord)
        {
            var currentValueNull = currentRecord.GetField(fieldName) == null;
            var fieldType = recordService.GetFieldType(fieldName, recordType);
            switch (fieldType)
            {
                case (RecordFieldType.String):
                {
                    return currentValueNull ? "BLAH" : "BLAHBLAH";
                }
                case (RecordFieldType.Date):
                {
                    return currentValueNull ? new DateTime(1980, 11, 15) : new DateTime(2001, 1, 1);
                }
                case (RecordFieldType.Lookup):
                {
                    var lookupTargetType = recordService.GetLookupTargetType(fieldName, recordType);
                    IRecord referenceRecord = null;
                    if (currentValueNull)
                    {
                        referenceRecord = recordService.GetFirst(lookupTargetType);
                    }
                    else
                    {
                        var rs = recordService.GetFirstX(lookupTargetType, 2, null, null, null);
                        if (rs.Any(r => r.Id != currentRecord.GetLookupId(fieldName)))
                            referenceRecord = rs.First(r => r.Id != currentRecord.GetLookupId(fieldName));
                    }
                    if (referenceRecord == null)
                    {
                        referenceRecord = recordService.NewRecord(lookupTargetType);
                        referenceRecord.SetField(recordService.GetPrimaryField(lookupTargetType), "TestLookup",
                            recordService);
                        recordService.Create(referenceRecord);
                    }
                    return referenceRecord.Id;
                }
                case (RecordFieldType.Picklist):
                case (RecordFieldType.Status):
                {
                    var options = recordService.GetPicklistKeyValues(fieldName, recordType);
                    var option1 = options.First().Key;
                    var option2 = options.Count() > 1 ? options.ElementAt(1).Key : options.First().Key;
                    if (currentValueNull)
                        return option1;
                    else
                        return currentRecord.GetOptionKey(fieldName) == option1 ? option2 : option1;
                }
                case (RecordFieldType.Boolean):
                {
                    return currentValueNull;
                }
                case (RecordFieldType.Integer):
                {
                    if (XrmRecordService.GetFieldMetadata(fieldName, recordType).IntegerFormat == IntegerType.TimeZone)
                    {
                        var timezoneRecords = XrmRecordService.GetFirstX("timezonedefinition", 2, null, null);
                        if (timezoneRecords.Count() < 2)
                            throw new Exception("At least 2 Records Required");
                        var option1 = timezoneRecords.ElementAt(0).GetIntegerField("timezonecode");
                        var option2 = timezoneRecords.ElementAt(1).GetIntegerField("timezonecode");
                        if (currentValueNull)
                            return option1;
                        else
                            return currentRecord.GetIntegerField(fieldName) == option1 ? option2 : option1;
                    }
                    else
                        return currentValueNull ? 111 : 222;
                }
                case (RecordFieldType.Decimal):
                {
                    return currentValueNull ? new Decimal(111) : new decimal(222);
                }
                case (RecordFieldType.Money):
                {
                    return currentValueNull ? new Decimal(111) : new decimal(222);
                }
                case (RecordFieldType.Double):
                {
                    return currentValueNull ? 111 : 222;
                }
                case (RecordFieldType.Uniqueidentifier):
                {
                    return currentValueNull ? Guid.NewGuid().ToString() : currentRecord.Id;
                }
                default:
                {
                    throw new ArgumentOutOfRangeException("Unmatched field type " + fieldType);
                }
            }
        }
    }
}