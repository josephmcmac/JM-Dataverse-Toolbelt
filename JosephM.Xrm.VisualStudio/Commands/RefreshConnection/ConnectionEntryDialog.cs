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
        public Solution2 Solution { get; set; }
        public string Directory { get; set; }

        public ConnectionEntryDialog(IDialogController dialogController, XrmRecordConfiguration objectToEnter, Solution2 solution, string directory)
            : base(dialogController, objectToEnter)
        {
            Solution = solution;
            Directory = directory;
        }

        protected override void LoadDialogExtention()
        {
            StartNextAction();
        }

        protected override void CompleteDialogExtention()
        {
            var directory = Directory;
            var dictionary = new Dictionary<string, string>();
            foreach (var prop in XrmRecordConfiguration.GetType().GetReadWriteProperties())
            {
                var value = XrmRecordConfiguration.GetPropertyValue(prop.Name);
                dictionary.Add(prop.Name, value == null ? null : value.ToString());
            }
            var serialised = JsonHelper.ObjectToJsonString(dictionary);

            var project = VsixUtility.AddSolutionFolder(Solution, "SolutionItems");
            var solutionItemsFolder = directory + @"\SolutionItems";
            var connectionFileName = "solution.xrmconnection";
            FileUtility.WriteToFile(solutionItemsFolder, connectionFileName, serialised);
            VsixUtility.AddProjectItem(project.ProjectItems, Path.Combine(solutionItemsFolder, connectionFileName));

            Project testProject = null;
            foreach (Project item in Solution.Projects)
            {
                if (item.Name.EndsWith(".Test"))
                    testProject = item;
            }
            if (testProject != null)
            {
                var linkedConnectionItem = VsixUtility.AddProjectItem(testProject.ProjectItems, Path.Combine(solutionItemsFolder, connectionFileName));

                foreach (Property prop in linkedConnectionItem.Properties)
                {
                    if (prop.Name == "CopyToOutputDirectory")
                    {
                        prop.Value = 1;
                    }
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