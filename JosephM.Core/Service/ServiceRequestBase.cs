using JosephM.Core.Attributes;
using System.Runtime.Serialization;

namespace JosephM.Core.Service
{
    [Group(Sections.SavedRequestDetails, true, 0)]
    [DataContract]
    public class ServiceRequestBase : IAllowSaveAndLoad
    {
        [Group(Sections.SavedRequestDetails)]
        [DataMember]
        [UniqueOn]
        [DisplayOrder(1)]
        [GridWidth(75)]
        [PropertyInContextByPropertyValue("DisplaySavedSettingFields", true)]
        public bool Autoload { get; set; }

        [Group(Sections.SavedRequestDetails)]
        [DataMember]
        [DisplayOrder(2)]
        [GridWidth(250)]
        [PropertyInContextByPropertyValue("DisplaySavedSettingFields", true)]
        [RequiredProperty]
        public string Name { get; set; }

        [Hidden]
        public bool DisplaySavedSettingFields { get; set; }

        public override string ToString()
        {
            return Name ?? base.ToString();
        }

        private static class Sections
        {
            public const string SavedRequestDetails = "SavedRequestDetails";
        }
    }
}