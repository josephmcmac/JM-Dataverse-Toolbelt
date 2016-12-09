using JosephM.Core.FieldType;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JosephM.CustomisationImporter.ImportMetadata
{
    public class ImportPicklistOption : PicklistOption
    {
        public ImportPicklistOption(string key, string value, int index)
            : base(key, value)
        {
            Index = index;
        }

        private int Index { get; set; }

        public int ExcelRow { get { return Index + 1; } }
    }
}
