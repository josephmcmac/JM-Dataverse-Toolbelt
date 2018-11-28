using JosephM.Core.Utility;
using JosephM.Xrm.Vsix.Application;
using System;
using System.IO;

namespace JosephM.Xrm.Vsix.Test
{
    public class FakeVisualStudioProject : IVisualStudioProject
    {
        public FakeVisualStudioProject(string directory)
        {
            Directory = directory;
        }

        public string Name => new DirectoryInfo(Directory).Name;

        private string Directory { get; set; }

        public IProjectItem AddProjectItem(string file)
        {
            throw new NotImplementedException();
        }

        public void AddItem(string fileName, string fileContent, params string[] folderPath)
        {
            var thisDirectory = new DirectoryInfo(Directory);
            var subFolders = thisDirectory.GetDirectories();
            if (folderPath != null)
            {
                foreach (var path in folderPath)
                {
                    DirectoryInfo thisPartSubDirectory = null;
                    foreach (var subFolder in subFolders)
                    {
                        if (subFolder.Name == path)
                            thisPartSubDirectory = subFolder;
                    }
                    if (thisPartSubDirectory == null)
                        thisPartSubDirectory = System.IO.Directory.CreateDirectory(Path.Combine(thisDirectory.FullName, path));
                    thisDirectory = thisPartSubDirectory;
                }
            }
            FileUtility.WriteToFile(thisDirectory.FullName, fileName, fileContent);
        }
    }
}