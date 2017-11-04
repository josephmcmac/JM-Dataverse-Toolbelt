using JosephM.Core.Attributes;
using JosephM.Core.FieldType;

namespace JosephM.Application.Prism.Module.AboutModule
{
    [Group(Sections.Application, Group.DisplayLayoutEnum.HorizontalInputOnly, 10)]
    [Group(Sections.Links, Group.DisplayLayoutEnum.HorizontalInputOnly, 40)]
    [Group(Sections.Detail, Group.DisplayLayoutEnum.HorizontalInputOnly, 30)]
    public class About
    {
        [Group(Sections.Application)]
        [DisplayOrder(10)]
        public string Application { get; set; }

        [Group(Sections.Application)]
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
            public const string Links = "Links";
            public const string Detail = "Detail";
        }
    }
}
