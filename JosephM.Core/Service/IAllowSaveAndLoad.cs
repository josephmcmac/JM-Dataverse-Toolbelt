using JosephM.Core.Attributes;

namespace JosephM.Core.Service
{
    public interface IAllowSaveAndLoad
    {
        bool Autoload { get; set; }
        string Name { get; set; }
    }

    public class SaveAndLoadFields : IAllowSaveAndLoad
    {
        [DisplayOrder(1)]
        public bool Autoload { get; set; }

        [DisplayOrder(2)]
        public string Name { get; set; }
    }
}