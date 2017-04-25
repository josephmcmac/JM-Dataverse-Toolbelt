//------------------------------------------------------------------------------
// <copyright file="Command1.cs" company="Company">
//     Copyright (c) Company.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using EnvDTE;
using JosephM.Application.ViewModel.Dialog;
using JosephM.XRM.VSIX.Dialogs;
using JosephM.XRM.VSIX.Utilities;

namespace JosephM.XRM.VSIX.Commands.DeployWebResource
{
    internal sealed class DeployWebResourceCommand : SolutionItemCommandBase
    {
        public override IEnumerable<string> ValidExtentions { get { return DeployWebResourcesService.WebResourceTypes.Keys; } }

        public override int CommandId
        {
            get { return 0x0102; }
        }

        public override string CommandSetId
        {
            get { return "dd8ecc36-be41-4089-831f-e9ee4dafecae"; }
        }

        private DeployWebResourceCommand(XrmPackage package)
            : base(package)
        {
        }

        public static DeployWebResourceCommand Instance { get; private set; }


        public static void Initialize(XrmPackage package)
        {
            Instance = new DeployWebResourceCommand(package);
        }

        public override void MenuItemCallback(object sender, EventArgs e)
        {
            var service = GetXrmRecordService();

            var files = new List<string>();
            var selectedItems = GetSelectedItems();
            foreach (SelectedItem item in selectedItems)
            {
                if (item.ProjectItem != null && !string.IsNullOrWhiteSpace(item.Name))
                {
                    files.Add(item.ProjectItem.FileNames[0]);
                }
            }
            var settings = VsixUtility.GetPackageSettings(GetDte2());
            if (settings == null)
                settings = new XrmPackageSettings();
            var deployResourcesService = new DeployWebResourcesService(service, settings);

            var request = new DeployWebResourcesRequest()
            {
                Files = files
            };
            var dialog = new VsixServiceDialog<DeployWebResourcesService, DeployWebResourcesRequest, DeployWebResourcesResponse, DeployWebResourcesResponseItem>(
                deployResourcesService,
                request,
                new DialogController(new VsixApplicationController("VSIX", null)));

            DialogUtility.LoadDialog(dialog);
        }
    }
}
