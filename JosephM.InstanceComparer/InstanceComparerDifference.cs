using JosephM.Core.Attributes;
using JosephM.Core.FieldType;

namespace JosephM.InstanceComparer
{
    public class InstanceComparerDifference
    {
        [GridWidth(150)]
        [DisplayOrder(10)]
        public string Type { get; set; }
        [DisplayOrder(20)]
        [GridWidth(300)]
        public string Name { get; set; }
        [DisplayOrder(30)]
        public string Difference { get; set; }
        [GridWidth(125)]
        [DisplayOrder(70)]
        [PropertyInContextByPropertyNotNull(nameof(Url1))]
        public Url Url1 { get; set; }
        [GridWidth(125)]
        [DisplayOrder(80)]
        [PropertyInContextByPropertyNotNull(nameof(Url2))]
        public Url Url2 { get; set; }
        [DisplayOrder(90)]
        [PropertyInContextByPropertyNotNull(nameof(Value1))]
        public string Value1 { get; set; }
        [DisplayOrder(95)]
        [PropertyInContextByPropertyNotNull(nameof(Value2))]
        public string Value2 { get; set; }


        public InstanceComparerDifference(string type, string name, string difference, string parentReference, string value1, string value2, Url url1, Url url2)
        {
            Type = type;
            Name = string.Format("{0}{1}", parentReference == null ? null : ("[" + parentReference + "] "), name);
            Difference = difference;
            Value1 = value1;
            Value2 = value2;
            Url1 = url1;
            Url2 = url2;
        }
    }
}
