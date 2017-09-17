using JosephM.Application.ViewModel.Dialog;
using JosephM.XRM.VSIX.Dialogs;
using JosephM.XRM.VSIX.Utilities;
using System.Collections.Generic;

namespace JosephM.XRM.VSIX.Commands.DeployWebResource
{
    internal sealed class DeployWebResourceCommand : SolutionItemCommandBase<DeployWebResourceCommand>
    {
        public override IEnumerable<string> ValidExtentions { get { return DeployWebResourcesService.WebResourceTypes.Keys; } }

        public override int CommandId
        {
            get { return 0x0102; }
        }

        public override void DoDialog()
        {
            var service = GetXrmRecordService();

            var files = GetSelectedFileNamesQualified();

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
                CreateDialogController());

            DialogUtility.LoadDialog(dialog);
        }
    }
}
