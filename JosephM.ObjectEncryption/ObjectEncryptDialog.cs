using System.IO;
using System.Runtime.Serialization;
using JosephM.Application.ViewModel.Dialog;
using JosephM.Core.Security;
using JosephM.Core.Utility;

namespace JosephM.ObjectEncryption
{
    public class ObjectEncryptDialog<T> : DialogViewModel
        where T : new()
    {
        private T EnteredObject { get; set; }
        private ObjectEncryptToFolder SaveTo { get; set; }

        public ObjectEncryptDialog(IDialogController dialogController)
            : base(dialogController)
        {
            EnteredObject = new T();
            SaveTo = new ObjectEncryptToFolder();
            var configEntryDialog = new ObjectEntryDialog(EnteredObject, this, ApplicationController);
            var saveToFolderDialog = new ObjectEntryDialog(SaveTo, this, ApplicationController);
            SubDialogs = new DialogViewModel[] { configEntryDialog, saveToFolderDialog };
        }

        protected override void LoadDialogExtention()
        {
            StartNextAction();
        }

        protected override void CompleteDialogExtention()
        {
            //stucture this and use app to reference in the test project

            // save to the setting exists in the settings folder then get it
            var folder = ApplicationController.SettingsPath;
            FileUtility.CheckCreateFolder(folder);
            var xmlString = DataContractSerializeObject(EnteredObject);
            var encrypt = StringEncryptor.Encrypt(xmlString);
            FileUtility.CheckCreateFolder(SaveTo.SaveToFolder.FolderPath);
            FileUtility.WriteToFile(SaveTo.SaveToFolder.FolderPath, typeof(T).Name + ".xml", encrypt);

            CompletionMessage = "The Object Has Been Encrypted";
        }

        public static string DataContractSerializeObject<T>(T objectToSerialize)
        {
            using (MemoryStream memStm = new MemoryStream())
            {
                var serializer = new DataContractSerializer(typeof(T));
                serializer.WriteObject(memStm, objectToSerialize);

                memStm.Seek(0, SeekOrigin.Begin);

                using (var streamReader = new StreamReader(memStm))
                {
                    string result = streamReader.ReadToEnd();
                    return result;
                }
            }
        }
    }
}