using JosephM.Prism.TestModule.Prism.TestDialog;
using JosephM.Prism.XrmModule.Crud;
using JosephM.Record.Application.Fakes;
using JosephM.Xrm.Vsix.Application;
using JosephM.Xrm.Vsix.Module.DeployAssembly;
using JosephM.Xrm.Vsix.Module.PluginTriggers;
using JosephM.Xrm.Vsix.Module.Web;
using JosephM.Xrm.Vsix.Test;
using Microsoft.Practices.Unity;
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

            var container = new PrismDependencyContainer(new UnityContainer());

            var settingsFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                    "JosephM Xrm", "Vsix Test Shell");
            var visualStudioService = new FakeVisualStudioService();
            container.RegisterInstance(typeof(IVisualStudioService), visualStudioService);

            var applicationController = new VsixApplicationController(container);
            VsixApplication = new VsixApplication(applicationController, new VsixSettingsManager(visualStudioService), new Guid("43816e6d-4db8-48d6-8bfa-75916cb080f0"));

            VsixApplication.AddModule<OpenWebModule>();
            VsixApplication.AddModule<DeployAssemblyModule>();
            VsixApplication.AddModule<ManagePluginTriggersModule>();
            VsixApplication.AddModule<XrmCrudModule>();
            VsixApplication.AddModule<TestDialogModule>();
        }

        public VsixApplication VsixApplication { get; set; }
    }
}
