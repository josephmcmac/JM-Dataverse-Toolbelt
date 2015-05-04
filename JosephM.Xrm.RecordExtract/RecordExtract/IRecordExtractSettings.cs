using System.Collections.Generic;
using JosephM.Record.Application;
using JosephM.Record.Application.SettingTypes;

namespace JosephM.Xrm.RecordExtract.RecordExtract
{
    public interface IRecordExtractSettings
    {
        bool IncludeCreatedByAndOn { get; set; }
        bool IncludeModifiedByAndOn { get; set; }
        bool IncludeCrmOwner { get; set; }
        bool IncludeState { get; set; }
        bool IncludeStatus { get; set; }
        IEnumerable<RecordTypeSetting> AllowedRecordTypes { get; set; }
        IEnumerable<string> GetRelationshipsToExclude();
        IEnumerable<RecordTypeSetting> ExtendedRecordTypesToExclude { get; set; }
        IEnumerable<string> GetRecordTypesToExclude();
        IEnumerable<RecordTypeSetting> RecordTypesOnlyDisplayName { get; set; }
        IEnumerable<string> GetStringValuesToExclude();
        IEnumerable<RecordFieldSetting> ExtendedFieldsToExclude { get; set; }
        IEnumerable<string> GetAllFieldsToExclude(string recordType);
        bool OnlyDisplayName(string recordType);
    }
}