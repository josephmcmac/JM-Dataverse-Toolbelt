using JosephM.Core.Attributes;
using JosephM.Core.FieldType;

namespace JosephM.Application.Desktop.Module.AboutModule
{
    [Group(Sections.Application, Group.DisplayLayoutEnum.HorizontalCenteredInputOnly, order: 10)]
    [Group(Sections.Version, Group.DisplayLayoutEnum.HorizontalCenteredInputOnly, order: 20)]
    [Group(Sections.Detail, Group.DisplayLayoutEnum.HorizontalCenteredInputOnly, order: 30)]
    [Group(Sections.Links, Group.DisplayLayoutEnum.HorizontalCenteredInputOnly, order: 40)]
    public class About
    {
        [Group(Sections.Application)]
        [DisplayOrder(10)]
        public string Application { get; set; }

        [PropertyInContextByPropertyNotNull(nameof(Version))]
        [Group(Sections.Version)]
        [DisplayOrder(20)]
        public string Version { get; set; }

        [Group(Sections.Links)]
        [DisplayOrder(100)]
        public Url CodeLink { get; set; }

        [Group(Sections.Links)]
        [DisplayOrder(110)]
        public Url CreateIssueLink { get; set; }

        [Group(Sections.Links)]
        [DisplayOrder(120)]
        public Url OtherLink { get; set; }

        [Multiline]
        [Group(Sections.Detail)]
        [DisplayOrder(200)]
        public string AboutDetail { get; set; }

        public class Sections
        {
            public const string Application = "Application";
            public const string Version = "Version";
            public const string Links = "Links";
            public const string Detail = "Detail";
        }
    }
}
