#region

using JosephM.Core.Attributes;
using JosephM.Core.Constants;
using JosephM.Core.FieldType;
using JosephM.Core.Service;
using System.Collections.Generic;

#endregion

namespace JosephM.Deployment.ImportXml
{
    [AllowSaveAndLoad]
    [Group(Sections.Main, true, 10)]
    [Group(Sections.Misc, true, 40)]
    public class ImportXmlRequest : ServiceRequestBase
    {
        [DisplayOrder(20)]
        [Group(Sections.Main)]
        [RequiredProperty]
        [DisplayName("Select The Folder Containing The XML Files")]
        public Folder Folder { get; set; }

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