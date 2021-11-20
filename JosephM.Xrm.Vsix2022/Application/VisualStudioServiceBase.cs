using JosephM.Core.Extentions;
using JosephM.Core.Serialisation;
using JosephM.Core.Utility;
using JosephM.Record.Xrm.XrmRecord;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace JosephM.Xrm.Vsix.Application
{
    public abstract class VisualStudioServiceBase : IVisualStudioService
    {
        public abstract string SolutionDirectory { get; }

        public virtual void AddFolder(string folderDirectory)
        {
            if (folderDirectory == null)
                throw new ArgumentNullException(nameof(folderDirectory));

            var solutionDirectory = SolutionDirectory;

            if (!folderDirectory.StartsWith(solutionDirectory))
                throw new ArgumentOutOfRangeException(nameof(folderDirectory), "Required to be in solution directory - " + solutionDirectory);

            var subPath = folderDirectory.Substring(solutionDirectory.Length);

            var subDirectories = subPath.Split(Path.DirectorySeparatorChar).Where(s => !string.IsNullOrWhiteSpace(s)).ToArray();

            ISolutionFolder carryProject = null;
            foreach (var subFolder in subDirectories)
            {
                if (carryProject == null)
                {
                    carryProject = AddSolutionFolder(subFolder);
                }
                else
                {
                    carryProject = carryProject.AddSubFolder(subFolder);
                }
            }
            carryProject.CopyFilesIntoSolutionFolder(folderDirectory);
        }

        public string AddVsixSetting(string name, string serialised)
        {
            var project = AddSolutionFolder(ItemFolderName);
            var solutionItemsFolder = SolutionDirectory + @"\" + ItemFolderName;
            FileUtility.WriteToFile(solutionItemsFolder, name, serialised);
            project.AddProjectItem(Path.Combine(solutionItemsFolder, name));
            return Path.Combine(solutionItemsFolder, name);
        }

        protected abstract ISolutionFolder AddSolutionFolder(string name);

        public string AddVsixSetting<T>(string name, T objectToSerialise)
        {
            if (objectToSerialise is XrmRecordConfiguration)
            {
                var dictionary = new Dictionary<string, string>();
                foreach (var prop in objectToSerialise.GetType().GetReadWriteProperties())
                {
                    var value = objectToSerialise.GetPropertyValue(prop.Name);
                    dictionary.Add(prop.Name, value?.ToString());
                }
                var serialised = JsonHelper.ObjectToJsonString(dictionary, format: true);

                var solutionItemFile = AddVsixSetting(name, serialised);

                return solutionItemFile;
            }
            else
            {
                var json = JsonHelper.ObjectAsTypeToJsonString(objectToSerialise, format: true);
                return AddVsixSetting(name, json);
            }
        }

        public string AddVsixSetting(string fileQualified)
        {
            var project = AddSolutionFolder(ItemFolderName);
            if (fileQualified.StartsWith(SolutionDirectory))
            {
                var subString = fileQualified.Substring(SolutionDirectory.Length + 1);
                if (subString.Contains(@"\"))
                {
                    var folder = subString.Substring(0, subString.LastIndexOf(@"\"));
                    FileUtility.CheckCreateFolder(SolutionDirectory + @"\" + folder);
                    project = AddSolutionFolder(folder);
                }
            }
            project.AddProjectItem(fileQualified);
            return fileQualified;
        }

        public abstract string BuildSelectedProjectAndGetAssemblyName();

        public abstract IEnumerable<string> GetSelectedFileNamesQualified();

        public abstract void SaveSelectedFiles();

        public abstract IEnumerable<IVisualStudioItem> GetSelectedItems();

        public abstract string GetSelectedProjectAssemblyName();

        public string GetVsixSettingText(string name)
        {
            //chnaged folder used so also check old folder
            //if new one not present
            var folderNames = new[]
            {
                ItemFolderName,
                "SolutionItems"
            };

            foreach (var possibleFolder in folderNames)
            {
                var text = GetItemText(name, possibleFolder);
                if (text != null)
                    return text;
            }
            return null;
        }

        public string ItemFolderName
        {
            get
            {
                return "Xrm.Vsix";
            }
        }

        public string GetItemText(string name, string folderName)
        {
            string fileName = null;
            var solutionItems = GetSolutionFolder(folderName);
            if (solutionItems == null)
                return null;
            foreach (var item in solutionItems.ProjectItems)
            {
                if (item.Name?.ToLower() == name?.ToLower())
                {
                    fileName = item.FileName;
                }
            }
            if (fileName == null || !File.Exists(fileName))
            {
                return null;
            }
            return File.ReadAllText(fileName);
        }

        public abstract ISolutionFolder GetSolutionFolder(string solutionFolderName);
        public abstract IVisualStudioProject GetProject(string name);
        public abstract string GetSolutionName();
        public abstract IEnumerable<IVisualStudioProject> GetProjects();
    }
}
