using JosephM.Core.Test;
using JosephM.Xrm.Vsix.Application;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace JosephM.Xrm.Vsix.Test
{
    public class FakeVisualStudioService : VisualStudioServiceBase
    {
        public FakeVisualStudioService(string solutionDirectory = null)
        {
            _solutionDirectory = solutionDirectory ?? Path.Combine(TestConstants.TestFolder, "FakeVSSolutionFolder");

            var fakeProjectName = "FakeProject";
            var rootFolder = GetActualSolutionRootFolder();
            _selectedPluginAssembly = Path.Combine(rootFolder.FullName, "SolutionItems", "TestPluginAssemblyBin", "TestXrmSolution.Plugins.dll");
            var fakeProjectFolderPath = Path.Combine(SolutionDirectory, fakeProjectName);
            Directory.CreateDirectory(fakeProjectFolderPath);
            SetSelectedItem(new FakeVisualStudioSolutionFolder(fakeProjectFolderPath));
        }

        private string _solutionDirectory;
        public override string SolutionDirectory { get { return _solutionDirectory; } }

        public override string BuildSelectedProjectAndGetAssemblyName(bool addIlMergePath)
        {
            return GetTestPluginAssemblyFile();
        }

        public string GetTestPluginAssemblyFile()
        {
            return _selectedPluginAssembly;
        }

        public string PluginAssemblyName
        {
            get
            {
                var fileInfo = new FileInfo(_selectedPluginAssembly);
                return fileInfo.Name.Substring(0, fileInfo.Name.Length - 4);
            }
        }

        public static DirectoryInfo GetActualSolutionRootFolder()
        {
            var rootFolderName = "JM-Dataverse-Toolbelt";
            var fileInfo = new FileInfo(Assembly.GetExecutingAssembly().CodeBase.Substring(8));
            var directory = fileInfo.Directory;
            while (directory.Name != rootFolderName)
            {
                directory = directory.Parent;
                if (directory == null)
                    throw new NullReferenceException("Could not find solution root folder of name '" + rootFolderName + "' in " + fileInfo.FullName);
            }
            return directory;
        }

        public override IEnumerable<string> GetSelectedFileNamesQualified()
        {
            return _selectedItems.Cast<FakeVisualStudioProjectItem>().Select(i => i.FileName).ToArray();
        }

        public override void SaveSelectedFiles()
        {
        }

        public override IEnumerable<IVisualStudioItem> GetSelectedItems()
        {
            return _selectedItems;
        }

        public override string GetSelectedProjectAssemblyName()
        {
            return PluginAssemblyName;
        }

        public override string GetSelectedProjectProperty(string propertyName)
        {
            return null;
        }

        public override string GetSelectedProjectDirectory()
        {
            return null;
        }

        public override IVisualStudioProject GetProject(string name)
        {
            foreach(var folder in Directory.GetDirectories(SolutionDirectory))
            {
                var directory = new DirectoryInfo(folder);
                if (directory.Name == name)
                    return new FakeVisualStudioProject(folder);
            }
            throw new NullReferenceException("Couldnt find folder named " + name);
        }

        private List<IVisualStudioItem> _selectedItems = new List<IVisualStudioItem>();
        private string _selectedPluginAssembly;

        internal void SetSelectedItem(IVisualStudioItem item)
        {
            _selectedItems.Clear();
            _selectedItems.Add(item);
        }

        public void SetSelectedItems(IEnumerable<IVisualStudioItem> items)
        {
            _selectedItems.Clear();
            _selectedItems.AddRange(items);
        }
        public void SetSelectedProjectAssembly(string assemblyFile)
        {
            _selectedPluginAssembly = assemblyFile;
        }


        protected override ISolutionFolder AddSolutionFolder(string name)
        {
            var qualifiedDirectory = Path.Combine(SolutionDirectory, name);
            if (!Directory.Exists(qualifiedDirectory))
                Directory.CreateDirectory(qualifiedDirectory);
            return new FakeVisualStudioSolutionFolder(qualifiedDirectory);
        }

        public override ISolutionFolder GetSolutionFolder(string name)
        {
            var qualifiedDirectory = Path.Combine(SolutionDirectory, name);
            if (!Directory.Exists(qualifiedDirectory))
               return null;
            return new FakeVisualStudioSolutionFolder(qualifiedDirectory);
        }

        public override string GetSolutionName()
        {
            return new DirectoryInfo(SolutionDirectory).Name;
        }

        public override IEnumerable<IVisualStudioProject> GetProjects()
        {
            return Directory.GetDirectories(SolutionDirectory).Select(di => new FakeVisualStudioProject(di)).ToArray();
        }
    }
}