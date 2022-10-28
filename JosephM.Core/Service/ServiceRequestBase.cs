using JosephM.Core.Attributes;
using System.Runtime.Serialization;

namespace JosephM.Core.Service
{
    [Group(Sections.SavedRequestDetails, Group.DisplayLayoutEnum.HorizontalLabelAbove, order: 0)]
    [DataContract]
    public class ServiceRequestBase : IAllowSaveAndLoad
    {
        [Group(Sections.SavedRequestDetails)]
        [DataMember]
        [UniqueOn]
        [DisplayOrder(1)]
        [GridWidth(80)]
        [PropertyInContextByPropertyValue(nameof(DisplaySavedSettingFields), true)]
        public bool Autoload { get; set; }

        [Group(Sections.SavedRequestDetails)]
        [DataMember]
        [DisplayOrder(2)]
        [GridWidth(200)]
        [PropertyInContextByPropertyValue(nameof(DisplaySavedSettingFields), true)]
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