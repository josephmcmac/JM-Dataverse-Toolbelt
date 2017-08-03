using JosephM.Record.Xrm.XrmRecord;
using JosephM.XRM.VSIX.Commands.RefreshConnection;
using JosephM.XRM.VSIX.Dialogs;
using JosephM.XRM.VSIX.Utilities;
using System.Collections.Generic;
using JosephM.XRM.VSIX.Commands.PackageSettings;
using System.IO;
using EnvDTE80;
using System;
using EnvDTE;
using System.Linq;
using VSLangProj;

namespace JosephM.XRM.VSIX.Wizards
{
    public class XrmSolutionWizard : MyWizardBase
    {
        public XrmRecordConfiguration XrmRecordConfiguration { get; set; }
        public XrmPackageSettings XrmPackageSettings { get; set; }
        public string DestinationDirectory { get; private set; }
        public string SafeProjectName { get; private set; }

        public override void RunStartedExtention(Dictionary<string, string> replacementsDictionary)
        {
            //get a xrm connection and package setting by loading the entry dialogs
            var xrmConfig = new XrmRecordConfiguration();
            #if DEBUG
                        xrmConfig.AuthenticationProviderType = XrmRecordAuthenticationProviderType.ActiveDirectory;
                        xrmConfig.DiscoveryServiceAddress = "http://qa2012/XRMServices/2011/Discovery.svc";
                        xrmConfig.Name = "TEST";
                        xrmConfig.OrganizationUniqueName = "TEST";
                        xrmConfig.Username = "joseph";
                        xrmConfig.Domain = "auqa2012";
            #endif
            var dialog = new ConnectionEntryDialog(DialogUtility.CreateDialogController(), xrmConfig,
                VisualStudioService, false);

            DialogUtility.LoadDialog(dialog, showCompletion: false, isModal: true);
            XrmRecordConfiguration = xrmConfig;

            XrmPackageSettings = new XrmPackageSettings();
            #if DEBUG
                XrmPackageSettings.SolutionDynamicsCrmPrefix = "template";
                XrmPackageSettings.SolutionObjectPrefix = "Template";
            #endif
            var settingsDialog = new XrmPackageSettingDialog(DialogUtility.CreateDialogController(), XrmPackageSettings, VisualStudioService, false, new XrmRecordService(XrmRecordConfiguration));
            DialogUtility.LoadDialog(settingsDialog, showCompletion: false, isModal: true);

            //add token replacements for the template projects
            AddReplacements(replacementsDictionary, XrmPackageSettings);

            //used later
            DestinationDirectory = replacementsDictionary["$destinationdirectory$"];
            SafeProjectName = replacementsDictionary["$safeprojectname$"];
        }

        public override void RunFinishedExtention()
        {
            if (DestinationDirectory.EndsWith(SafeProjectName + Path.DirectorySeparatorChar + SafeProjectName))
            {
                //The projects were created under a seperate folder -- lets fix it

                //first move each projects up a directory
                var projectsObjects = new List<Project>();
                foreach (Project childProject in DTE.Solution.Projects)
                {
                    var fileName = childProject.FileName;
                    if (!string.IsNullOrEmpty(fileName)) //Solution Folder
                    {
                        var projectBadPath = fileName;
                        var projectGoodPath = projectBadPath.Replace(
                            SafeProjectName + Path.DirectorySeparatorChar + SafeProjectName + Path.DirectorySeparatorChar,
                            SafeProjectName + Path.DirectorySeparatorChar);

                        DTE.Solution.Remove(childProject);

                        Directory.Move(Path.GetDirectoryName(projectBadPath), Path.GetDirectoryName(projectGoodPath));

                        DTE.Solution.AddFromFile(projectGoodPath);
                    }
                }
                //now add the references to the plugin project
                //because they got removed when we move the project folders
                Project pluginProject = null;
                foreach (Project childProject in DTE.Solution.Projects)
                {
                    var fileName = childProject.FileName;
                    if (!string.IsNullOrEmpty(fileName)) //Solution Folder
                    {
                        if (fileName.EndsWith(".Plugins.csproj"))
                        {
                            pluginProject = childProject;
                        }
                    }
                }
                foreach (Project childProject in DTE.Solution.Projects)
                {
                    var fileName = childProject.FileName;
                    if (!string.IsNullOrEmpty(fileName)) //Solution Folder
                    {
                        if (fileName.EndsWith(".Test.csproj")
                            || fileName.EndsWith(".Console.csproj"))
                        {
                            VSProject vsProj = (VSProject)childProject.Object;
                            vsProj.References.AddProject(pluginProject);
                        }
                    }
                }
            }

            if (DestinationDirectory.EndsWith(SafeProjectName + Path.DirectorySeparatorChar + SafeProjectName))
                DestinationDirectory = DestinationDirectory.Substring(0, DestinationDirectory.Length - (Path.DirectorySeparatorChar + SafeProjectName).Length);

            var consoleFileName = DestinationDirectory + Path.DirectorySeparatorChar + SafeProjectName + ".Console" + Path.DirectorySeparatorChar + "Encrypt XRM Connection.bat";
            if(File.Exists(consoleFileName))
            {
                var read = File.ReadAllText(consoleFileName);
                read = read.Replace("$ext_safeprojectname$", SafeProjectName);
                File.WriteAllText(consoleFileName, read);
            }

            //add xrm connection and package settings to solution items
            VisualStudioService.AddSolutionItem("xrmpackage.xrmsettings", XrmPackageSettings);
            VsixUtility.AddXrmConnectionToSolution(XrmRecordConfiguration, VisualStudioService);
            VisualStudioService.CloseAllDocuments();
        }
    }
}
