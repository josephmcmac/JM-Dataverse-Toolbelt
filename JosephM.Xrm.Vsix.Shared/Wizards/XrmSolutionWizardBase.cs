﻿using EnvDTE;
using JosephM.Application.Application;
using JosephM.Core.AppConfig;
using JosephM.Core.Extentions;
using JosephM.Xrm.Vsix.App;
using JosephM.Xrm.Vsix.Application;
using JosephM.Xrm.Vsix.Module.PackageSettings;
using JosephM.XrmModule.SavedXrmConnections;
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
        public VsixApplication VsixApplication { get; set; }
        public XrmPackageSettings XrmPackageSettings { get; set; }
        public string DestinationDirectory { get; private set; }
        public string SafeProjectName { get; private set; }

        public override void RunStartedExtention(Dictionary<string, string> replacementsDictionary)
        {
            //get a xrm connection and package setting by loading the entry dialogs
            XrmPackageSettings = new XrmPackageSettings();
            if (replacementsDictionary.ContainsKey("$projectname$"))
            {
                XrmPackageSettings.PluginProjects = new[]
                {
                    new XrmPackageSettings.PluginProject(replacementsDictionary["$projectname$"] + ".Plugins")
                };
                XrmPackageSettings.WebResourceProjects = new[]
                {
                    new XrmPackageSettings.WebResourceProject(replacementsDictionary["$projectname$"] + ".WebResources")
                };
                XrmPackageSettings.DeployIntoFieldProjects = new[]
                {
                    new XrmPackageSettings.DeployIntoFieldProject("N/A")
                };
            }
            var visualStudioService = new VisualStudioService(DTE);
            var container = new DependencyContainer();
            container.RegisterInstance<IVisualStudioService>(visualStudioService);
            var app = Factory.CreateJosephMXrmVsixApp(visualStudioService, container, isNonSolutionExplorerContext: true);
            VsixApplication = app;
            app.VsixApplicationController.LogEvent("Xrm Solution Template Wizard Loaded");
            try
            {
                if (replacementsDictionary.ContainsKey("$specifiedsolutionname$") && (replacementsDictionary["$specifiedsolutionname$"] == null || replacementsDictionary["$specifiedsolutionname$"] == ""))
                {
                    app.Controller.UserMessage("Warning! When Creating The Solution If 'Create Directory For Solution' Is Not Set In Visual Studio 2017, Or If 'Place Solution And Project In The Same Directory' Is Set In Visual Studio 2019, Then The New Solution May Not Be Created Correctly Due To Visual Studio Placing The .sln File In An Incorrect Folder Or Creating A Nested Projects Folder. Recommend Restarting The Solution Creation With This Checked Appropriately");
                }

                var solutionName = replacementsDictionary.ContainsKey("$specifiedsolutionname$")
                    && replacementsDictionary["$specifiedsolutionname$"] != null
                    ? replacementsDictionary["$specifiedsolutionname$"]
                    : null;

                SolutionWizardPackageSettingsDialog.Run(XrmPackageSettings, app.VsixApplicationController, solutionName);

                //add token replacements for the template projects
                AddReplacements(replacementsDictionary, XrmPackageSettings);

                //used later
                DestinationDirectory = replacementsDictionary["$destinationdirectory$"];
                SafeProjectName = replacementsDictionary["$safeprojectname$"];
            }
            catch (Exception ex)
            {
                app.VsixApplicationController.LogEvent("Xrm Solution Template Wizard Fatal Error", new Dictionary<string, string> { { "Is Error", true.ToString() }, { "Error", ex.Message }, { "Error Trace", ex.DisplayString() } });
                throw;
            }
        }

        public override void RunFinishedExtention()
        {
            try
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

                var visualStudioService = new VisualStudioService(DTE, useSolutionDirectory: DestinationDirectory);
                //add xrm connection and package settings to solution items
                var vsixSettingsManager = new VsixSettingsManager(visualStudioService, null);
                vsixSettingsManager.SaveSettingsObject(XrmPackageSettings);
                if (XrmPackageSettings.Connections.Any())
                {
                    var connection = GetActiveConnectionToSave();
                    if (connection != null)
                    {
                        vsixSettingsManager.SaveSettingsObject(connection);
                    }
                }
                visualStudioService.CloseAllDocuments();

                RemoveEmptyFolders(DestinationDirectory);

                VsixApplication.VsixApplicationController.LogEvent("Xrm Solution Template Wizard Completed", new Dictionary<string, string> { { "Is Completed Event", true.ToString() } });
            }
            catch (Exception ex)
            {
                VsixApplication.VsixApplicationController.LogEvent("Xrm Solution Template Wizard Fatal Error", new Dictionary<string, string> { { "Is Error", true.ToString() }, { "Error", ex.Message }, { "Error Trace", ex.DisplayString() } });
                throw;
            }
        }

        private SavedXrmRecordConfiguration GetActiveConnectionToSave()
        {
            SavedXrmRecordConfiguration connection = null;
            if (XrmPackageSettings.Connections.Count() == 1)
                connection = XrmPackageSettings.Connections.First();
            else if (XrmPackageSettings.Connections.Count(c => c.Active) == 1)
                connection = XrmPackageSettings.Connections.First(c => c.Active);
            return connection;
        }

        public void RemoveEmptyFolders(string directory)
        {
            if (Directory.Exists(directory))
            {
                foreach (var subDirectory in Directory.GetDirectories(directory))
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