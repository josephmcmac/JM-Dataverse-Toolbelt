using JosephM.Core.Attributes;
using JosephM.Record.Xrm.XrmRecord;
using System;

namespace JosephM.SavedViewsExport
{
    public class SavedView
    {
        private XrmRecordService _viewSourceConnection;

        public SavedView(XrmRecordService viewSourceConnection)
        {
            _viewSourceConnection = viewSourceConnection;
        }

        public XrmRecordService GetSourceConnection()
        {
            return _viewSourceConnection;
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
        public string FetchXml { get; set; }

        [DisplayOrder(80)]
        public string LayoutXml { get; set; }

        [DisplayOrder(90)]
        public string Description { get; set; }
    }
}
