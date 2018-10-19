using JosephM.Application.Desktop.Module.Themes;
using JosephM.Deployment;
using JosephM.InstanceComparer;
using JosephM.TestModule.TestDialog;
using JosephM.Xrm.Vsix.Application;
using JosephM.Xrm.Vsix.Module.DeployAssembly;
using JosephM.Xrm.Vsix.Module.PackageSettings;
using JosephM.Xrm.Vsix.Module.PluginTriggers;
using JosephM.Xrm.Vsix.Module.Web;
using JosephM.Xrm.Vsix.Test;
using JosephM.XrmModule.Crud;
using JosephM.XrmModule.SavedXrmConnections;
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

            VsixApplication = VsixApplication.Create(visualStudioService, container, "Vsix Test Shell", new Guid("43816e6d-4db8-48d6-8bfa-75916cb080f0"));
            VsixApplication.AddModule<SavedXrmConnectionsModule>();
            VsixApplication.AddModule<OpenWebModule>();
            VsixApplication.AddModule<DeployAssemblyModule>();
            VsixApplication.AddModule<ManagePluginTriggersModule>();
            VsixApplication.AddModule<XrmCrudModule>();
            VsixApplication.AddModule<TestDialogModule>();
            VsixApplication.AddModule<PackageSettingsAppConnectionModule>();
            VsixApplication.AddModule<ThemeModule>();
            VsixApplication.AddModule<InstanceComparerModule>();
            VsixApplication.AddModule<DeploymentModule>();
        }

        public VsixApplication VsixApplication { get; set; }
    }
}
