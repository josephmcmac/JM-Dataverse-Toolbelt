//------------------------------------------------------------------------------
// <copyright file="XrmPackage.cs" company="Company">
//     Copyright (c) Company.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using EnvDTE80;
using JosephM.Application.Application;
using JosephM.Application.Desktop.Application;
using JosephM.Application.Desktop.Module.AboutModule;
using JosephM.Application.Desktop.Module.Themes;
using JosephM.CodeGenerator.JavaScriptOptions;
using JosephM.InstanceComparer;
using JosephM.Xrm.RecordExtract.TextSearch;
using JosephM.Xrm.Vsix;
using JosephM.Xrm.Vsix.App;
using JosephM.Xrm.Vsix.Application;
using JosephM.Xrm.Vsix.Module;
using JosephM.Xrm.Vsix.Module.CreatePackage;
using JosephM.Xrm.Vsix.Module.CustomisationImport;
using JosephM.Xrm.Vsix.Module.DeployAssembly;
using JosephM.Xrm.Vsix.Module.DeployIntoField;
using JosephM.Xrm.Vsix.Module.DeployPackage;
using JosephM.Xrm.Vsix.Module.DeployWebResource;
using JosephM.Xrm.Vsix.Module.ImportCsvs;
using JosephM.Xrm.Vsix.Module.ImportRecords;
using JosephM.Xrm.Vsix.Module.ImportSolution;
using JosephM.Xrm.Vsix.Module.PackageSettings;
using JosephM.Xrm.Vsix.Module.PluginTriggers;
using JosephM.Xrm.Vsix.Module.RefreshSchema;
using JosephM.Xrm.Vsix.Module.UpdateAssembly;
using JosephM.Xrm.Vsix.Module.Web;
using JosephM.XrmModule.Crud;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.ComponentModel.Design;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace JosephM.XRM.VSIX
{
    [ProvideAutoLoad(UIContextGuids80.SolutionExists)]
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)] // Info on this package for Help/About
    [Guid(PackageGuidString)]
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "pkgdef, VS and vsixmanifest are valid VS terms")]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    public sealed class XrmPackage : Package
    {
        public const string PackageGuidString = "e6f49165-430c-4175-b821-b126db4d680e";

        #region Package Members

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initialization code that rely on services provided by VisualStudio.
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();

            var container = new DependencyContainer();

            var commandService = GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            container.RegisterInstance(typeof(IMenuCommandService), commandService);


            var dte = GetService(typeof(SDTE)) as DTE2;
            var visualStudioService = new VisualStudioService(dte);
            container.RegisterInstance(typeof(IVisualStudioService), visualStudioService);

            var app = Factory.CreateJosephMXrmVsixApp(visualStudioService, container);
        }


        #endregion
    }
}
