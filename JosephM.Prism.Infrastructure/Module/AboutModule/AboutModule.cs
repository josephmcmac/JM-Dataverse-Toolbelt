using JosephM.Application.Modules;
using JosephM.Core.Attributes;
using JosephM.Core.FieldType;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace JosephM.Application.Prism.Module.AboutModule
{
    [MyDescription("About This Application")]
    /// <summary>
    ///     Base Class For A Module Which Plugs A Settings Type Into The Application
    /// </summary>
    public abstract class AboutModule : SettingsModuleBase
    {
        public override void RegisterTypes()
        {
            var about = new About()
            {
                Application = ApplicationController.ApplicationName,
                Version = GetVersionString(),
                CodeLink = CodeLink == null ? null : new Url(CodeLink, "Source Code"),
                CreateIssueLink = CreateIssueLink == null ? null : new Url(CreateIssueLink, "Create Issue"),
                AboutDetail = AboutDetail,
                OtherLink = OtherLink
            };
            RegisterInstance(about);
            RegisterTypeForNavigation<AboutDialog>();
        }

        public virtual Assembly MainAssembly
        {
            get
            {
                return Assembly.GetEntryAssembly();
            }
        }

        private string GetVersionString()
        {
            var installedVersion = GetInstalledApplicationVersion();
            if (installedVersion != null)
                return installedVersion;
            return MainAssembly == null ? null : "v " + AssemblyName.GetAssemblyName(MainAssembly.Location).Version?.ToSt‌​ring();
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

        public virtual string CreateIssueLink { get; }

        public virtual string CodeLink { get; }

        public virtual Url OtherLink { get; }

        public abstract string AboutDetail { get; }

        public override string MainOperationName => "About";

        public override void DialogCommand()
        {
            NavigateTo<AboutDialog>();
        }
    }
}