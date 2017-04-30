//------------------------------------------------------------------------------
// <copyright file="Command1.cs" company="Company">
//     Copyright (c) Company.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using JosephM.XRM.VSIX.Commands.PackageSettings;
using JosephM.XRM.VSIX.Dialogs;
using JosephM.XRM.VSIX.Utilities;
using System;
using System.IO;
using System.Windows;

namespace JosephM.XRM.VSIX.Commands.RefreshSettings
{
    internal sealed class RefreshSettingsCommand : CommandBase
    {
        public override int CommandId
        {
            get { return 0x0106; }
        }

        private RefreshSettingsCommand(XrmPackage package)
            : base(package)
        {
        }

        public static RefreshSettingsCommand Instance { get; private set; }

        public static void Initialize(XrmPackage package)
        {
            Instance = new RefreshSettingsCommand(package);
        }

        public override void MenuItemCallback(object sender, EventArgs e)
        {
            MessageBox.Show("Okay");

            var solution = GetSolution();
            if (solution != null)
            {
                var fileInfo = new FileInfo(solution.FullName);
                var path = fileInfo.DirectoryName;
                var settings = GetPackageSettings();
                if (settings == null)
                    settings = new XrmPackageSettings();
                var xrmService = GetXrmRecordService();
                var dialog = new XrmPackageSettingDialog(DialogUtility.CreateDialogController(), settings, GetVisualStudioService(), true, xrmService);

                DialogUtility.LoadDialog(dialog);
            }
        }
    }
}
