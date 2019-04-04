using JosephM.Xrm.Vsix.Application;
using System;
using System.IO;

namespace JosephM.Xrm.Vsix.Test
{
    public class FakeVisualStudioProjectItem : IProjectItem
    {
        public FakeVisualStudioProjectItem(string fileName)
        {
            FileName = fileName;
        }

        public string FileFolder
        {
            get
            {
                var fileInfo = new FileInfo(FileName);
                return fileInfo.DirectoryName;
            }
        }

        public string FileName { get; set; }

        public string Name
        {
            get
            {
                var fileInfo = new FileInfo(FileName);
                return fileInfo.Name;
            }
        }

        public void SetProperty(string propertyName, object value)
        {
            
        }

        public string NameOfContainingProject
        {
            get
            {
                return null;
            }
        }
    }
}
