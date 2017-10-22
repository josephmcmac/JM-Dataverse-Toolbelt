namespace JosephM.Core.FieldType
{
    public class Url
    {
        public Url(string url, string label)
        {
            Label = label;
            Uri = url;
        }

        public string Label { get; set; }
        public string Uri { get; set; }

        public override string ToString()
        {
            return Uri;
        }
    }
}