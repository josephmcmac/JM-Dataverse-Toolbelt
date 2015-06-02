#region

using System.Collections.Generic;
using JosephM.Core.FieldType;
using JosephM.Core.Service;
using JosephM.Record.Metadata;
using JosephM.Record.Query;

#endregion

namespace JosephM.Record.IService
{
    /// <summary>
    ///     A Data Source For Querying Data And Metadata Defining The Record Types
    /// </summary>
    public interface IRecordService
    {
        /// <summary>
        ///     Returns All Records In The Data Store Of The Given Type With The Specified Fields Loaded
        /// </summary>
        IEnumerable<IRecord> RetrieveAll(string recordType, IEnumerable<string> fields);

        /// <summary>
        ///     Gets The Label Of The Field With The Given name In The Given RecordType
        /// </summary>
        string GetFieldLabel(string fieldName, string recordType);

        /// <summary>
        ///     Gets The Maximum Length Of A String Field
        /// </summary>
        int GetMaxLength(string fieldName, string recordType);

        /// <summary>
        ///     Gets The Picklist Options For A Picklist Field
        /// </summary>
        IEnumerable<PicklistOption> GetPicklistKeyValues(string fieldName, string recordType);

        /// <summary>
        ///     Gets The Picklist Options For A Picklist Field Filtered By The Depenedant Values
        /// </summary>
        IEnumerable<PicklistOption> GetPicklistKeyValues(string fieldName, string recordType, string dependentValue);

        /// <summary>
        ///     Return If The Field Is Defined As Required From The Data Stores Metadata
        /// </summary>
        bool IsMandatory(string fieldName, string recordType);

        /// <summary>
        ///     Return The Type Of The Field From The Data Stores Metadata
        /// </summary>
        RecordFieldType GetFieldType(string fieldName, string recordType);

        /// <summary>
        ///     Return The Maximum Value Of An Integer Field From The Data Stores Metadata
        /// </summary>
        int GetMaxIntValue(string field, string recordType);

        /// <summary>
        ///     Return The Minimum Value Of An Integer Field From The Data Stores Metadata
        /// </summary>
        int GetMinIntValue(string field, string recordType);

        /// <summary>
        ///     Gets The First Record Of The Type With The Given Value In The Field
        /// </summary>
        IRecord GetFirst(string recordType, string fieldName, object fieldValue);

        /// <summary>
        ///     Saves All Fields Loaded In The Record To The Record In The Data Store
        /// </summary>
        void Update(IRecord record);

        /// <summary>
        ///     Initialises A New IRecord Of The Correct IRecord Type For The IRecordService
        /// </summary>
        IRecord NewRecord(string recordType);

        /// <summary>
        ///     Saves The New Record To The Data Stored And Returns The Id (Primary Key) Of The Saved Record
        /// </summary>
        string Create(IRecord record);

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
        ///     Attempts To Convert A Value To The Type Required By A Field Of The Type In The Metadata Cache
        ///     Returns If The Conversion was Successful And if So The Converted Value
        /// </summary>
        ParseFieldResponse ParseFieldRequest(ParseFieldRequest parseFieldRequest);

        /// <summary>
        ///     Saves Only The Specified Fields To The Record In The Data Store
        /// </summary>
        void Update(IRecord record, IEnumerable<string> changedPersistentFields);

        /// <summary>
        ///     Returns The First Records With A Record Names Starting With The Search String
        /// </summary>
        IEnumerable<IRecord> RetrieveMultiple(string recordType, string searchString, int maxCount);

        /// <summary>
        ///     Gets The Target Type Of A Lookup Field
        /// </summary>
        string GetLookupTargetType(string fieldName, string recordType);

        /// <summary>
        ///     Gets The Name Of The Primary Name Field Of The Record Type
        /// </summary>
        string GetPrimaryField(string recordType);

        /// <summary>
        ///     Gets All Applicable Fields For The Record Type
        /// </summary>
        IEnumerable<string> GetFields(string recordType);

        /// <summary>
        ///     Gets The Collection Name For Records Of The Type
        /// </summary>
        string GetCollectionName(string recordType);

        /// <summary>
        ///     Gets The Display Name For A Records Type
        /// </summary>
        string GetDisplayName(string recordTypes);

        /// <summary>
        ///     Returns If Audit Is Set To On For Records Of The Type In The Data Store
        /// </summary>
        bool IsAuditOn(string recordTypes);

        /// <summary>
        ///     Gets The Description Of The Field In The Data Stored Metadata
        /// </summary>
        string GetFieldDescription(string fieldName, string recordType);

        /// <summary>
        ///     Returns If Audit Is Set To On For The Field In The Data Store
        /// </summary>
        bool IsFieldAuditOn(string fieldName, string recordType);

