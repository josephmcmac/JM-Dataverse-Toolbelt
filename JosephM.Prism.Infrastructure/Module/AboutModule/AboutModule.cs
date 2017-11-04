using JosephM.Application.Modules;
using JosephM.Core.FieldType;
using System;
using System.Reflection;

namespace JosephM.Application.Prism.Module.AboutModule
{
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
            return MainAssembly == null ? null : "v " + AssemblyName.GetAssemblyName(MainAssembly.Location).Version?.ToSt‌​ring();
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