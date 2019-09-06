using JosephM.Application.Desktop.Module.Settings;
using JosephM.Application.Modules;
using JosephM.Application.ViewModel.Extentions;
using JosephM.Application.ViewModel.RecordEntry.Form;
using JosephM.Core.Attributes;
using JosephM.Core.Serialisation;
using JosephM.Core.Utility;
using System;
using System.IO;
using System.Net;

namespace JosephM.Application.Desktop.Module.ReleaseCheckModule
{
    [DependantModule(typeof(SettingsAggregatorModule))]
    [MyDescription("Module To Check For Newer Releases")]
    /// <summary>
    /// Module to check for a newer release on start up
    /// </summary>
    public abstract class GitHubReleaseCheckModule : AggregatedSettingModule<UpdateSettings>
    {
        public override void RegisterTypes()
        {
            base.RegisterTypes();
            //add a button to the update settings form to check for new release
            var customFormFunction = new CustomFormFunction("RELEASECHECK", "Check For Release", (x) => CheckNewForRelease(displayNoUpdate: true), (x) => true);
            this.AddCustomFormFunction(customFormFunction, typeof(UpdateSettings));
        }

        public override void InitialiseModule()
        {
            base.InitialiseModule();
            if (ApplicationController.RunThreadsAsynch)
            {
                ApplicationController.DoOnAsyncThread(() =>
                {
                    var updateSettings = (UpdateSettings)ApplicationController.ResolveType(typeof(UpdateSettings));
                    if (updateSettings.CheckForNewReleaseOnStartup)
                    {
                        CheckNewForRelease(displayNoUpdate: false);
                    }
                });
            }
        }

        public string MainOperationName => "Update Settings";

        public void CheckNewForRelease(bool displayNoUpdate = false)
        {
            try
            {
                var latestRelease = GetLatestRelease();
                var latestVersionString = latestRelease.tag_name;
                var thisVersionString = ApplicationController.Version;

                bool isNewer = VersionHelper.IsNewerVersion(latestVersionString, thisVersionString);

                if (isNewer)
                {
                    var confirm = ApplicationController.UserConfirmation("A newer release is available. Do you want to get the latest release?");
                    if (confirm)
                    {
                        var latestReleaseurl = $"https://github.com/{Githubusername}/{GithubRepository}/releases/latest";
                        ApplicationController.StartProcess(latestReleaseurl);
                    }
                }
                else
                {
                    if (displayNoUpdate)
                    {
                        ApplicationController.UserMessage($"No Newer Release Is Available");
                    }
                }
            }
            catch (Exception ex)
            {
                ApplicationController.ThrowException(ex);
            }
        }

        private GithubRelease GetLatestRelease()
        {
            var url = $"https://api.github.com/repos/{Githubusername}/{GithubRepository}/releases/latest";

            HttpWebRequest webRequest = WebRequest.Create(url) as HttpWebRequest;
            webRequest.Method = "GET";
            //not sure why had to add this https://stackoverflow.com/questions/22649419/protocol-violation-using-github-api
            webRequest.UserAgent = ApplicationController.ApplicationName;
            string json = null;
            using (StreamReader responseReader = new StreamReader(webRequest.GetResponse().GetResponseStream()))
            {
                json = responseReader.ReadToEnd();
            }
            return (GithubRelease)JsonHelper.JsonStringToObject(json, typeof(GithubRelease));
        }

        public abstract string Githubusername { get; }

        public abstract string GithubRepository { get; }
    }
}