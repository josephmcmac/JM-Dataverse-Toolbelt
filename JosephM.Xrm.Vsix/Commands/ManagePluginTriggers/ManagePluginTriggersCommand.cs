using EnvDTE;
using JosephM.XRM.VSIX.Dialogs;
using JosephM.XRM.VSIX.Utilities;

namespace JosephM.XRM.VSIX.Commands.ManagePluginTriggers
{
    internal sealed class ManagePluginTriggersCommand : CommandBase<ManagePluginTriggersCommand>
    {
        public override int CommandId
        {
            get { return 0x0104; }
        }

        public override void DoDialog()
        {
            var selectedItems = GetSelectedItems();
            foreach (SelectedItem item in selectedItems)
            {
                var project = item.Project;
                if (project.Name != null)
                {
                    var service = GetXrmRecordService();
                    var settings = VsixUtility.GetPackageSettings(GetDte2());
                    if (settings == null)
                        settings = new XrmPackageSettings();
                    var assemblyName = VsixUtility.GetProperty(project.Properties, "AssemblyName");

                    var dialog = new ManagePluginTriggersDialog(CreateDialogController(settings), assemblyName, service, settings);

                    DialogUtility.LoadDialog(dialog);
                } 
            }
        }
    }
}
