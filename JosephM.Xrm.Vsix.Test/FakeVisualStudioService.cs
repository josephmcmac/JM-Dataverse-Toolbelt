using System.Collections.Generic;
using JosephM.XRM.VSIX.Utilities;

namespace JosephM.Xrm.Vsix.Test
{
    public class FakeVisualStudioService : IVisualStudioService
    {
        public string SolutionDirectory { get { return null; } }
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