using JosephM.XRM.VSIX.Dialogs;
using JosephM.XRM.VSIX.Utilities;

namespace JosephM.XRM.VSIX.Commands.DeployAssembly
{
    internal sealed class DeployAssemblyCommand : CommandBase<DeployAssemblyCommand>
    {
        public override int CommandId
        {
            get { return 0x0103; }
        }

        public override void DoDialog()
        {
            var assemblyFile = VsixUtility.BuildSelectedProjectAndGetAssemblyName(ServiceProvider);
            if (!string.IsNullOrWhiteSpace(assemblyFile))
            {
                var settings = VsixUtility.GetPackageSettings(GetDte2());
                if (settings == null)
                    settings = new XrmPackageSettings();
                var dialog = new DeployAssemblyDialog(CreateDialogController(settings), assemblyFile,
                    GetXrmRecordService(), settings);
                DialogUtility.LoadDialog(dialog);
            }
        }
    }
}
