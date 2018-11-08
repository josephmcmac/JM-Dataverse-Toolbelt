#region

using JosephM.Core.Attributes;
using JosephM.Core.Constants;
using JosephM.Core.FieldType;
using JosephM.Core.Service;
using System.Collections.Generic;

#endregion

namespace JosephM.Deployment.ImportXml
{
    [DisplayName("Import XML")]
    [Instruction("All XML Files In The Folder Will Be Imported Into The Target Instance. Matches To Update Records In The Target Will By Done By Either Primary Key, Then Name, Else If No Match Is Found A New Record Will Be Created")]
    [AllowSaveAndLoad]
    [Group(Sections.Main, true, 10)]
    [Group(Sections.Misc, true, 40)]
    public class ImportXmlRequest : ServiceRequestBase
    {
        public ImportXmlRequest()
        {
            MatchByName = true;
        }

        [GridWidth(350)]
        [DisplayOrder(20)]
        [Group(Sections.Main)]
        [RequiredProperty]
        [DisplayName("Select The Folder Containing The XML Files")]
        public Folder Folder { get; set; }

        [GridWidth(110)]
        [Group(Sections.Misc)]
        [DisplayOrder(399)]
        [RequiredProperty]
        public bool IncludeOwner { get; set; }

        [GridWidth(115)]
        [DisplayOrder(215)]
        [Group(Sections.Misc)]
        [RequiredProperty]
        public bool MatchByName { get; set; }

        [GridWidth(120)]
        [Group(Sections.Misc)]
        [DisplayOrder(400)]
        [RequiredProperty]
        public bool MaskEmails { get; set; }

        private static class Sections
        {
            public const string Main = "Main";
            public const string Misc = "Misc";
         }
    }
}