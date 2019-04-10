using JosephM.Application.Modules;
using JosephM.Xrm.Vsix.Application;
using JosephM.Core.AppConfig;
using JosephM.Xrm.Vsix.Test;
using System;
using System.Windows.Forms;
using System.Linq;

namespace JosephM.Xrm.Vsix.TestShell
{
    public class SetSelectedModule : OptionActionModule
    {
        public override string MainOperationName => "Set Selected Items";

        public override string MenuGroup => "Fake VS";

        public override void DialogCommand()
        {
            var visualStudioService = ApplicationController.ResolveType<IVisualStudioService>() as FakeVisualStudioService;
            if (visualStudioService == null)
                throw new NullReferenceException("visualStudioService");

            var fileDialog = new OpenFileDialog() { Multiselect = true,  InitialDirectory = visualStudioService.SolutionDirectory };
            var result = fileDialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                visualStudioService.SetSelectedItems(fileDialog.FileNames.Select(f => new FakeVisualStudioProjectItem(f)).ToArray());
            }
        }
    }
}
