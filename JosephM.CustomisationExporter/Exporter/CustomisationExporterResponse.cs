using JosephM.Core.Attributes;
using JosephM.Core.Service;

namespace JosephM.CustomisationExporter.Exporter
{
    public class CustomisationExporterResponse : ServiceResponseBase<CustomisationExporterResponseItem>
    {
        [Hidden]
        public string Folder { get; set; }
        [Hidden]
        public string FieldsFileName { get; set; }
        [Hidden]
        public string FieldsFileNameQualified
        {
            get { return string.Format("{0}/{1}", Folder, FieldsFileName); }
        }
        [Hidden]
        public string TypesFileName { get; set; }
        [Hidden]
        public string TypesFileNameQualified
        {
            get { return string.Format("{0}/{1}", Folder, TypesFileName); }
        }
        [Hidden]
        public string RelationshipsFileName { get; set; }
        [Hidden]
        public string RelationshipsFileNameQualified
        {
            get { return string.Format("{0}/{1}", Folder, RelationshipsFileName); }
        }
        [Hidden]
        public string OptionSetsFileName { get; set; }
        [Hidden]
        public string OptionSetsFileNameQualified
        {
            get { return string.Format("{0}/{1}", Folder, OptionSetsFileName); }
        }
    }
}