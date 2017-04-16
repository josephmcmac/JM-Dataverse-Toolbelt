using JosephM.Application.ViewModel.SettingTypes;
using JosephM.Core.FieldType;
using JosephM.Core.Log;
using JosephM.Xrm.RecordExtract.DocumentWriter;
using System.Collections.Generic;
using System.Linq;

namespace JosephM.Xrm.RecordExtract.RecordExtract
{
    public class RecordExtractToDocumentRequest
    {
        public RecordExtractToDocumentRequest(Lookup lookup, Section section, LogController controller, DetailLevel relatedDetail
            , IEnumerable<string> typesOnlyDisplayName = null
            , IEnumerable<RecordFieldSetting> extendedFieldsToExclude = null
            , IEnumerable<string> extendedRecordTypesToExclude = null
            , bool includeCreatedByAndOn = false, bool includeModifiedByAndOn = false, bool includeCrmOwner = false, bool includeState = false, bool includeStatus = false)
        {
            RecordLookup = lookup;
            Section = section;
            Controller = controller;
            RelatedDetail = relatedDetail;
            TypesOnlyDisplayName = typesOnlyDisplayName == null ? new string[0] : typesOnlyDisplayName;
            ExtendedFieldsToExclude = extendedFieldsToExclude == null ? new RecordFieldSetting[0] : extendedFieldsToExclude;
            ExtendedRecordTypesToExclude = extendedRecordTypesToExclude == null ? new string[0] : extendedRecordTypesToExclude;
            IncludeCreatedByAndOn = includeCreatedByAndOn;
            IncludeModifiedByAndOn = includeModifiedByAndOn;
            IncludeCrmOwner = includeCrmOwner;
            IncludeState = includeState;
            IncludeStatus = includeStatus;
        }

        public Lookup RecordLookup { get; set; }
        public Section Section { get; set; }
        public LogController Controller { get; set; }
        public DetailLevel RelatedDetail { get; set; }
        public IEnumerable<string> TypesOnlyDisplayName { get; private set; }
        public IEnumerable<RecordFieldSetting> ExtendedFieldsToExclude { get; private set; }
        public IEnumerable<string> ExtendedRecordTypesToExclude { get; private set; }
        public bool IncludeCreatedByAndOn { get; private set; }
        public bool IncludeModifiedByAndOn { get; private set; }
        public bool IncludeCrmOwner { get; private set; }
        public bool IncludeState { get; private set; }
        public bool IncludeStatus { get; private set; }

        public bool OnlyDisplayName(string recordType)
        {
            return TypesOnlyDisplayName == null || !TypesOnlyDisplayName.Any(r => r == recordType);
        }

        public IEnumerable<string> GetAllFieldsToExclude(string recordType)
        {
            var fields = new List<string>();
            fields.AddRange(ExtractUtility.GetSystemFieldsToExclude());
            fields.AddRange(ExtendedFieldsToExclude
                .Where(f => f.RecordType.Key == recordType)
                .Select(f => f.RecordField.Key));
            if (!IncludeCreatedByAndOn)
                fields.AddRange(new[] { "createdon", "createdby" });
            if (!IncludeModifiedByAndOn)
                fields.AddRange(new[] { "modifiedon", "modifiedby" });
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
    }
}