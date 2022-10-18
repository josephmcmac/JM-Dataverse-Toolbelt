using JosephM.ToolbeltTheme;
using JosephM.Xrm.Vsix.App;
using JosephM.Xrm.Vsix.Application;
using JosephM.Xrm.Vsix.Test;
using System;
using System.IO;
using System.Windows;

namespace JosephM.Xrm.Vsix.TestShell
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : System.Windows.Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            //load an application with the module buttons and fake things to spawn the vsix dialogs

            var container = new VsixDependencyContainer();

            var applicationName = "Vsix Test Shell";

            var settingsFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                    "JosephM Xrm", applicationName);
            var visualStudioService = new FakeVisualStudioService(solutionDirectory: settingsFolder);
            container.RegisterInstance(typeof(IVisualStudioService), visualStudioService);

            VsixApplication = Factory.CreateJosephMXrmVsixApp(visualStudioService, container, appName: "Vsix Test Shell", isNonSolutionExplorerContext: true);
            //VsixApplication.AddModule<SavedXrmConnectionsModule>();
            //VsixApplication.AddModule<OpenWebModule>();
            //VsixApplication.AddModule<OpenAdvancedFindModule>();
            //VsixApplication.AddModule<OpenDefaultSolutionModule>();
            //VsixApplication.AddModule<OpenSolutionModule>();
            //VsixApplication.AddModule<DeployAssemblyModule>();
            //VsixApplication.AddModule<UpdateAssemblyModule>();
            //VsixApplication.AddModule<ManagePluginTriggersModule>();
            //VsixApplication.AddModule<XrmCrudModule>();
            //VsixApplication.AddModule<TestDialogModule>();
            //VsixApplication.AddModule<PackageSettingsAppConnectionModule>();
            //VsixApplication.AddModule<ThemeModule>();
            //VsixApplication.AddModule<InstanceComparerModule>();
            //VsixApplication.AddModule<DeploymentModule>();
            //VsixApplication.AddModule<AddPortalCodeModule>();
            //VsixApplication.AddModule<DeployWebResourceModule>();

            VsixApplication.AddModule<ToolbeltThemeModule>();
            VsixApplication.AddModule<SetSelectedItemsModule>();
            VsixApplication.AddModule<SetSelectedProjectAssemblyModule>();
            VsixApplication.AddModule<SetSelectedSolutionFolderModule>(); 
        }

        public VsixApplication VsixApplication { get; set; }
    }
}
