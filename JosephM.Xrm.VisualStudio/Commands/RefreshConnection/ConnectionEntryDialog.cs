using System.Collections.Generic;
using System.IO;
using EnvDTE;
using EnvDTE80;
using JosephM.Application.ViewModel.Dialog;
using JosephM.Core.Extentions;
using JosephM.Core.Serialisation;
using JosephM.Core.Utility;
using JosephM.Record.Xrm.XrmRecord;
using JosephM.XRM.VSIX.Dialogs;
using JosephM.XRM.VSIX.Utilities;

namespace JosephM.XRM.VSIX.Commands.RefreshConnection
{
    public class ConnectionEntryDialog : VsixEntryDialog
    {
        public IVisualStudioService VisualStudioService { get; set; }
        public ConnectionEntryDialog(IDialogController dialogController, XrmRecordConfiguration objectToEnter, IVisualStudioService visualStudioService)
            : base(dialogController, objectToEnter)
        {
            VisualStudioService = visualStudioService;
        }

        protected override void LoadDialogExtention()
        {
            StartNextAction();
        }

        protected override void CompleteDialogExtention()
        {
            var dictionary = new Dictionary<string, string>();
            foreach (var prop in XrmRecordConfiguration.GetType().GetReadWriteProperties())
            {
                var value = XrmRecordConfiguration.GetPropertyValue(prop.Name);
                dictionary.Add(prop.Name, value == null ? null : value.ToString());
            }
            var serialised = JsonHelper.ObjectToJsonString(dictionary);

            var connectionFileName = "solution.xrmconnection";
            var file = VisualStudioService.AddSolutionItem(connectionFileName, serialised);

            foreach (var item in VisualStudioService.GetSolutionProjects())
            {
                if (item.Name.EndsWith(".Test"))
                {
                    var linkedConnectionItem = item.AddProjectItem(file);
                    linkedConnectionItem.SetProperty("CopyToOutputDirectory", 1);
                }
            }
            CompletionMessage = "Connection Refreshed";
        }

        private XrmRecordConfiguration XrmRecordConfiguration
        {
            get
            {
                return EnteredObject as XrmRecordConfiguration;
            }
        }
    }
}