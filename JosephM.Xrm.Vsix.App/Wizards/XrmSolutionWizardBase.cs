using EnvDTE;
using JosephM.Application;
using JosephM.Application.ViewModel.Dialog;
using JosephM.Core.Extentions;
using JosephM.Prism.XrmModule.Crud;
using JosephM.Record.Application.Fakes;
using JosephM.Record.Xrm.XrmRecord;
using JosephM.Xrm.Vsix.Application;
using JosephM.Xrm.Vsix.Module.PackageSettings;
using Microsoft.Practices.Unity;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Windows;
using VSLangProj;

namespace JosephM.Xrm.Vsix.Wizards
{
    public class XrmSolutionWizardBase : MyWizardBase
    {
        public XrmPackageSettings XrmPackageSettings { get; set; }
        public string DestinationDirectory { get; private set; }
        public string SafeProjectName { get; private set; }

        public override void RunStartedExtention(Dictionary<string, string> replacementsDictionary)
        {
            //get a xrm connection and package setting by loading the entry dialogs
            XrmPackageSettings = new XrmPackageSettings();
            #if DEBUG
                XrmPackageSettings.SolutionDynamicsCrmPrefix = "template";
                XrmPackageSettings.SolutionObjectPrefix = "Template";
            #endif

            var container = new PrismDependencyContainer(new UnityContainer());
            var applicationController = new VsixApplicationController(container);
            if (replacementsDictionary.ContainsKey("$specifiedsolutionname$") && (replacementsDictionary["$specifiedsolutionname$"] == null || replacementsDictionary["$specifiedsolutionname$"] == ""))
            {
                applicationController.UserMessage("Warning! The XRM Solution Generation Will Not Work Correctly If Create Directory For Solution Was Not Specified In The New Solution Dialog");
            }


            RunWizardSettingsEntry(XrmPackageSettings, applicationController);

            //add token replacements for the template projects
            AddReplacements(replacementsDictionary, XrmPackageSettings);

            //used later
            DestinationDirectory = replacementsDictionary["$destinationdirectory$"];
            SafeProjectName = replacementsDictionary["$safeprojectname$"];
        }

        public static void RunWizardSettingsEntry(XrmPackageSettings packageSettings, VsixApplicationController applicationController)
        {
            //ensure the package settings resolves when the app settings dialog runs
            var resolvePackageSettings = applicationController.ResolveType(typeof(XrmPackageSettings));
            if (resolvePackageSettings == null)
                applicationController.RegisterInstance(typeof(XrmPackageSettings), new XrmPackageSettings());

            var settingsDialog = new XrmPackageSettingsDialog(new DialogController(applicationController), packageSettings, null, new XrmRecordService(new XrmRecordConfiguration(), formService: new XrmFormService()));
            settingsDialog.SaveSettings = false;
            var uriQuery = new UriQuery();
            uriQuery.Add("Modal", true.ToString());
            applicationController.RequestNavigate("Main", settingsDialog, uriQuery, showCompletionScreen: false, isModal: true);
        }

        public override void RunFinishedExtention()
        {
            if (DestinationDirectory.EndsWith(SafeProjectName + Path.DirectorySeparatorChar + SafeProjectName))
            {
                //The projects were created under a seperate folder -- lets fix it

                //todo this appears to do something incorrect if create directory for solution was not selected

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

            //okay so lets update the encrypt connection bat and the xrmsetting.txt files in the console project
            var consoleProjectPath = DestinationDirectory + Path.DirectorySeparatorChar + SafeProjectName + ".Console";
            var encryptBatFileName = consoleProjectPath + Path.DirectorySeparatorChar + "Encrypt XRM Connection.bat";
            if(File.Exists(encryptBatFileName))
            {
                var read = File.ReadAllText(encryptBatFileName);
                read = read.Replace("$ext_safeprojectname$", SafeProjectName);
                File.WriteAllText(encryptBatFileName, read);
            }
            var consoleConnectionFileName = consoleProjectPath + Path.DirectorySeparatorChar + "XrmSetting.txt";
            if (File.Exists(consoleConnectionFileName))
            {
                if(XrmPackageSettings != null && XrmPackageSettings.Connections != null && XrmPackageSettings.Connections.Any())
                {
                    try
                    {
                        var connection = XrmPackageSettings.Connections.First();
                        var serialise = ObjectToJsonString(connection);
                        File.WriteAllText(consoleConnectionFileName, serialise);
                    }
                    catch(Exception ex)
                    {
                        MessageBox.Show("Error setting console application connection: " + ex.DisplayString());
                    }
                }
            }

            var visualStudioService = new VisualStudioService(DTE, useSolutionDirectory: DestinationDirectory);
            //add xrm connection and package settings to solution items
            visualStudioService.AddSolutionItem("xrmpackage.xrmsettings", XrmPackageSettings);
            if (XrmPackageSettings.Connections.Any())
                visualStudioService.AddSolutionItem("solution.xrmconnection", XrmPackageSettings.Connections.First());
            visualStudioService.CloseAllDocuments();

            RemoveEmptyFolders(DestinationDirectory);
        }

        public void RemoveEmptyFolders(string directory)
        {
            if (Directory.Exists(directory))
            {
                foreach(var subDirectory in Directory.GetDirectories(directory))
                {
                    RemoveEmptyFolders(subDirectory);
                }
                if (!Directory.GetFiles(directory).Any() && !Directory.GetDirectories(directory).Any())
                {
                    Directory.Delete(directory);
                }
            }

        }

        public static string ObjectToJsonString<T>(T objectValue)
        {
            var serializer = new DataContractJsonSerializer(typeof(T));
            using (var stream = new MemoryStream())
            {
                serializer.WriteObject(stream, objectValue);
                return Encoding.Default.GetString(stream.ToArray());
            }
        }
    }
}
