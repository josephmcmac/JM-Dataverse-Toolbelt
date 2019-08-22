using JosephM.Application.Modules;
using JosephM.Core.AppConfig;
using JosephM.Xrm.Vsix.Application;
using JosephM.Xrm.Vsix.Test;
using System;
using System.Windows.Forms;

namespace JosephM.Xrm.Vsix.TestShell
{
    public class SetSelectedSolutionFolderModule : OptionActionModule
    {
        public override string MainOperationName => "Set Selected Solution Folder";

        public override string MenuGroup => "Fake VS";

        public override void DialogCommand()
        {
            var visualStudioService = ApplicationController.ResolveType<IVisualStudioService>() as FakeVisualStudioService;
            if (visualStudioService == null)
                throw new NullReferenceException("visualStudioService");

            var selectFolderDialog = new FolderBrowserDialog { ShowNewFolderButton = false, SelectedPath = visualStudioService.SolutionDirectory };
            var result = selectFolderDialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                visualStudioService.SetSelectedItems(new[] { new FakeVisualStudioSolutionFolder(selectFolderDialog.SelectedPath) });
            }
        }
    }
}
