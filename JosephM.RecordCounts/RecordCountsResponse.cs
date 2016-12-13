using JosephM.Core.Service;
using System.Collections.Generic;

namespace JosephM.RecordCounts.Exporter
{
    public class RecordCountsResponse : ServiceResponseBase<RecordCountsResponseItem>
    {
        public string Folder { get; set; }

        public string RecordCountsByOwnerFileName { get; set; }

        public string RecordCountsByOwnerFileNameQualified
        {
            get { return string.Format("{0}/{1}", Folder, RecordCountsByOwnerFileName); }
        }

        public string RecordCountsFileName { get; set; }

        public string RecordCountsFileNameQualified
        {
            get { return string.Format("{0}/{1}", Folder, RecordCountsFileName); }
        }

        public IEnumerable<RecordCount> RecordCounts { get; set; }
    }
}