using System.Collections.Generic;
using JosephM.XRM.VSIX.Utilities;
using JosephM.Core.Test;
using System.IO;

namespace JosephM.Xrm.Vsix.Test
{
    public class FakeVisualStudioService : IVisualStudioService
    {
        //WARNING THIS IS CLEARED EVERY TEST SCRIPT !!
        public string SolutionDirectory { get { return Path.Combine(TestConstants.TestFolder, "FakeVSSolutionFolder"); } }

        public void AddFolder(string folderDirectory)
        {
        }

        public string AddSolutionItem(string connectionFileName, string serialised)
        {
            return null;
        }

        public string AddSolutionItem<T>(string name, T objectToSerialise)
        {
            return null;
        }

        public string AddSolutionItem(string fileName)
        {
            return null;
        }

        public IEnumerable<IVisualStudioProject> GetSolutionProjects()
        {
            return new IVisualStudioProject[0];
        }
    }
}