using JosephM.InstanceComparer;
using JosephM.XRM.VSIX.Dialogs;

namespace JosephM.XRM.VSIX.Commands.InstanceComparison
{
    internal sealed class InstanceComparisonCommand : CommandBase<InstanceComparisonCommand>
    {
        public override int CommandId
        {
            get { return 0x0111; }
        }

        public override void DoDialog()
        {
            var xrmService = GetXrmRecordService();
            var settings = GetPackageSettings();

            var request = new InstanceComparerRequest();
            var instanceComparerService = new InstanceComparerService();
            var dialog = new VsixServiceDialog<InstanceComparerService, InstanceComparerRequest, InstanceComparerResponse, InstanceComparerResponseItem>(
                instanceComparerService,
                request,
                CreateDialogController()
                , showRequestEntryForm: true);

            DialogUtility.LoadDialog(dialog);
        }
    }
}
