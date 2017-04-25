//------------------------------------------------------------------------------
// <copyright file="Command1.cs" company="Company">
//     Copyright (c) Company.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using JosephM.Record.Xrm.XrmRecord;
using JosephM.XRM.VSIX.Dialogs;
using JosephM.XRM.VSIX.Utilities;

namespace JosephM.XRM.VSIX.Commands.RefreshConnection
{
    internal sealed class RefreshConnectionCommand : CommandBase
    {
        public override int CommandId
        {
            get { return 0x0101; }
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
                var xrmConfig = VsixUtility.GetXrmConfig(ServiceProvider, true);
                if(xrmConfig == null)
                    xrmConfig = new XrmRecordConfiguration();
                var dialog = new ConnectionEntryDialog(DialogUtility.CreateDialogController(), xrmConfig, GetVisualStudioService(), true);

                DialogUtility.LoadDialog(dialog);
            }
        }
    }
}
