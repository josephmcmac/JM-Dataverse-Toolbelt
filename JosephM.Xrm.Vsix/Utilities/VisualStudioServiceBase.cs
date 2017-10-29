using JosephM.Core.Extentions;
using JosephM.Core.Serialisation;
using JosephM.Core.Utility;
using JosephM.Record.Xrm.XrmRecord;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace JosephM.Xrm.Vsix.Utilities
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
            //todo create the folder paths in the solution folders
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

        public string AddSolutionItem(string name, string serialised)
        {
            var project = AddSolutionFolder("SolutionItems");
            var solutionItemsFolder = SolutionDirectory + @"\SolutionItems";
            FileUtility.WriteToFile(solutionItemsFolder, name, serialised);
            project.AddProjectItem(Path.Combine(solutionItemsFolder, name));
            return Path.Combine(solutionItemsFolder, name);
        }

        protected abstract ISolutionFolder AddSolutionFolder(string name);

        public string AddSolutionItem<T>(string name, T objectToSerialise)
        {
            if (objectToSerialise is XrmRecordConfiguration)
            {
                var dictionary = new Dictionary<string, string>();
                foreach (var prop in objectToSerialise.GetType().GetReadWriteProperties())
                {
                    var value = objectToSerialise.GetPropertyValue(prop.Name);
                    dictionary.Add(prop.Name, value == null ? null : value.ToString());
                }
                name = "solution.xrmconnection";
                var serialised = JsonHelper.ObjectToJsonString(dictionary);

                return AddSolutionItem(name, serialised);
            }
            else
            {
                var json = JsonHelper.ObjectAsTypeToJsonString(objectToSerialise);
                return AddSolutionItem(name, json);
            }
        }

        public string AddSolutionItem(string fileQualified)
        {
            var project = AddSolutionFolder("SolutionItems");
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

        public abstract IEnumerable<IVisualStudioItem> GetSelectedItems();

        public abstract string GetSelectedProjectAssemblyName();

        public string GetSolutionItemText(string name)
        {
            string fileName = null;
            var solutionItems = GetSolutionFolder("SolutionItems");
            if (solutionItems == null)
                return null;
            foreach (var item in solutionItems.ProjectItems)
            {
                if (item.Name == name)
                {
                    fileName = item.FileName;
                }
            }
            if (fileName == null)
                return null;
            return File.ReadAllText(fileName);
        }

        protected abstract ISolutionFolder GetSolutionFolder(string solutionFolderName);

        public abstract IEnumerable<IVisualStudioProject> GetSolutionProjects();
    }
}
