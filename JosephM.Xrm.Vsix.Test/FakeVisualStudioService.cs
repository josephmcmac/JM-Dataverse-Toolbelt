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
        }

        private string _solutionDirectory;
        public override string SolutionDirectory { get { return _solutionDirectory; } }

        public override string BuildSelectedProjectAndGetAssemblyName()
        {
            return GetTestPluginAssemblyFile();
        }

        public static string GetTestPluginAssemblyFile()
        {
            var rootFolder = GetActualSolutionRootFolder();
            var pluginAssembly = Path.Combine(rootFolder.FullName, "SolutionItems", "TestPluginAssemblyBin",
                PluginAssemblyName + ".dll");
            return pluginAssembly;
        }

        public static string PluginAssemblyName
        {
            get { return "TestXrmSolution.Plugins"; }
        }

        public static DirectoryInfo GetActualSolutionRootFolder()
        {
            var rootFolderName = "XRM-Developer-Tool";
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

        public override IEnumerable<IVisualStudioItem> GetSelectedItems()
        {
            return _selectedItems;
        }

        public override string GetSelectedProjectAssemblyName()
        {
            return PluginAssemblyName;
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

        internal void SetSelectedItem(IVisualStudioItem item)
        {
            _selectedItems.Clear();
            _selectedItems.Add(item);
        }

        internal void SetSelectedItems(IEnumerable<IVisualStudioItem> items)
        {
            _selectedItems.Clear();
            _selectedItems.AddRange(items);
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
    }
}