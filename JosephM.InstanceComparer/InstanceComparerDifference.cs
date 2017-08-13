namespace JosephM.InstanceComparer
{
    public class InstanceComparerDifference
    {
        public string Type { get; set; }
        public string Name { get; set; }
        public string Difference { get; set; }
        public string ParentReference { get; set; }
        public string Value1 { get; set; }
        public string Value2 { get; set; }
        public string Id1 { get; set; }
        public string Id2 { get; set; }

        public InstanceComparerDifference(string type, string name, string difference, string parentReference, string value1, string value2, string id1, string id2)
        {
            Type = type;
            Name = name;
            Difference = difference;
            ParentReference = parentReference;
            Value1 = value1;
            Value2 = value2;
            Id1 = id1;
            Id2 = id2;
        }
    }
}
