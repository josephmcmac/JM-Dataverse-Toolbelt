using JosephM.Application.Modules;
using JosephM.Xrm.Vsix.Application;
using JosephM.Core.AppConfig;
using JosephM.Xrm.Vsix.Test;
using System;
using System.Windows.Forms;
using System.Linq;

namespace JosephM.Xrm.Vsix.TestShell
{
    public class SetSelectedProjectAssemblyModule : OptionActionModule
    {
        public override string MainOperationName => "Set Selected Assembly";

        public override string MenuGroup => "Fake VS";

        public override void DialogCommand()
        {
            var visualStudioService = ApplicationController.ResolveType<IVisualStudioService>() as FakeVisualStudioService;
            if (visualStudioService == null)
                throw new NullReferenceException("visualStudioService");

            var fileDialog = new OpenFileDialog() { Multiselect = false, Filter = "Assemblies(*.dll)|*.dll",  InitialDirectory = visualStudioService.SolutionDirectory };
            var result = fileDialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                visualStudioService.SetSelectedProjectAssembly(fileDialog.FileNames[0]);
            }
        }
    }
}
