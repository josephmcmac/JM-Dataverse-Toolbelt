#region

using System;
using System.Collections.Generic;
using JosephM.Core.FieldType;

#endregion

namespace JosephM.Record.IService
{
    /// <summary>
    ///     Interface For A Late Bound Record With A Property Bag Of Fields Referenced By A String Name
    /// </summary>
    public interface IRecord
    {
        /// <summary>
        ///     The Type Of Record
        /// </summary>
        string Type { get; set; }

        /// <summary>
        ///     The Primary Key For The Given Record
        ///     Null If Not Yet Created
        /// </summary>
        string Id { get; set; }

        /// <summary>
        ///     Used to determine an alternative field to use as the key when updating
        /// </summary>
        string IdTargetFieldOverride { get; set; }

        /// <summary>
        ///     Get The Value Of The Field With The Given Name. Null If Not Loaded Or Empty
        /// </summary>
        object this[string fieldName] { get; set; }

        /// <summary>
        ///     Sets The Field To The Value Calling The Parse Method On The Service To Convert To The Expected Type If Implemented
        /// </summary>
        void SetField(string field, object value, IRecordService service);

        /// <summary>
        ///     If The Field Is Null Or Not Loaded Into The Record
        /// </summary>
        bool FieldIsEmpty(string field);

        /// <summary>
        ///     Get The Value Of The Field With The Given Name. Null If Not Loaded Or Empty
        /// </summary>
        object GetField(string field);

        /// <summary>
        ///     If The Record Has A Field With The Given Name In Its Property Bag
        /// </summary>
        bool ContainsField(string field);

        /// <summary>
        ///     False If Null. Gets The Fields Object Value Cast As A Boolean.
        /// </summary>
        bool GetBoolField(string field);

        /// <summary>
        ///     Get The Fields Object Value Cast As A String.
        /// </summary>
        string GetStringField(string field);

        /// <summary>
        ///     0 If Null. Gets The Fields Object Value Cast As A Boolean.
        /// </summary>
        int GetIntegerField(string fieldName);

        /// <summary>
        ///     Returns All Fields Which Have A Null Or Non-Empty Value Loaded Into The Record
        /// </summary>
        IEnumerable<string> GetFieldsInEntity();

        /// <summary>
        ///     Gets The Type Which Is Referenced By A Lookup Field. Null If No Record Referenced (Field Null)
        /// </summary>
        string GetLookupType(string fieldName);

        /// <summary>
        ///     Gets The Id Of The Record Which Is Referenced By A Lookup Field. Null If No Record Referenced (Field Null)
        /// </summary>
        string GetLookupId(string fieldName);

        /// <summary>
        ///     Null If Name Not Loaded. Gets The Name Of The Record Which Is Referenced By A Lookup Field. Null If No Record
        ///     Referenced (Field Null)
        /// </summary>
        string GetLookupName(string fieldName);

        /// <summary>
        ///     Sets The Lookup Field To Reference A Record Of The type With The Given Id
        /// </summary>
        void SetLookup(string fieldName, string id, string recordType);

        /// <summary>
        ///     Gets The Key Of An Option Set Field. Null If The Field Is Not Loaded Ot Has Not Selected Option
        /// </summary>
        string GetOptionKey(string fieldName);

        /// <summary>
        ///     Gets The Value In The Primary Field (Name) Of This Record
        /// </summary>
        string GetName(IRecordService service);

        /// <summary>
        ///     Gets The Activity Party Records Which Are Loaded Into An Activity Party Field
        /// </summary>
        IEnumerable<IRecord> GetActivityParties(string field);

        /// <summary>
        ///     Gets A Nullable DateTime Field
        /// </summary>
        DateTime? GetDateTime(string field);

        /// <summary>
        ///     Gets A Lookupo Field Value
        /// </summary>
        Lookup GetLookupField(string fieldName);
    }
}