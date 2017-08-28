#region

using JosephM.Core.FieldType;
using JosephM.Core.Service;
using JosephM.Record.Metadata;
using JosephM.Record.Query;
using System.Collections.Generic;

#endregion

namespace JosephM.Record.IService
{
    /// <summary>
    ///     A Data Source For Querying Data And Metadata Defining The Record Types
    /// </summary>
    public interface IRecordService
    {
        IEnumerable<IFieldMetadata> GetFieldMetadata(string recordType);

        IRecordTypeMetadata GetRecordTypeMetadata(string recordType);

        IEnumerable<IPicklistSet> GetSharedPicklists();

        /// <summary>
        ///     Gets All One To Many Relationships To Records Of The Type In The Data Store
        /// </summary>
        IEnumerable<IOne2ManyRelationshipMetadata> GetOneToManyRelationships(string recordType);

        /// <summary>
        ///     Gets All Many To Many Relationships To Records Of The Type In The Data Store
        /// </summary>
        IEnumerable<IMany2ManyRelationshipMetadata> GetManyToManyRelationships(string recordType);

        /// <summary>
        ///     Gets The Picklist Options For A Picklist Field Filtered By The Dependant Values
        /// </summary>
        IEnumerable<PicklistOption> GetPicklistKeyValues(string fieldName, string recordType, string dependentValue, IRecord record);

        /// <summary>
        ///     Initialises A New IRecord Of The Correct IRecord Type For The IRecordService
        /// </summary>
        IRecord NewRecord(string recordType);

        /// <summary>
        ///     Creates The Record With Only The Specified Fields
        /// </summary>
        string Create(IRecord record, IEnumerable<string> fieldToSet);

        /// <summary>
        ///     Returns All Records Linked To A Record By A Lookup Field
        /// </summary>
        IEnumerable<IRecord> GetLinkedRecords(string linkedRecordType, string recordTypeFrom, string linkedRecordLookup,
            string recordFromId);

        /// <summary>
        ///     Returns All Records Linked To A Record By A Lookup Field
        /// </summary>
        IEnumerable<IRecord> GetLinkedRecordsThroughBridge(string linkedRecordType, string recordTypeThrough, string recordTypeFrom, string linkedThroughLookupFrom, string linkedThroughLookupTo,
            string recordFromId);

        /// <summary>
        ///     Saves Only The Specified Fields To The Record In The Data Store
        /// </summary>
        void Update(IRecord record, IEnumerable<string> fieldsToSubmit);

        /// <summary>
        ///     Gets The Target Type Of A Lookup Field
        /// </summary>
        string GetLookupTargetType(string fieldName, string recordType);

        /// <summary>
        ///     Gets The Record Of The Type With The Given Id Loading All Fields
        /// </summary>
        IRecord Get(string recordType, string id);

        /// <summary>
        ///     Gets A String Value For For Matching The Fields Value
        /// </summary>
        string GetFieldAsMatchString(string recordType, string fieldName, object fieldValue);

        /// <summary>
        ///     Attempts To Convert The Value To The Correct Type In The Data Store For The Field
        /// </summary>
        object ParseField(string fieldName, string recordType, object value);

        /// <summary>
        ///     Returns All Records Which Match At Least One Of The Given Conditions With the Specified Fields Loaded
        /// </summary>
        IEnumerable<IRecord> RetrieveAllOrClauses(string recordType, IEnumerable<Condition> orConditions,
            IEnumerable<string> fields);

        /// <summary>
        ///     Deletes The Record Of The Type With The Given Id 
        /// </summary>
        void Delete(string recordType, string id);

        /// <summary>
        ///     Gets The First X Records Of The Type
        /// </summary>
        IEnumerable<IRecord> GetFirstX(string recordType, int x, IEnumerable<string> fields,
            IEnumerable<Condition> conditions, IEnumerable<SortExpression> sort);

        /// <summary>
        ///     Saves The Record Type Creating It If It Does Not Yet Exist
        /// </summary>
        void CreateOrUpdate(RecordMetadata recordMetadata);

        /// <summary>
        ///     Saves The Relationship Creating It If It Does Not Yet Exist
        /// </summary>
        void CreateOrUpdate(IMany2ManyRelationshipMetadata relationshipMetadata);

        /// <summary>
        ///     Saves The Field Creating It If It Does Not Yet Exist
        /// </summary>
        void CreateOrUpdate(FieldMetadata field, string recordType);

        /// <summary>
        ///     Returns A Response Containing If The Service Successfully Connected
        /// </summary>
        IsValidResponse VerifyConnection();

        /// <summary>
        ///     Sets the Data Store To Apply All Customisation Changes
        /// </summary>
        void Publish(string xml = null);

        /// <summary>
        ///     Updates The Views Defined In The RecordMetadata
        /// </summary>
        void UpdateViews(RecordMetadata recordMetadata);

        /// <summary>
        ///     Saves The Shared Option Set To The Data Store Creating It If It Does Not Yet Exist
        /// </summary>
        void CreateOrUpdateSharedOptionSet(PicklistOptionSet sharedOptionSet);

        /// <summary>
        ///     Updates The Shared Option Set To Contain The Specified Values
        /// </summary>
        void UpdateFieldOptionSet(string recordType, string fieldName, PicklistOptionSet optionSet);

        /// <summary>
        ///     Returns The Name Of All Applicable Record Types Which Exists
        /// </summary>
        IEnumerable<string> GetAllRecordTypes();

        /// <summary>
        ///     Returns A Human Readable String For The Value In The Field Of The Record
        /// </summary>
        string GetFieldAsDisplayString(IRecord record, string fieldName);

        /// <summary>
        ///     Service To Get Data For Lookup Fields In The Data Store
        /// </summary>
        IRecordService LookupService { get; }

        /// <summary>
        ///     Gets All Records Related To The Record Via The Many To Many Relationship
        /// </summary>
        IEnumerable<IRecord> GetRelatedRecords(IRecord recordToExtract,
            IMany2ManyRelationshipMetadata many2ManyRelationshipMetadata, bool from1);

        /// <summary>
        ///     Gets All Views Of The Record Type Which Are Defined In The Data Store
        /// </summary>
        IEnumerable<ViewMetadata> GetViews(string recordType);

        void ClearCache();

        IRecordService GetLookupService(string fieldName, string recordType, string reference, IRecord record);

        IsValidResponse IsValidForNewType(string typeName);

        string GetDisplayNameField(string recordType);
    }
}
