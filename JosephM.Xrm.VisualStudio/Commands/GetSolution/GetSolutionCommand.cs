//------------------------------------------------------------------------------
// <copyright file="Command1.cs" company="Company">
//     Copyright (c) Company.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using JosephM.XRM.VSIX.Dialogs;
using JosephM.XRM.VSIX.Utilities;

namespace JosephM.XRM.VSIX.Commands.GetSolution
{
    internal sealed class GetSolutionCommand : CommandBase
    {
        public override int CommandId
        {
            get { return 0x0107; }
        }

        public override string CommandSetId
        {
            get { return "dd8ecc36-be41-4089-831f-e9ee4dafecae"; }
        }

        private GetSolutionCommand(XrmPackage package)
            : base(package)
        {
        }

        public static GetSolutionCommand Instance { get; private set; }


        public static void Initialize(XrmPackage package)
        {
            Instance = new GetSolutionCommand(package);
        }

        public override void MenuItemCallback(object sender, EventArgs e)
        {
            var settings = VsixUtility.GetPackageSettings(GetDte2());
            if (settings == null)
                settings = new XrmPackageSettings();
            var dialog = new GetSolutionDialog(DialogUtility.CreateDialogController(), GetXrmRecordService(), settings, GetVisualStudioService());
            DialogUtility.LoadDialog(dialog);
        }
    }
}
