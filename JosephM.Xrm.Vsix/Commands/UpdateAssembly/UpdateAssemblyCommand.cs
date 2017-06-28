using JosephM.XRM.VSIX.Dialogs;
using JosephM.XRM.VSIX.Utilities;

namespace JosephM.XRM.VSIX.Commands.UpdateAssembly
{
    internal sealed class UpdateAssemblyCommand : CommandBase<UpdateAssemblyCommand>
    {
        public override int CommandId
        {
            get { return 0x0105; }
        }

        public override void DoDialog()
        {
            var assemblyFile = VsixUtility.BuildSelectedProjectAndGetAssemblyName(ServiceProvider);
            if (!string.IsNullOrWhiteSpace(assemblyFile))
            {
                var dialog = new UpdateAssemblyDialog(DialogUtility.CreateDialogController(), assemblyFile,
                    GetXrmRecordService(), GetPackageSettings());
                DialogUtility.LoadDialog(dialog);
            }
        }
    }
}
