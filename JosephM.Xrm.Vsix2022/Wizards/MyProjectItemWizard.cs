using EnvDTE;
using JosephM.Xrm.Vsix.Application;
using JosephM.Xrm.Vsix.Module.PackageSettings;
using JosephM.Xrm.Vsix.Wizards;
using System;
using System.Collections.Generic;

namespace JosephM.XRM.VSIX.Wizards
{
    public class MyProjectItemWizard : MyWizardBase
    {
        private VisualStudioService _visualStudioService;
        protected VisualStudioService VisualStudioService
        {
            get
            {
                if (_visualStudioService == null)
                {
                    _visualStudioService = new VisualStudioService(DTE);
                }
                return _visualStudioService;
            }
        }

        public override void RunStartedExtention(Dictionary<string, string> replacementsDictionary)
        {
            AddRootNamespaceReplacement(replacementsDictionary);

            var vsixSettingsManager = new VsixSettingsManager(VisualStudioService, null);
            var packageSettings = vsixSettingsManager.Resolve<XrmPackageSettings>();
            if (packageSettings == null)
                throw new NullReferenceException("packageSettings");

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
                    var theNamespace = new VisualStudioProject(project).GetProperty("RootNamespace");
                    if (!replacementsDictionary.ContainsKey("$josephmrootnamespace$"))
                        replacementsDictionary.Add("$josephmrootnamespace$", theNamespace);
                }
            }
        }
    }
}
