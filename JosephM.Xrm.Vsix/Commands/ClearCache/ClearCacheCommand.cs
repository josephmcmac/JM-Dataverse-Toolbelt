//------------------------------------------------------------------------------
// <copyright file="Command1.cs" company="Company">
//     Copyright (c) Company.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using JosephM.XRM.VSIX.Dialogs;
using JosephM.XRM.VSIX.Utilities;
using JosephM.Application.ViewModel.Dialog;
using JosephM.Xrm.ImportExporter.Service;
using JosephM.ObjectMapping;
using JosephM.Prism.XrmModule.SavedXrmConnections;
using JosephM.Core.FieldType;
using System.Windows;
using JosephM.Core.Extentions;

namespace JosephM.XRM.VSIX.Commands.ClearCache
{
    internal sealed class ClearCacheCommand : CommandBase
    {
        public override int CommandId
        {
            get { return 0x0109; }
        }

        private ClearCacheCommand(XrmPackage package)
            : base(package)
        {
        }

        public static ClearCacheCommand Instance { get; private set; }


        public static void Initialize(XrmPackage package)
        {
            Instance = new ClearCacheCommand(package);
        }

        public override void MenuItemCallback(object sender, EventArgs e)
        {
            try
            {
                DoDialog();
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.DisplayString());
            }
        }

        private void DoDialog()
        {
            var xrmRecordService = GetXrmRecordService();
            xrmRecordService.ClearCache();
            MessageBox.Show("Cache Cleared");
        }
    }
}
