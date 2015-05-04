using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Runtime.Serialization;
using JosephM.Core.Attributes;
using JosephM.Record.Application;
using JosephM.Record.Application.SettingTypes;

namespace JosephM.Xrm.RecordExtract.RecordExtract
{
    [DisplayName("Record Report Settings")]
    public class RecordExtractSettings : IRecordExtractSettings
    {
        public bool IncludeCreatedByAndOn { get; set; }
        public bool IncludeModifiedByAndOn { get; set; }
        public bool IncludeCrmOwner { get; set; }
        public bool IncludeState { get; set; }
        public bool IncludeStatus { get; set; }

        //if set null set the properties to an empty array
        private IEnumerable<RecordTypeSetting> _allowedRecordTypes = new RecordTypeSetting[0];

        public IEnumerable<RecordTypeSetting> AllowedRecordTypes
        {
            get { return _allowedRecordTypes; }
            set
            {
                if (value != null)
                    _allowedRecordTypes = value;
                else
                    _allowedRecordTypes = new RecordTypeSetting[0];
            }
        }

        public IEnumerable<string> GetRelationshipsToExclude()
        {
            return ExtractUtility.GetSystemRelationshipsToExclude();
        }

        private IEnumerable<RecordTypeSetting> _extendedRecordTypesToExclude = new RecordTypeSetting[0];

        public IEnumerable<RecordTypeSetting> ExtendedRecordTypesToExclude
        {
            get { return _extendedRecordTypesToExclude; }
            set
            {
                if (value != null)
                    _extendedRecordTypesToExclude = value;
                else
                    _extendedRecordTypesToExclude = new RecordTypeSetting[0];
            }
        }

        public IEnumerable<string> GetRecordTypesToExclude()
        {
            return
                ExtractUtility.GetSystemRecordTypesToExclude()
                    .Union(ExtendedRecordTypesToExclude.Select(r => r.RecordType.Key));
        }

        private IEnumerable<RecordTypeSetting> _recordTypesOnlyDisplayName = new RecordTypeSetting[0];

        public IEnumerable<RecordTypeSetting> RecordTypesOnlyDisplayName
        {
            get { return _recordTypesOnlyDisplayName; }
            set
            {
                if (value != null)
                    _recordTypesOnlyDisplayName = value;
                else
                    _recordTypesOnlyDisplayName = new RecordTypeSetting[0];
            }
        }

        public IEnumerable<string> GetStringValuesToExclude()
        {
            return ExtractUtility.GetStringValuesToExclude();
        }

        private IEnumerable<RecordFieldSetting> _extendedFieldsToExclude = new RecordFieldSetting[0];

        public IEnumerable<RecordFieldSetting> ExtendedFieldsToExclude
        {
            get { return _extendedFieldsToExclude; }
            set
            {
                if (value != null)
                    _extendedFieldsToExclude = value.ToArray();
                else
                    _extendedFieldsToExclude = new RecordFieldSetting[0];
            }
        }

        public IEnumerable<string> GetAllFieldsToExclude(string recordType)
        {
            var fields = new List<string>();
            fields.AddRange(ExtractUtility.GetSystemFieldsToExclude());
            fields.AddRange(ExtendedFieldsToExclude
                .Where(f => f.RecordType.Key == recordType)
                .Select(f => f.RecordField.Key));
            if (!IncludeCreatedByAndOn)
                fields.AddRange(new[] {"createdon", "createdby"});
            if (!IncludeModifiedByAndOn)
                fields.AddRange(new[] {"modifiedon", "modifiedby"});
            if (!IncludeCrmOwner)
            {
                fields.Add("ownerid");
                fields.Add("owninguser");
                fields.Add("owningteam");
            }
            if (!IncludeState)
                fields.Add("statecode");
            if (!IncludeStatus)
                fields.Add("statuscode");
            return fields;
        }

        public bool OnlyDisplayName(string recordType)
        {
            return RecordTypesOnlyDisplayName.Select(r => r.RecordType.Key).Contains(recordType);
        }
    }
}