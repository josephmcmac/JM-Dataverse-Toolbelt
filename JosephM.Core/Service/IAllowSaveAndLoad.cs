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
        [RequiredProperty]
        [DisplayOrder(1)]
        public bool Autoload { get; set; }

        [RequiredProperty]
        [DisplayOrder(2)]
        public string Name { get; set; }
    }
}