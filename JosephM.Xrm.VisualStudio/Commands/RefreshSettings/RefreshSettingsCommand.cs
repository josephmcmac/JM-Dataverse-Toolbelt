//------------------------------------------------------------------------------
// <copyright file="Command1.cs" company="Company">
//     Copyright (c) Company.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.IO;
using JosephM.XRM.VSIX.Commands.PackageSettings;
using JosephM.XRM.VSIX.Dialogs;
using JosephM.XRM.VSIX.Utilities;

namespace JosephM.XRM.VSIX.Commands.RefreshSettings
{
    internal sealed class RefreshSettingsCommand : CommandBase
    {
        public override int CommandId
        {
            get { return 0x0106; }
        }

        public override string CommandSetId
        {
            get { return "dd8ecc36-be41-4089-831f-e9ee4dafecae"; }
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
            var solution = GetSolution();
            if (solution != null)
            {
                var fileInfo = new FileInfo(solution.FullName);
                var path = fileInfo.DirectoryName;

                var settings = VsixUtility.GetPackageSettings(GetDte2());
                if(settings == null)
                    settings = new XrmPackageSettings();
                var xrmService = GetXrmRecordService();
                var dialog = new XrmPackageSettingDialog(DialogUtility.CreateDialogController(), settings, GetVisualStudioService(), true, xrmService);

                DialogUtility.LoadDialog(dialog);
            }
        }
    }
}
