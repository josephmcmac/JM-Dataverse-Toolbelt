#region

using JosephM.ObjectEncryption;
using JosephM.Prism.Infrastructure.Console;
using JosephM.Prism.Infrastructure.Module.SavedRequests;
using JosephM.Prism.Infrastructure.Prism;
using JosephM.Prism.TestModule.ObjectEncrypt;
using JosephM.Prism.TestModule.Prism.TestCrud;
using JosephM.Prism.TestModule.Prism.TestDialog;
using JosephM.Prism.TestModule.Prism.TestSettings;
using JosephM.Prism.XrmModule.SavedXrmConnections;
using JosephM.Prism.XrmModule.Xrm;
using JosephM.Prism.XrmTestModule.DebugModule;
using JosephM.Prism.XrmTestModule.TestXrmSettingsDialog;
using JosephM.Xrm.RecordExtract.Test.TextSearch;
using System.Windows;

#endregion

namespace JosephM.Prism.TestPrismApplication
{
    /// <summary>
    ///     Interaction logic for App.xaml
    /// </summary>
    public partial class App : System.Windows.Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var prism = new PrismApplication("Test Prism Application");
            prism.AddModule<XrmModuleModule>();
            prism.AddModule<TestTextSearchModule>();
            prism.AddModule<TestDialogModule>();
            prism.AddModule<SavedXrmConnectionsModule>();
            prism.AddModule<DebugDialogModule>();
            prism.AddModule<XrmSettingsModule>();
            prism.AddModule<ObjectEncryptModule<TestObjectEncryptDialog, TestClassToEncrypt>>();
            prism.AddModule<TestSettingsModule>();
            prism.AddModule<TestCrudModule>();
            prism.AddModule<SavedRequestModule>();
            prism.AddModule<ConsoleApplicationModule>();
            prism.Run();
        }
    }
}