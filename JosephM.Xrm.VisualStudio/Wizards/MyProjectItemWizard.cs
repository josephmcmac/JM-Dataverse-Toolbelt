using System.Collections.Generic;
using EnvDTE;
using EnvDTE80;
using JosephM.XRM.VSIX.Utilities;
using Microsoft.VisualStudio.TemplateWizard;

namespace JosephM.XRM.VSIX.Wizards
{
    public class MyProjectItemWizard : MyWizardBase
    {
        public override void RunStartedExtention(Dictionary<string, string> replacementsDictionary)
        {
            AddRootNamespaceReplacement(replacementsDictionary);

            var packageSettings = VsixUtility.GetPackageSettings(DTE);

            AddReplacements(replacementsDictionary, packageSettings);
        }

        private void AddRootNamespaceReplacement(Dictionary<string, string> replacementsDictionary)
        {
            var selectedItems = DTE.SelectedItems;
            foreach (SelectedItem item in selectedItems)
            {
                var project = item.Project ?? item.ProjectItem.ContainingProject;
                if (project != null)
                {
                    var theNamespace = VsixUtility.GetProperty(project.Properties, "RootNamespace");
                    if (!replacementsDictionary.ContainsKey("$josephmrootnamespace$"))
                        replacementsDictionary.Add("$josephmrootnamespace$", theNamespace);
                }
            }
        }
    }
}
