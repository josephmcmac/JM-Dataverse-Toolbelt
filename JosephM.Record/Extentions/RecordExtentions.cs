using System.Collections.Generic;
using System.Linq;
using JosephM.Core.Extentions;
using JosephM.Core.FieldType;
using JosephM.Record.IService;
using JosephM.Record.Query;

namespace JosephM.Record.Extentions
{
    /// <summary>
    ///     General Use Utility Methods For IRecords
    /// </summary>
    public static class RecordExtentions
    {
        /// <summary>
        ///     Loads The IRecord Into A Lookup. Does Not Load Name
        /// </summary>
        /// <param name="record"></param>
        /// <returns></returns>
        public static Lookup ToLookup(this IRecord record)
        {
            return new Lookup(record.Type, record.Id, null);
        }

        /// <summary>
        ///     Populates All Lookup Fields In The Set Of IRecords With The Name Of The Referenced Record
        ///     Used If The IRecordService Has Not Loaded The Name Of Referenced Records
        /// </summary>
        /// <param name="records">Records To Load The Names Into Lookup Fields</param>
        /// <param name="service">Service To Retrieve The Record Names</param>
        /// <param name="ignoreType">Types To Not Load The Name</param>
        public static void PopulateEmptyLookups(this IEnumerable<IRecord> records, IRecordService service,
            IEnumerable<string> ignoreType)
        {
            var emptyLookupNames = new Dictionary<string, List<Lookup>>();
            foreach (var record in records)
            {
                foreach (var field in record.GetFieldsInEntity())
                {
                    if (!field.IsNullOrWhiteSpace())
                    {
                        var value = record.GetField(field);
                        if (value is Lookup && ((Lookup) value).Name == null)
                        {
                            if (!emptyLookupNames.ContainsKey(field))
                                emptyLookupNames.Add(field, new List<Lookup>());
                            emptyLookupNames[field].Add((Lookup) value);
                        }
                    }
                }
            }
            foreach (var field in emptyLookupNames.Keys)
            {
                var distinctTypes =
                    emptyLookupNames[field].Select(l => l.RecordType).Distinct();
                foreach (var distinctType in distinctTypes.Where(t => !ignoreType.Contains(t)))
                {
                    try
                    {
                        var thisDistinctType = distinctType;
                        if (!thisDistinctType.IsNullOrWhiteSpace())
                        {
                            var typePrimaryKey = service.GetPrimaryKey(distinctType);
                            var typePrimaryField = service.GetPrimaryField(distinctType);
                            if (!typePrimaryField.IsNullOrWhiteSpace() && !typePrimaryKey.IsNullOrWhiteSpace())
                            {
                                var thisTypeLookups =
                                    emptyLookupNames[field].Where(l => l.RecordType == thisDistinctType).ToArray();
                                var distinctIds =
                                    thisTypeLookups.Select(l => l.Id).Where(s => !s.IsNullOrWhiteSpace()).Distinct();
                                var conditions =
                                    distinctIds.Select(id => new Condition(typePrimaryKey, ConditionType.Equal, id));
                                var theseRecords = service.RetrieveAllOrClauses(thisDistinctType, conditions,
                                    new[] {typePrimaryField}).ToArray();
                                foreach (var lookup in thisTypeLookups)
                                {
                                    if (theseRecords.Any(r => r.Id == lookup.Id))
                                        lookup.Name = theseRecords.First(r => r.Id == lookup.Id)
                                            .GetStringField(typePrimaryField);
                                }
                            }
                        }
                    }
// ReSharper disable once EmptyGeneralCatchClause
                    catch
                    {
                    }
                }
            }
        }
    }
}