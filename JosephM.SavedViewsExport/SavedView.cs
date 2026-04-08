using JosephM.Core.Attributes;
using System;

namespace JosephM.SavedViewsExport
{
    public class SavedView
    {
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
    }
}
