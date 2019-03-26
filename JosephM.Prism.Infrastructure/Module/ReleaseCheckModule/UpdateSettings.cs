using JosephM.Core.Attributes;

namespace JosephM.Application.Desktop.Module.ReleaseCheckModule
{
    [Group(Sections.Main, wrapHorizontal: true, displayLabel: false)]
    public class UpdateSettings
    {
        public UpdateSettings()
        {
            CheckForNewReleaseOnStartup = true;
        }

        [Group(Sections.Main)]
        [DisplayOrder(10)]
        public bool CheckForNewReleaseOnStartup { get; set; }

        public class Sections
        {
            public const string Main = "Main";
        }
    }
}
