#region

using System;
using System.Windows;
using JosephM.Core.Test;
using JosephM.ObjectEncryption;
using JosephM.Prism.Infrastructure.Prism;
using JosephM.Prism.TestModule.ObjectEncrypt;
using JosephM.Prism.TestModule.Prism;
using JosephM.Prism.TestModule.Prism.TestDialog;
using JosephM.Prism.TestModule.SearchModule;
using JosephM.Prism.XrmModule.SavedXrmConnections;
using JosephM.Prism.XrmModule.Xrm;
using JosephM.Prism.XrmTestModule.DebugModule;
using JosephM.Prism.XrmTestModule.TestXrmSettingsDialog;
using JosephM.Xrm.RecordExtract.Test.TextSearch;

#endregion

namespace JosephM.Prism.TestPrismApplication
{
    /// <summary>
    ///     Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var prism = new PrismApplication("Test Prism Application");
            prism.AddModule<XrmModuleModule>();
            prism.AddModule<XrmTestModule.Prism.XrmTestModule>();
            prism.AddModule<TestModule.Prism.TestModule>();
            prism.AddModule<TestTextSearchModule>();
            prism.AddModule<TestDialogModule>();
            prism.AddModule<SearchModule>();
            prism.AddModule<SavedXrmConnectionsModule>();
            prism.AddModule<DebugDialogModule>();
            prism.AddModule<XrmSettingsModule>();
            prism.AddModule<ObjectEncryptModule<TestObjectEncryptDialog, TestClassToEncrypt>>();
            prism.Run();
        }
    }
}