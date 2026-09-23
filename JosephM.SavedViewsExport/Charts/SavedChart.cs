using JosephM.Core.Attributes;
using JosephM.Record.Xrm.XrmRecord;
using System;

namespace JosephM.UserSavedObjectsUtility.Charts
{
    public class SavedChart
    {
        private XrmRecordService _sourceConnection;

        public SavedChart(XrmRecordService viewSourceConnection)
        {
            _sourceConnection = viewSourceConnection;
        }

        public XrmRecordService GetSourceConnection()
        {
            return _sourceConnection;
        }

        public string Id { get; set; }

        [Hidden]
        public string OwningUserId { get; set; }

        [DisplayOrder(20)]
        public string Owner { get; set; }

        [DisplayOrder(30)]
        public string RecordType { get; set; }

        [DisplayOrder(40)]
        public string Name { get; set; }

        [DisplayOrder(50)]
        public DateTime? CreatedOn { get; set; }

        [DisplayOrder(60)]
        public DateTime? ModifiedOn { get; set; }

        [DisplayOrder(70)]
        public string ChartPresentationXml { get; set; }

        [DisplayOrder(80)]
        public string ChartDataXml { get; set; }

        [DisplayOrder(90)]
        public string Description { get; set; }
    }
}
