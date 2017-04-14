using JosephM.Core.Attributes;
using System.Runtime.Serialization;

namespace JosephM.Core.Service
{
    [DataContract]
    public class ServiceRequestBase : IAllowSaveAndLoad
    {
        [DataMember]
        [UniqueOn]
        [DisplayOrder(1)]
        [GridWidth(75)]
        [PropertyInContextByPropertyValue("DisplaySavedSettingFields", true)]
        public bool Autoload { get; set; }

        [DataMember]
        [DisplayOrder(2)]
        [GridWidth(250)]
        [PropertyInContextByPropertyValue("DisplaySavedSettingFields", true)]
        public string Name { get; set; }

        [Hidden]
        public bool DisplaySavedSettingFields { get; set; }

        public override string ToString()
        {
            return Name ?? base.ToString();
        }
    }
}