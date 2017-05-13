using JosephM.Record.Xrm.XrmRecord;
using JosephM.XRM.VSIX.Dialogs;
using JosephM.XRM.VSIX.Utilities;

namespace JosephM.XRM.VSIX.Commands.RefreshConnection
{
    internal sealed class RefreshConnectionCommand : CommandBase<RefreshConnectionCommand>
    {
        public override int CommandId
        {
            get { return 0x0101; }
        }

        public override void DoDialog()
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
