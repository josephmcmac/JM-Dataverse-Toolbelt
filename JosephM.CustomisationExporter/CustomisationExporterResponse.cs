using JosephM.Core.Attributes;
using JosephM.Core.Service;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace JosephM.CustomisationExporter
{
    public class CustomisationExporterResponse : ServiceResponseBase<CustomisationExporterResponseItem>
    {
        [Hidden]
        public string Folder { get; set; }
        [Hidden]
        public string ExcelFileName { get; set; }
        [Hidden]
        public string ExcelFileNameQualified
        {
            get { return ExcelFileName != null ? Path.Combine(Folder, ExcelFileName) : null; }
        }

        private Dictionary<string, IEnumerable> _listsToOutput = new Dictionary<string, IEnumerable>();

        public void AddListToOutput(string name, IEnumerable items)
        {
            _listsToOutput.Add(name, items);
        }

        public IDictionary<string, IEnumerable> GetListsToOutput()
        {
            return _listsToOutput;
        }
    }
}