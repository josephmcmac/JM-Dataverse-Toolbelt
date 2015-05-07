using System.IO;
using JosephM.Core.Attributes;
using JosephM.Core.Service;

namespace JosephM.Core.FieldType
{
    public class Folder : IValidatableObject
    {
        public Folder(string folderPath)
        {
            FolderPath = folderPath;
        }

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