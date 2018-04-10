using JosephM.Application.ViewModel.SettingTypes;
using System.Collections.Generic;

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