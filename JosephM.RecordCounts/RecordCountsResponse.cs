using JosephM.Core.Attributes;
using JosephM.Core.Service;
using System.Collections.Generic;

namespace JosephM.RecordCounts.Exporter
{
    public class RecordCountsResponse : ServiceResponseBase<RecordCountsResponseItem>
    {
        [Hidden]
        public string Folder { get; set; }
        [Hidden]
        public string RecordCountsByOwnerFileName { get; set; }
        [Hidden]
        public string RecordCountsByOwnerFileNameQualified
        {
            get { return string.Format("{0}/{1}", Folder, RecordCountsByOwnerFileName); }
        }
        [Hidden]
        public string RecordCountsFileName { get; set; }
        [Hidden]
        public string RecordCountsFileNameQualified
        {
            get { return string.Format("{0}/{1}", Folder, RecordCountsFileName); }
        }
        [Hidden]
        public IEnumerable<RecordCount> RecordCounts { get; set; }
    }
}