        /// <summary>
        ///     Returns If Audit Is Set To Searchable For The Field In The Data Store
        /// </summary>
        bool IsFieldSearchable(string fieldName, string recordType);

        /// <summary>
        ///     Gets The Text Format Of The String Field In The Data Store
        /// </summary>
        TextFormat GetTextFormat(string fieldName, string recordType);

        /// <summary>
        ///     Return The Minimum Value Of An Decimal Field From The Data Stores Metadata
        /// </summary>
        decimal GetMinDecimalValue(string fieldName, string recordType);

        /// <summary>
        ///     Return The Maximum Value Of An Decimal Field From The Data Stores Metadata
        /// </summary>
        decimal GetMaxDecimalValue(string fieldName, string recordType);

        /// <summary>
        ///     Return If A Date Field Is Defined To Include A Time Compponent
        /// </summary>
        bool IsDateIncludeTime(string fieldName, string recordType);

        /// <summary>
        ///     Gets The First Record Of The Given Type From The Data Store
        /// </summary>
        IRecord GetFirst(string recordType);

        /// <summary>
        ///     Gets All Records Of The Given Type Where The String Field Has The Given Value
        /// </summary>
        IEnumerable<IRecord> GetWhere(string recordType, string fieldName, string value);

        /// <summary>
        ///     Gets The Integer Format Of The Integer Field In The Data Store
        /// </summary>
        IntegerType GetIntegerType(string fieldType, string recordType);

        /// <summary>
        ///     Gets The Name Of The Primary Key Field For The Record Type
        /// </summary>
        string GetPrimaryKey(string recordType);

        /// <summary>
        ///     Gets The Record Of The Type With The Given Id Loading All Fields
        /// </summary>
        IRecord Get(string recordType, string id);

        /// <summary>
        ///     Gets A String Value For For Matching The Fields Value
        /// </summary>
        string GetFieldAsMatchString(string recordType, string fieldName, object fieldValue);

        /// <summary>
        ///     Indexes All Records By The GetFieldAsMatchString Value For The Given Field
        /// </summary>
        IDictionary<string, IRecord> IndexRecordsByField(string recordType, string fieldName);

        /// <summary>
        ///     Attempts To Convert The Value To The Correct Type In The Data Store For The Field
        /// </summary>
        object ParseField(string fieldName, string recordType, object value);

        /// <summary>
        ///     Indexes All Records Associated In The Relationship From A Specified Side
        /// </summary>
        IDictionary<string, IEnumerable<string>> IndexAssociatedIds(string recordType, string relationshipName,
            string otherSideId);

        /// <summary>
        ///     Returns The Ids Of Records Associated To The Record With The Id In The Relationship
        /// </summary>
        IEnumerable<string> GetAssociatedIds(string recordType, string id, string relationshipName, string otherSideId);

        /// <summary>
        ///     Indexes All Ids Of Records Of The Type By The GetFieldAsMatchString Field
        /// </summary>
        IDictionary<string, string> IndexGuidsByValue(string recordType, string fieldName);

        /// <summary>
        ///     Indexes All Ids Of Records Of The Type By The GetFieldAsMatchString Field Where The String Field Has A Value In
        ///     unmatchedStrings
        /// </summary>
        IDictionary<string, string> IndexMatchingGuids(string recordType, string fieldName,
            IEnumerable<string> unmatchedStrings);

        /// <summary>
        ///     Returns If The Field Is Of A Type Which references Another Record
        /// </summary>
        bool IsLookup(string field, string recordType);

        /// <summary>
        ///     Returns If The Field Is Of A Type Which references Another Record
        /// </summary>
        bool IsCustomer(string recordType, string field);

        /// <summary>
        ///     Returns All Records Which Match At Least One Of The Given Conditions With All Fields Loaded
        /// </summary>
        IEnumerable<IRecord> RetrieveAllOrClauses(string recordType, IEnumerable<Condition> orConditions);

        /// <summary>
        ///     Returns All Records Which Match At Least One Of The Given Conditions With the Specified Fields Loaded
        /// </summary>
        IEnumerable<IRecord> RetrieveAllOrClauses(string recordType, IEnumerable<Condition> orConditions,
            IEnumerable<string> fields);

        /// <summary>
        ///     Returns All Records Which Match All Of The Given Conditions With All Fields Loaded
        /// </summary>
        IEnumerable<IRecord> RetrieveAllAndClauses(string recordType, IEnumerable<Condition> andConditions);

        /// <summary>
        ///     Returns All Records Which Match At Least One Of The Given Conditions All Fields Loaded
        /// </summary>
        IEnumerable<IRecord> RetrieveAllAndClauses(string recordType, IEnumerable<Condition> andConditions,
            IEnumerable<string> fields);

