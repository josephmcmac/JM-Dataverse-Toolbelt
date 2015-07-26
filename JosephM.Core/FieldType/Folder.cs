using System.IO;
using System.Runtime.Serialization;
using JosephM.Core.Attributes;
using JosephM.Core.Service;

namespace JosephM.Core.FieldType
{
    [DataContract]
    public class Folder : IValidatableObject
    {
        public Folder(string folderPath)
        {
            FolderPath = folderPath;
        }

        [DataMember]
        public string FolderPath { get; set; }

        public override string ToString()
        {
            return FolderPath;
        }

        public IsValidResponse Validate()
        {
            var response = new IsValidResponse();
            if(!Directory.Exists(FolderPath))
                response.AddInvalidReason("The Directory Does Not Exist");
            return response;
        }
    }
}