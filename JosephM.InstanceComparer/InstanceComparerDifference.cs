namespace JosephM.InstanceComparer
{
    public class InstanceComparerDifference
    {
        public string Type { get; set; }
        public string Name { get; set; }
        public string Difference { get; set; }
        public string ParentReference { get; set; }

        public InstanceComparerDifference(string type, string name, string difference, string parentReference)
        {
            Type = type;
            Name = name;
            Difference = difference;
            ParentReference = parentReference;
        }
    }
}