        /// <summary>
        ///     Creates The Record With Only The Specified Fields
        /// </summary>
        void Create(IRecord record, IEnumerable<string> fieldToSet);

        /// <summary>
        ///     Deletes The Record From The Data Store
        /// </summary>
        void Delete(IRecord record);

        /// <summary>
        ///     Deletes The Record Of The Tyoe With The Given Id from The Data Store
        /// </summary>
        void Delete(string recordType, string id);

        /// <summary>
        ///     Gets The First Records Of The Type From The Data Store With All Fields Loaded
        /// </summary>
        IEnumerable<IRecord> GetFirstX(string recordType, int x);

        /// <summary>
        ///     Gets The First Records Of The Type From The Data Store With Only The Specified Fields Loaded
        /// </summary>
        IEnumerable<IRecord> GetFirstX(string recordType, int x, IEnumerable<string> fields,
            IEnumerable<Condition> conditions, IEnumerable<SortExpression> sort);

        /// <summary>
        ///     Saves The Record Type To The Data Store Creating It If It Does Not Yet Exist
        /// </summary>
        void CreateOrUpdate(RecordMetadata recordMetadata);

        /// <summary>
        ///     Saves The Relationship To The Data Store Creating It If It Does Not Yet Exist
        /// </summary>
        void CreateOrUpdate(RelationshipMetadata relationshipMetadata);

        /// <summary>
        ///     Saves The Field To The Data Store Creating It If It Does Not Yet Exist
        /// </summary>
        void CreateOrUpdate(FieldMetadata field, string recordType);

        /// <summary>
        ///     Returns If A Record Type With The Given Name Exists In The Data Store
        /// </summary>
        bool RecordTypeExists(string recordType);

        /// <summary>
        ///     Returns If A Field Exists In The Data Store
        /// </summary>
        bool FieldExists(string fieldName, string recordType);

        /// <summary>
        ///     Returns If A Relationship Type With The Given Name Exists In The Data Store
        /// </summary>
        bool RelationshipExists(string relationshipName);

        /// <summary>
        ///     Returns A Response Containing If The Service Successfully Connected Tio The Data Store
        /// </summary>
        IsValidResponse VerifyConnection();

        /// <summary>
        ///     Sets the Data Store To Apply All Customisation Changes
        /// </summary>
        void PublishAll();

        /// <summary>
        ///     Updates The Views Defined In The RecordMetadata To The Data Store
        /// </summary>
        void UpdateViews(RecordMetadata recordMetadata);

        /// <summary>
        ///     Returns If There Is A Shared Option Set With The Given Name In The Data Store
        /// </summary>
        bool SharedOptionSetExists(string optionSetName);

        /// <summary>
        ///     Saves The Shared Option Set To The Data Store Creating It If It Does Not Yet Exist
        /// </summary>
        void CreateOrUpdateSharedOptionSet(PicklistOptionSet sharedOptionSet);

        /// <summary>
        ///     Updates The Shared Option Set To Contain The Specified Values
        /// </summary>
        void UpdateFieldOptionSet(string recordType, string fieldName, PicklistOptionSet optionSet);

        /// <summary>
        ///     Returns The Name Of All Applicable Record Types Which Exists In The Data Store
        /// </summary>
        IEnumerable<string> GetAllRecordTypes();

        /// <summary>
        ///     Returns A Human Readable String For The Value In The Field Of The Record
        /// </summary>
        string GetFieldAsDisplayString(IRecord record, string fieldName);

        /// <summary>
        ///     Gets The Name Of The Sql Accessible View For The Record Type In The Data Store
        /// </summary>
        string GetSqlViewName(string recordType);

        /// <summary>
        ///     Gets The Name Of The Sql Data base Containing Data In The Data Store
        /// </summary>
        string GetDatabaseName();

        /// <summary>
        ///     Service To Get Data For Lookup Fields In The Data Store
        /// </summary>
        IRecordService LookupService { get; }

        /// <summary>
        ///     Gets All One To Many Relationships To Records Of The Type In The Data Store
        /// </summary>
        IEnumerable<One2ManyRelationshipMetadata> GetOneToManyRelationships(string recordType);

        /// <summary>
        ///     Gets All Records Related To The Record Via The One To Many Relationship
        /// </summary>
        IEnumerable<IRecord> GetRelatedRecords(IRecord recordToExtract,
            One2ManyRelationshipMetadata one2ManyRelationshipMetadata);

        /// <summary>
        ///     Gets The Label For Records Related Through The One To Many relationship
        /// </summary>
        string GetRelationshipLabel(One2ManyRelationshipMetadata one2ManyRelationshipMetadata);

