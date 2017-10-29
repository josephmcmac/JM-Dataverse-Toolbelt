using JosephM.Core.Test;
using JosephM.Xrm.Vsix.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace JosephM.Xrm.Vsix.Test
{
    public class FakeVisualStudioService : VisualStudioServiceBase
    {
        public override string SolutionDirectory { get { return Path.Combine(TestConstants.TestFolder, "FakeVSSolutionFolder"); } }

        public override string BuildSelectedProjectAndGetAssemblyName()
        {
            return GetTestPluginAssemblyFile();
        }

        public static string GetTestPluginAssemblyFile()
        {
            var rootFolder = GetRootFolder();
            var pluginAssembly = Path.Combine(rootFolder.FullName, "TestFiles", "PluginAssemblyBin",
                PluginAssemblyName + ".dll");
            return pluginAssembly;
        }

        public static string PluginAssemblyName
        {
            get { return "TestXrmSolution.Plugins"; }
        }

        public static DirectoryInfo GetRootFolder()
        {

            var assemblyLocation = new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath;
            var fileInfo = new FileInfo(assemblyLocation);
            var rootFolder = fileInfo.Directory.Parent.Parent;
            return rootFolder;
        }

        public override IEnumerable<string> GetSelectedFileNamesQualified()
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<IVisualStudioItem> GetSelectedItems()
        {
            return _selectedItems;
        }

        public override string GetSelectedProjectAssemblyName()
        {
            return PluginAssemblyName;
        }

        public override IEnumerable<IVisualStudioProject> GetSolutionProjects()
        {
            return new IVisualStudioProject[0];
        }

        private List<IVisualStudioItem> _selectedItems = new List<IVisualStudioItem>();

        internal void SetSelectedItem(IVisualStudioItem item)
        {
            _selectedItems.Clear();
            _selectedItems.Add(item);
        }

        protected override ISolutionFolder AddSolutionFolder(string name)
        {
            var qualifiedDirectory = Path.Combine(SolutionDirectory, name);
            if (!Directory.Exists(qualifiedDirectory))
                Directory.CreateDirectory(qualifiedDirectory);
            return new FakeVisualStudioSolutionFolder(qualifiedDirectory);
        }

        protected override ISolutionFolder GetSolutionFolder(string name)
        {
            var qualifiedDirectory = Path.Combine(SolutionDirectory, name);
            if (!Directory.Exists(qualifiedDirectory))
                throw new NullReferenceException("Could not find directory " + qualifiedDirectory);
            return new FakeVisualStudioSolutionFolder(qualifiedDirectory);
        }
    }
}