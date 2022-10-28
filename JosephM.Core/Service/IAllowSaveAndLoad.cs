using JosephM.Core.Attributes;

namespace JosephM.Core.Service
{
    public interface IAllowSaveAndLoad
    {
        bool Autoload { get; set; }
        string Name { get; set; }

        [Hidden]
        bool DisplaySavedSettingFields { get; set; }
    }

    [Group(Sections.Main, Group.DisplayLayoutEnum.VerticalCentered)]
    [Instruction("Enter A Name For Your Saved Input And Optionally Elect To Autoload The Input When This Process Is Run")]
    public class SaveAndLoadFields : IAllowSaveAndLoad
    {
        [RequiredProperty]
        [Group(Sections.Main)]
        [DisplayOrder(1)]
        public bool Autoload { get; set; }

        [RequiredProperty]
        [Group(Sections.Main)]
        [DisplayOrder(2)]
        public string Name { get; set; }

        [Hidden]
        public bool DisplaySavedSettingFields { get; set; }

        private static class Sections
        {
            public const string Main = "Saved Input Details";
        }
    }
}