        /// <summary>
        ///     Gets All Many To Many Relationships To Records Of The Type In The Data Store
        /// </summary>
        IEnumerable<Many2ManyRelationshipMetadata> GetManyToManyRelationships(string recordType);

        /// <summary>
        ///     Gets All Records Related To The Record Via The Many To Many Relationship
        /// </summary>
        IEnumerable<IRecord> GetRelatedRecords(IRecord recordToExtract,
            Many2ManyRelationshipMetadata many2ManyRelationshipMetadata, bool from1);

        /// <summary>
        ///     Gets The Label For Records Related Through The Many To Many relationship. Specifiying The Specific Side From If
        ///     Both Of Same Type
        /// </summary>
        string GetRelationshipLabel(Many2ManyRelationshipMetadata many2ManyRelationshipMetadata, bool from1);

        /// <summary>
        ///     Returns If The Field Is Of Activity Party Type
        /// </summary>
        bool IsActivityParty(string fieldName, string recordType);

        /// <summary>
        ///     Associates The Records In The Data Store By The Many To Many Relationship
        /// </summary>
        void Associate(Many2ManyRelationshipMetadata relationshipName, IRecord record1, IRecord record2);

        /// <summary>
        ///     Gets The Label For The Specified Key In The Picklist Field
        /// </summary>
        string GetOptionLabel(string optionKey, string fieldName, string recordType);

        /// <summary>
        ///     Gets All Views Of The Record Type Which Are Defined In The Data Store
        /// </summary>
        IEnumerable<ViewMetadata> GetViews(string recordType);

        /// <summary>
        ///     Returns If The Field Is Of A String Type
        /// </summary>
        bool IsString(string fieldName, string recordType);

        /// <summary>
        ///     Returns All Fields Of String Type In The Record Type
        /// </summary>
        IEnumerable<string> GetStringFields(string recordType);

        /// <summary>
        ///     Returns All Fields Which Are Queryable Through The Service
        /// </summary>
        IEnumerable<string> GetAllRecordTypesForSearch();

        /// <summary>
        ///     Returns If The Record Type Is An Activity Type
        /// </summary>
        bool IsActivityType(string recordType);

        /// <summary>
        ///     Returns If The Record Type Which May Be A Participant In An Activity
        /// </summary>
        IEnumerable<string> GetActivityParticipantTypes();

        /// <summary>
        ///     Returns If The Record Type May Be A Participant In An Activity
        /// </summary>
        bool IsActivityPartyParticipant(string recordType);

        /// <summary>
        ///     Returns All Record Type Which Are An Activity Type
        /// </summary>
        IEnumerable<string> GetActivityTypes();

        /// <summary>
        /// Returns All if The Field Is Updateable
        /// </summary>
        bool IsWritable(string fieldName, string recordType);

        /// <summary>
        /// Returns All if The Field Is Valid For Create
        /// </summary>
        bool IsCreateable(string fieldName, string recordType);

        bool IsReadable(string fieldName, string recordType);

        bool IsCustomField(string fieldName, string recordType);

        bool IsCustomType(string recordType);

        bool IsCustomRelationship(string relationshipName);

        string GetRecordTypeCode(string thisType);

        bool IsDisplayRelated(Many2ManyRelationshipMetadata relationship, bool from1);

        bool IsCustomLabel(Many2ManyRelationshipMetadata relationship, bool from1);

        int DisplayOrder(Many2ManyRelationshipMetadata relationship, bool from1);

        int GetDisplayOrder(Many2ManyRelationshipMetadata relationship, bool from1);

        bool IsDisplayRelated(One2ManyRelationshipMetadata relationship);

        int GetDisplayOrder(One2ManyRelationshipMetadata relationship);

        bool IsCustomLabel(One2ManyRelationshipMetadata relationship);

        double GetMinDoubleValue(string field, string recordType);

        double GetMaxDoubleValue(string field, string recordType);

        decimal GetMinMoneyValue(string field, string recordType);

        decimal GetMaxMoneyValue(string field, string recordType);

        bool IsFieldDisplayRelated(string field, string recordType);

        bool IsSharedPicklist(string field, string recordType);

        string GetSharedPicklistDisplayName(string field, string recordType);

        string GetSharedPicklistDisplayName(string optionSetName);

        bool HasNotes(string recordType);

        bool HasActivities(string recordType);

        bool HasConnections(string recordType);

        bool HasMailMerge(string recordType);

        bool HasQueues(string recordType);

        string GetDescription(string recordType);

        IEnumerable<string> GetSharedOptionSetNames();

        IEnumerable<KeyValuePair<string,string>> GetSharedOptionSetKeyValues(string optionSetName);

        void ClearCache();
    }
}