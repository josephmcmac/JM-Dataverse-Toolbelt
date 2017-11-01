using EnvDTE;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JosephM.Xrm.Vsix.Utilities
{
    public class VisualStudioProject : VisualStudioProjectBase, IVisualStudioProject
    {
        public VisualStudioProject(Project project)
            : base(project)
        {
        }
    }
}
