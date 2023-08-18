using JosephM.Application.Desktop.Application;
using JosephM.Application.Desktop.Module.AboutModule;
using JosephM.Application.Desktop.Module.SavedRequests;
using JosephM.Application.Desktop.Module.Themes;
using JosephM.Application.Modules;
using JosephM.Core.FieldType;
using JosephM.Spreadsheet;
using JosephM.TestModule.AllPropertyTypesCentered;
using JosephM.TestModule.AllPropertyTypesCompact;
using JosephM.TestModule.AllPropertyTypesModule;
using JosephM.TestModule.TestCrud;
using JosephM.TestModule.TestDialog;
using JosephM.TestModule.TestGridEdit;
using JosephM.TestModule.TestGridOnly;
using JosephM.TestModule.TestSettings;
using JosephM.XrmModule.SavedXrmConnections;
using JosephM.XrmTestModule.DebugModule;
using System.Windows;

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
            app.AddModule<TestDialogModule>();
            app.AddModule<SavedXrmConnectionsModule>();
            app.AddModule<DebugDialogModule>();
            app.AddModule<TestGridOnlyModule>();
            app.AddModule<TestSettingsModule>();
            app.AddModule<TestCrudModule>();
            app.AddModule<SavedRequestModule>();
            app.AddModule<TestGridEditModule>();
            app.AddModule<ColourThemeModule>();
            app.AddModule<AllPropertyTypesDialogModule>();
            app.AddModule<AllPropertyTypesCompactModule>();
            app.AddModule<AllPropertyTypesCenteredModule>();
            app.AddModule<TestAppAboutModule>();
            app.AddModule<TestReadExcelModule>(); 
            app.Run();
        }

        public class TestReadExcelModule : OptionActionModule
        {
            public override string MainOperationName => "Read Excel";

            public override string MenuGroup => "Read Excel";

            public override void DialogCommand()
            {
                var blah = SpreadsheetReader.ReadToDataTable(@"C:\Users\joseph.mcgregor-macd\Desktop\temp\Customisations Import Template V2.xlsx", "Fields");
            }
        }

        public class TestAppAboutModule : AboutModule
        {
            public override string CodeLink => "https://github.com/josephmcmac/XRM-Developer-Tool";

            public override string CreateIssueLink => "https://github.com/josephmcmac/XRM-Developer-Tool/issues/new";

            public override Url OtherLink => new Url("https://visualstudiogallery.msdn.microsoft.com/28fb85a8-70d8-4621-96c7-54151eac11cf", "Visual Studio Extention");

            public override string AboutDetail =>
                "Blah blah blah\n" +
                "\n" +
                "Blah blah blah Blah blah blah Blah blah blah Blah blah blah Blah blah blah Blah blah blah Blah blah blah Blah blah blah Blah blah blah Blah blah blah Blah blah blah Blah blah blah\n" +
                "\n" +
                "Blah blah blah Blah blah blah Blah blah blah Blah blah blah Blah blah blah Blah blah blah Blah blah blah Blah blah blah";
        }
    }
}