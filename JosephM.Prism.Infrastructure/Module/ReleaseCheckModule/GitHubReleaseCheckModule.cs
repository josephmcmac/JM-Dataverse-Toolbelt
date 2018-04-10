using JosephM.Application.Prism.Module.Settings;
using JosephM.Application.ViewModel.Extentions;
using JosephM.Application.ViewModel.RecordEntry.Form;
using JosephM.Core.Attributes;
using JosephM.Core.Serialisation;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;

namespace JosephM.Application.Prism.Module.ReleaseCheckModule
{
    [MyDescription("Module To Check For Newer Releases")]
    /// <summary>
    /// Module to check for a newer release on start up
    /// </summary>
    public abstract class GitHubReleaseCheckModule : SettingsModule<UpdateSettingsDialog, UpdateSettings, UpdateSettings>
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

        public override string MainOperationName => "Update Settings";

        public void CheckNewForRelease(bool displayNoUpdate = false)
        {
            try
            {
                var latestRelease = GetLatestRelease();
                var latestVersionString = latestRelease.tag_name;
                var thisVersionString = GetInstalledApplicationVersion();

                bool isNewer = IsNewerVersion(latestVersionString, thisVersionString);

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

        public string GetInstalledApplicationVersion()
        {
            var rKey = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall");

            var insApplication = new List<string>();

            if (rKey != null && rKey.SubKeyCount > 0)
            {
                insApplication = rKey.GetSubKeyNames().ToList();
            }

            int i = 0;

            string result = null;

            foreach (string appName in insApplication)
            {

                RegistryKey finalKey = rKey.OpenSubKey(insApplication[i]);

                string installedApp = finalKey.GetValue("DisplayName")?.ToString();

                if (installedApp == ApplicationController.ApplicationName)
                {
                    var thisOne = finalKey.GetValue("DisplayVersion").ToString();
                    if (result == null || IsNewerVersion(thisOne, result))
                        result = thisOne;
                }
                i++;
            }
            return result;
        }

        public bool IsNewerVersion(string latestVersionString, string thisVersionString)
        {
            var isNewer = false;
            if (thisVersionString != null && latestVersionString != null)
            {
                var latestNumbers = ParseVersionNumbers(latestVersionString);
                var thisNumbers = ParseVersionNumbers(thisVersionString);
                isNewer = IsNewerVersion(latestNumbers, thisNumbers);
            }

            return isNewer;
        }

        private bool IsNewerVersion(IEnumerable<int> latestNumbers, IEnumerable<int> thisNumbers)
        {
            if (!latestNumbers.Any())
            {
                return false;
            }
            else if (!thisNumbers.Any())
            {
                if (latestNumbers.All(i => i == 0))
                    return false;
                else
                    return true;
            }
            else if (thisNumbers.First() > latestNumbers.First())
                return false;
            else if (latestNumbers.First() > thisNumbers.First())
                return true;
            else
                return IsNewerVersion(latestNumbers.Skip(1).ToArray(), thisNumbers.Skip(1).ToArray());
        }

        private static IEnumerable<int> ParseVersionNumbers(string versionString)
        {
            var splitVersion = versionString.Split('.');

            var splitLatestInts = new List<int>();
            if (versionString != null)
            {
                foreach (var item in versionString.Split('.'))
                {
                    int parsed = 0;
                    if (!int.TryParse(item, out parsed))
                    {
                        throw new Exception($"Error parsing version numbers. The version/release string was '{versionString}'");
                    }
                    splitLatestInts.Add(parsed);
                }
            }
            return splitLatestInts;
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