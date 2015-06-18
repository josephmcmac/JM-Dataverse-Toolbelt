using System.IO;
using System.Runtime.Serialization;
using JosephM.Core.Test;
using JosephM.Core.Utility;
using JosephM.Record.Application.Dialog;
using JosephM.Xrm.Test;

namespace JosephM.Prism.XrmTestModule.TestXrmSettingsDialog
{
    public class XrmSettingsDialog : DialogViewModel
    {
        public XrmSettingsDialog(IDialogController dialogController)
            : base(dialogController)
        {
            SettingsObject = new EncryptedXrmConfiguration();
            var configEntryDialog = new ObjectEntryDialog(SettingsObject, this, ApplicationController, null,
                null);

            SubDialogs = new DialogViewModel[] { configEntryDialog };
        }

        private EncryptedXrmConfiguration SettingsObject { get; set; }

        protected override void LoadDialogExtention()
        {
            StartNextAction();
        }

        protected override void CompleteDialogExtention()
        {
            var type = SettingsObject.GetType();
            var serializer = new DataContractSerializer(type);

            var folder = TestConstants.TestSettingsFolder;
            FileUtility.CheckCreateFolder(folder);

            using (
                var fileStream = new FileStream(Xrm.Test.XrmTest.TestXrmConnectionFileName,
                    FileMode.Create))
            {
                serializer.WriteObject(fileStream, SettingsObject);
            }
            CompletionMessage = "The Settings Have Been Saved To " + Xrm.Test.XrmTest.TestXrmConnectionFileName;
        }
    }
}