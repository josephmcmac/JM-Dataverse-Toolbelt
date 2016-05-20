//------------------------------------------------------------------------------
// <copyright file="Command1.cs" company="Company">
//     Copyright (c) Company.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.IO;
using EnvDTE;
using JosephM.XRM.VSIX.Dialogs;
using JosephM.XRM.VSIX.Utilities;
using Microsoft.VisualStudio.Shell;

namespace JosephM.XRM.VSIX.Commands.DeployAssembly
{
    internal sealed class DeployAllCommand : CommandBase
    {
        public override int CommandId
        {
            get { return 0x0103; }
        }

        public override string CommandSetId
        {
            get { return "dd8ecc36-be41-4089-831f-e9ee4dafecae"; }
        }

        private DeployAllCommand(XrmPackage package)
            : base(package)
        {
        }

        public static DeployAllCommand Instance { get; private set; }


        public static void Initialize(XrmPackage package)
        {
            Instance = new DeployAllCommand(package);
        }

        public override void MenuItemCallback(object sender, EventArgs e)
        {
            var dte = GetDte2();
            var build = dte.Solution.SolutionBuild;
            build.Clean(true);
            build.Build(true);
            var info = build.LastBuildInfo;

            if (info == 0)
            {
                var selectedItems = GetSelectedItems();
                foreach (SelectedItem item in selectedItems)
                {
                    var project = item.Project;
                    if (project.Name != null)
                    {
                        var assemblyName = VsixUtility.GetProperty(project.Properties, "AssemblyName");
                        var outputPath =
                            VsixUtility.GetProperty(project.ConfigurationManager.ActiveConfiguration.Properties,
                                "OutputPath");
                        var fileInfo = new FileInfo(project.FullName);
                        var rootFolder = fileInfo.DirectoryName;
                        var outputFolder = Path.Combine(rootFolder ?? "", outputPath);
                        var assemblyFile = Path.Combine(outputFolder, assemblyName) + ".dll";

                        var dialog = new DeployAssemblyDialog(DialogUtility.CreateDialogController(), assemblyFile,
                            GetXrmRecordService());
                        DialogUtility.LoadDialog(dialog);
                    }
                }
            }
        }
    }
}
