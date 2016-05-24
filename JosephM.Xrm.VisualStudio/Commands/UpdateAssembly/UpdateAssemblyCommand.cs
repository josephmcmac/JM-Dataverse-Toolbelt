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
    internal sealed class UpdateAssemblyCommand : CommandBase
    {
        public override int CommandId
        {
            get { return 0x0105; }
        }

        public override string CommandSetId
        {
            get { return "dd8ecc36-be41-4089-831f-e9ee4dafecae"; }
        }

        private UpdateAssemblyCommand(XrmPackage package)
            : base(package)
        {
        }

        public static UpdateAssemblyCommand Instance { get; private set; }


        public static void Initialize(XrmPackage package)
        {
            Instance = new UpdateAssemblyCommand(package);
        }

        public override void MenuItemCallback(object sender, EventArgs e)
        {
            var assemblyFile = VsixUtility.BuildSelectedProjectAndGetAssemblyName(ServiceProvider);
            if (!string.IsNullOrWhiteSpace(assemblyFile))
            {
                var dialog = new UpdateAssemblyDialog(DialogUtility.CreateDialogController(), assemblyFile,
                    GetXrmRecordService());
                DialogUtility.LoadDialog(dialog);
            }
        }
    }
}
