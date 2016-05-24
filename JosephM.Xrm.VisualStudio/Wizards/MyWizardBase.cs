using System.Collections.Generic;
using EnvDTE;
using EnvDTE80;
using JosephM.XRM.VSIX.Utilities;
using Microsoft.VisualStudio.TemplateWizard;

namespace JosephM.XRM.VSIX.Wizards
{
    public class MyWizardBase : IWizard
    {
        protected void AddReplacements(Dictionary<string, string> replacementsDictionary,
            XrmPackageSettings xrmPackageSettings)
        {
            if (!string.IsNullOrWhiteSpace(xrmPackageSettings.SolutionObjectPrefix) &&
                !replacementsDictionary.ContainsKey("$jmobjprefix$"))
                replacementsDictionary.Add("$jmobjprefix$", xrmPackageSettings.SolutionObjectPrefix);
            if (!string.IsNullOrWhiteSpace(xrmPackageSettings.SolutionObjectInstancePrefix) &&
                !replacementsDictionary.ContainsKey("$jminstprefix$"))
                replacementsDictionary.Add("$jminstprefix$", xrmPackageSettings.SolutionObjectInstancePrefix);
            if (!string.IsNullOrWhiteSpace(xrmPackageSettings.SolutionDynamicsCrmPrefix) &&
                !replacementsDictionary.ContainsKey("$jmxrmprefix$"))
                replacementsDictionary.Add("$jmxrmprefix$", xrmPackageSettings.SolutionDynamicsCrmPrefix);
        }

        protected DTE2 DTE { get; set; }
        public void RunStarted(object automationObject, Dictionary<string, string> replacementsDictionary, WizardRunKind runKind, object[] customParams)
        {
            DTE = automationObject as DTE2;
            RunStartedExtention(replacementsDictionary);
        }

        public void ProjectFinishedGenerating(Project project)
        {
        }

        public void ProjectItemFinishedGenerating(ProjectItem projectItem)
        {
        }

        public bool ShouldAddProjectItem(string filePath)
        {
            return true;
        }

        public void BeforeOpeningFile(ProjectItem projectItem)
        {
        }

        public void RunFinished()
        {
            RunFinishedExtention();
        }

        public virtual void RunStartedExtention(Dictionary<string, string> replacementsDictionary)
        {
            
        }

        public virtual void RunFinishedExtention()
        {
        }
    }
}