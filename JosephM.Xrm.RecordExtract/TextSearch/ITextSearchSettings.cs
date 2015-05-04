using System.Collections.Generic;
using JosephM.Record.Application;
using JosephM.Record.Application.SettingTypes;

namespace JosephM.Xrm.RecordExtract.TextSearch
{
    public interface ITextSearchSettings
    {
        IEnumerable<RecordTypeSetting> ExtendedRecordTypesToExclude { get; set; }
        IEnumerable<RecordFieldSetting> ExtendedFieldsToExclude { get; set; }
        IEnumerable<string> GetFieldsToExclude();
        int TextSearchSetSize { get; set; }
        IEnumerable<RecordFieldSetting> ExtendedTextSearchSetFields { get; set; }
        IEnumerable<string> GetRecordTypesToExclude();
        IEnumerable<RecordFieldSetting> GetTextSearchSetFields();
    }
}