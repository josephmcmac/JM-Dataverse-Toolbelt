using JosephM.Application.Modules;
using JosephM.Core.Attributes;
using JosephM.Core.FieldType;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace JosephM.Application.Desktop.Module.AboutModule
{
    [MyDescription("About This Application")]
    /// <summary>
    ///     Base Class For A Module Which Plugs A Settings Type Into The Application
    /// </summary>
    public abstract class AboutModule : SettingsModuleBase
    {
        public override int SettingsOrder => 99999;

        public override void RegisterTypes()
        {
            var about = new About()
            {
                Application = ApplicationController.ApplicationName,
                Version = ApplicationController.Version,
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