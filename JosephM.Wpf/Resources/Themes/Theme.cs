using JosephM.Core.Attributes;

namespace JosephM.Wpf.Resources.Themes
{
    [Group(Sections.Theme, true, displayLabel: false)]
    public class Theme
    {
        [DisplayName("Colour Theme")]
        [RequiredProperty]
        [Group(Sections.Theme)]
        [SettingsLookup(typeof(XamlThemes), nameof(XamlThemes.Themes), allowAddNew: false)]
        public XamlTheme SelectedTheme { get; set; }

        private static class Sections
        {
            public const string Theme = "Theme";
        }
    }
}
