//------------------------------------------------------------------------------
// <copyright file="XrmPackage.cs" company="Company">
//     Copyright (c) Company.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.Win32;
using JosephM.Record.Xrm.XrmRecord;
using JosephM.XRM.VSIX.Utilities;
using EnvDTE80;
using JosephM.Prism.XrmModule.Crud;
using JosephM.XRM.VSIX.Dialogs;
using Microsoft.Practices.Unity;
using JosephM.Record.Application.Fakes;
using JosephM.Xrm.Vsix.Module;
using JosephM.Xrm.Vsix.Module.Web;
using JosephM.InstanceComparer;
using JosephM.Xrm.Vsix.Module.DeployAssembly;
using JosephM.Xrm.Vsix.Module.UpdateAssembly;
using JosephM.Xrm.Vsix.Module.PackageSettings;
using JosephM.Xrm.Vsix.Module.PluginTriggers;
using JosephM.Xrm.Vsix.Module.CustomisationImport;
using JosephM.Xrm.Vsix.Module.CreatePackage;
using JosephM.Xrm.Vsix.Module.RefreshSchema;
using JosephM.Xrm.Vsix.Module.DeployWebResource;
using JosephM.Xrm.Vsix.Module.ImportCsvs;
using JosephM.Xrm.Vsix.Module.DeployPackage;
using JosephM.Xrm.Vsix.Utilities;

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

            var container = new PrismDependencyContainer(new UnityContainer());

            var commandService = GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            container.RegisterInstance(typeof(IMenuCommandService), commandService);


            var dte = GetService(typeof(SDTE)) as DTE2;
            var visualStudioService = new VisualStudioService(dte);
            container.RegisterInstance(typeof(IVisualStudioService), visualStudioService);

            var applicationController = new VsixApplicationController(container);
            var app = new VsixApplication(applicationController, new VsixSettingsManager(visualStudioService), new Guid("43816e6d-4db8-48d6-8bfa-75916cb080f0"));

            app.AddModule<OpenWebModule>(0x010B);
            app.AddModule<OpenSolutionModule>(0x010C);
            app.AddModule<OpenAdvancedFindModule>(0x010D);
            app.AddModule<ClearCacheModule>(0x0109);
            app.AddModule<XrmCrudModule>(0x0112);
            app.AddModule<InstanceComparerModule>(0x0111);
            app.AddModule<UpdateAssemblyModule>(0x0105);
            app.AddModule<XrmPackageSettingsModule>(0x0106);
            app.AddModule<DeployAssemblyModule>(0x0103);
            app.AddModule<ManagePluginTriggersModule>(0x0104);
            app.AddModule<VsixCustomisationImportModule>(0x010A);
            app.AddModule<VsixCreatePackageModule>(0x010E);
            app.AddModule<RefreshSchemaModule>(0x0100);
            app.AddModule<DeployWebResourceModule>(0x0102);
            app.AddModule<VsixImportCsvsModule>(0x0108);
            app.AddModule<VsixDeployPackageModule> (0x0110); 
        }

        #endregion
    }
}
