using EnvDTE;

namespace JosephM.Xrm.Vsix.Application
{
    public class VisualStudioProject : VisualStudioProjectBase, IVisualStudioProject
    {
        public VisualStudioProject(Project project)
            : base(project)
        {
        }

        public string GetProperty(string name)
        {
            foreach (Property prop in Project.Properties)
            {
                if (prop.Name == name)
                {
                    return prop.Value == null ? null : prop.Value.ToString();
                }
            }
            return null;
        }
    }
}
