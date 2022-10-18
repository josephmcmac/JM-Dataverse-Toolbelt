using JosephM.Application.Desktop.Module.ApplicationInsights;
using JosephM.Application.Desktop.Module.ReleaseCheckModule;
using JosephM.Application.Desktop.Module.Settings;
using JosephM.Application.Desktop.Module.Themes;
using JosephM.Application.ViewModel.RecordEntry.Form;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using JosephM.Core.AppConfig;
using JosephM.Application.Application;

namespace JosephM.Application.Desktop.Test
{
    [TestClass]
    public class AggregatedSettingsModuleTests : ModuleTest
    {
        [TestMethod]
        public void AggregatedSettingsModuleTest()
        {
            //Load several modules with their settings in the aggregator form

            var app = CreateAndLoadTestApplication<TestReleaseCheckModule>();
            app.AddModule<TestApplicationInsightsModule>();
            app.AddModule<ColourThemeModule>();

            //verify navigate to the settings form
            app.GetModule<SettingsAggregatorModule>().DialogCommand();
            var settingsDialog = app.GetNavigatedDialog<SettingsAggregatorDialog>();
            var aggregatorForm = settingsDialog.Controller.UiItems.First() as RecordEntryAggregatorViewModel;
            Assert.AreEqual(3, aggregatorForm.EntryForms.Count());

            //verify all save buttons
            foreach(var settingForm in aggregatorForm.EntryForms)
            {
                settingForm.SaveButtonViewModel.Invoke();
            }

            //okay lets set a boolean field in the update settings
            //and verify it is updated both in the container and the settings after each save

            //set false
            var updateSettingsForm = aggregatorForm.EntryForms.First(f => f.GetRecordType() == typeof(UpdateSettings).AssemblyQualifiedName);
            updateSettingsForm.GetBooleanFieldFieldViewModel(nameof(UpdateSettings.CheckForNewReleaseOnStartup)).Value = false;
            Assert.IsTrue(updateSettingsForm.Validate());
            updateSettingsForm.SaveButtonViewModel.Invoke();

            //verify updated both in memory and on disk
            var updateSettings = app.Controller.ResolveType<UpdateSettings>();
            Assert.IsFalse(updateSettings.CheckForNewReleaseOnStartup);
            var settingsManager = app.Controller.ResolveType<ISettingsManager>();
            updateSettings = settingsManager.Resolve<UpdateSettings>();
            Assert.IsFalse(updateSettings.CheckForNewReleaseOnStartup);

            //set true
            updateSettingsForm.GetBooleanFieldFieldViewModel(nameof(UpdateSettings.CheckForNewReleaseOnStartup)).Value = true;
            Assert.IsTrue(updateSettingsForm.Validate());
            updateSettingsForm.SaveButtonViewModel.Invoke();

            //verify updated both in memory and on disk
            updateSettings = app.Controller.ResolveType<UpdateSettings>();
            Assert.IsTrue(updateSettings.CheckForNewReleaseOnStartup);
            settingsManager = app.Controller.ResolveType<ISettingsManager>();
            updateSettings = settingsManager.Resolve<UpdateSettings>();
            Assert.IsTrue(updateSettings.CheckForNewReleaseOnStartup);
        }

        public class TestApplicationInsightsModule : ApplicationInsightsModule
        {
            public override string InstrumentationKey => Guid.Empty.ToString();
        }

        public class TestReleaseCheckModule : GitHubReleaseCheckModule
        {
            public override string Githubusername => "testingonly";

            public override string GithubRepository => "testingonly";
        }
    }
}