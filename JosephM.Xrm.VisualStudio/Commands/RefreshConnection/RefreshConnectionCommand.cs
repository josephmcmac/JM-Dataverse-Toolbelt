//------------------------------------------------------------------------------
// <copyright file="Command1.cs" company="Company">
//     Copyright (c) Company.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using JosephM.Prism.XrmModule.SavedXrmConnections;
using JosephM.Record.Xrm.XrmRecord;
using JosephM.XRM.VSIX.Dialogs;
using JosephM.XRM.VSIX.Utilities;
using Microsoft.VisualStudio.Shell;

namespace JosephM.XRM.VSIX.Commands.RefreshConnection
{
    internal sealed class RefreshConnectionCommand : CommandBase
    {
        public override int CommandId
        {
            get { return 0x0101; }
        }

        public override string CommandSetId
        {
            get { return "dd8ecc36-be41-4089-831f-e9ee4dafecae"; }
        }

        private RefreshConnectionCommand(XrmPackage package)
            : base(package)
        {
        }

        public static RefreshConnectionCommand Instance { get; private set; }

        public static void Initialize(XrmPackage package)
        {
            Instance = new RefreshConnectionCommand(package);
        }

        public override void MenuItemCallback(object sender, EventArgs e)
        {
            var solution = GetSolution();
            if (solution != null)
            {
                var fileInfo = new FileInfo(solution.FullName);
                var path = fileInfo.DirectoryName;

                var xrmConfig = VsixUtility.GetXrmConfig(ServiceProvider);
                if(xrmConfig == null)
                    xrmConfig = new XrmRecordConfiguration();
                var dialog = new ConnectionEntryDialog(DialogUtility.CreateDialogController(), xrmConfig, solution, path);

                DialogUtility.LoadDialog(dialog);
            }
        }
    }
}
