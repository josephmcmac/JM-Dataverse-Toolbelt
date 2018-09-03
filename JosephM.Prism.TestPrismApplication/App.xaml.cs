using JosephM.Application.Desktop.Application;
using JosephM.Application.Desktop.Console;
using JosephM.Application.Desktop.Module.SavedRequests;
using JosephM.ObjectEncryption;
using JosephM.TestModule.ObjectEncrypt;
using JosephM.TestModule.TestCrud;
using JosephM.TestModule.TestDialog;
using JosephM.TestModule.TestGridEdit;
using JosephM.TestModule.TestSettings;
using JosephM.XrmModule.SavedXrmConnections;
using JosephM.XrmTestModule.DebugModule;
using JosephM.XrmTestModule.TestSettings;
using JosephM.Xrm.RecordExtract.Test.TextSearch;
using System.Windows;
using JosephM.Application.Desktop.Module.Themes;

namespace JosephM.TestDesktopApplication
{
    /// <summary>
    ///     Interaction logic for App.xaml
    /// </summary>
    public partial class App : System.Windows.Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var app = DesktopApplication.Create("Test Desktop Application");
            //app.AddModule<SavedXrmConnectionsModule>();
            app.AddModule<TestTextSearchModule>();
            app.AddModule<TestDialogModule>();
            app.AddModule<SavedXrmConnectionsModule>();
            app.AddModule<DebugDialogModule>();
            //app.AddModule<XrmSettingsModule>();
            app.AddModule<ObjectEncryptModule<TestObjectEncryptDialog, TestClassToEncrypt>>();
            app.AddModule<TestSettingsModule>();
            app.AddModule<TestCrudModule>();
            app.AddModule<SavedRequestModule>();
            app.AddModule<ConsoleApplicationModule>();
            app.AddModule<TestGridEditModule>();
            app.AddModule<ThemeModule>();
            app.Run();
        }
    }
}