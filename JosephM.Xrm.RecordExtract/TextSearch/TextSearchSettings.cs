using System.Collections.Generic;
using System.Linq;
using JosephM.Record.Application;
using JosephM.Record.Application.SettingTypes;

namespace JosephM.Xrm.RecordExtract.TextSearch
{
    public class TextSearchSettings : ITextSearchSettings
    {
        private IEnumerable<RecordTypeSetting> _recordTypesToExclude = new RecordTypeSetting[0];

        public IEnumerable<RecordTypeSetting> ExtendedRecordTypesToExclude
        {
            get { return _recordTypesToExclude; }
            set
            {
                if (value != null)
                    _recordTypesToExclude = value.ToArray();
                else
                    _recordTypesToExclude = new RecordTypeSetting[0];
            }
        }

        public IEnumerable<string> GetRecordTypesToExclude()
        {
            return
                ExtractUtility.GetSystemRecordTypesToExclude()
                    .Union(ExtendedRecordTypesToExclude.Select(r => r.RecordType.Key));
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

        public IEnumerable<string> GetFieldsToExclude()
        {
            return
                ExtractUtility.GetSystemFieldsToExclude().Union(ExtendedFieldsToExclude.Select(r => r.RecordField.Key));
        }

        private int _textSearchSetSize = 5000;

        public int TextSearchSetSize
        {
            get { return _textSearchSetSize; }
            set { _textSearchSetSize = value; }
        }

        private IEnumerable<RecordFieldSetting> _extendedTextSearchSetFields = new RecordFieldSetting[0];

        public IEnumerable<RecordFieldSetting> ExtendedTextSearchSetFields
        {
            get { return _extendedTextSearchSetFields; }
            set
            {
                if (value != null)
                    _extendedTextSearchSetFields = value;
                else
                    _extendedTextSearchSetFields = new RecordFieldSetting[0];
            }
        }

        public IEnumerable<RecordFieldSetting> GetTextSearchSetFields()
        {
            return ExtractUtility.GetSystemTextSearchSetFields().Union(ExtendedTextSearchSetFields);
        }
    }
}