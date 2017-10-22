using JosephM.InstanceComparer;
using JosephM.Prism.XrmModule.Crud;
using JosephM.XRM.VSIX.Dialogs;

namespace JosephM.XRM.VSIX.Commands.CrudCommand
{
    internal sealed class CrudCommand : CommandBase<CrudCommand>
    {
        public override int CommandId
        {
            get { return 0x0112; }
        }

        public override void DoDialog()
        {
            var xrmService = GetXrmRecordService();

            var request = new InstanceComparerRequest();

            var dialog = new XrmCrudDialog(
                xrmService, 
                CreateDialogController());

            DialogUtility.LoadDialog(dialog);
        }
    }
}
