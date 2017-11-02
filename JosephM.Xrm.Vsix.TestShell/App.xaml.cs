using JosephM.Application.Options;
using JosephM.Prism.XrmModule.Crud;
using JosephM.Record.Application.Fakes;
using JosephM.Xrm.Vsix.Module.PluginTriggers;
using JosephM.Xrm.Vsix.Module.Web;
using JosephM.Xrm.Vsix.Test;
using JosephM.Xrm.Vsix.Utilities;
using JosephM.XRM.VSIX;
using JosephM.XRM.VSIX.Dialogs;
using Microsoft.Practices.Unity;
using System;
using System.Windows;
using System.Windows.Navigation;

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

            //okay lets try load a window with the module buttons and fake things to spawn the vsix dialogs
            //todo decouple this from the vsix so it doesn't have to bul

            var container = new PrismDependencyContainer(new UnityContainer());


            var visualStudioService = new FakeVisualStudioService();
            container.RegisterInstance(typeof(IVisualStudioService), visualStudioService);

            var applicationController = new VsixApplicationController(container);
            VsixApplication = new VsixApplication(applicationController, new VsixSettingsManager(visualStudioService), new Guid("43816e6d-4db8-48d6-8bfa-75916cb080f0"));

            VsixApplication.AddModule<OpenWebModule>();
            VsixApplication.AddModule<ManagePluginTriggersModule>();
            VsixApplication.AddModule<XrmCrudModule>();
        }

        public VsixApplication VsixApplication { get; set; }
    }
}
