using JosephM.Core.Service;

namespace JosephM.Xrm.CustomisationExporter
{
    public class CustomisationExporterResponse : ServiceResponseBase<CustomisationExporterResponseItem>
    {
        public string Folder { get; set; }
        public string FieldsFileName { get; set; }
        public string FieldsFileNameQualified
        {
            get { return string.Format("{0}/{1}", Folder, FieldsFileName); }
        }
        public string TypesFileName { get; set; }
        public string TypesFileNameQualified
        {
            get { return string.Format("{0}/{1}", Folder, TypesFileName); }
        }
        public string RelationshipsFileName { get; set; }
        public string RelationshipsFileNameQualified
        {
            get { return string.Format("{0}/{1}", Folder, RelationshipsFileName); }
        }
        public string OptionSetsFileName { get; set; }
        public string OptionSetsFileNameQualified
        {
            get { return string.Format("{0}/{1}", Folder, OptionSetsFileName); }
        }
    }
}