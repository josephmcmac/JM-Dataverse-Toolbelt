using JosephM.Core.Attributes;
using JosephM.Core.FieldType;

namespace JosephM.ObjectEncryption
{
    public class ObjectEncryptToFolder
    {
        [RequiredProperty]
        public Folder SaveToFolder { get; set; }
    }
}
