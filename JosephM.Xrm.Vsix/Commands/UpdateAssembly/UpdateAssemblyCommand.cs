//------------------------------------------------------------------------------
// <copyright file="Command1.cs" company="Company">
//     Copyright (c) Company.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using JosephM.XRM.VSIX.Dialogs;
using JosephM.XRM.VSIX.Utilities;

namespace JosephM.XRM.VSIX.Commands.UpdateAssembly
{
    internal sealed class UpdateAssemblyCommand : CommandBase
    {
        public override int CommandId
        {
            get { return 0x0105; }
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
