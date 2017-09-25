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

namespace JosephM.XRM.VSIX
{
    /// <summary>
    /// This is the class that implements the package exposed by this assembly.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The minimum requirement for a class to be considered a valid package for Visual Studio
    /// is to implement the IVsPackage interface and register itself with the shell.
    /// This package uses the helper classes defined inside the Managed Package Framework (MPF)
    /// to do it: it derives from the Package class that provides the implementation of the
    /// IVsPackage interface and uses the registration attributes defined in the framework to
    /// register itself and its components with the shell. These attributes tell the pkgdef creation
    /// utility what data to put into .pkgdef file.
    /// </para>
    /// <para>
    /// To get loaded into VS, the package must be referred by &lt;Asset Type="Microsoft.VisualStudio.VsPackage" ...&gt; in .vsixmanifest file.
    /// </para>
    /// </remarks>
    [ProvideAutoLoad(UIContextGuids80.SolutionExists)]
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)] // Info on this package for Help/About
    [Guid(XrmPackage.PackageGuidString)]
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "pkgdef, VS and vsixmanifest are valid VS terms")]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    public sealed class XrmPackage : Package
    {
        /// <summary>
        /// XrmPackage GUID string.
        /// </summary>
        public const string PackageGuidString = "e6f49165-430c-4175-b821-b126db4d680e";

        /// <summary>
        /// Initializes a new instance of the <see cref="XrmPackage"/> class.
        /// </summary>
        public XrmPackage()
        {
            // Inside this method you can place any initialization code that does not require
            // any Visual Studio service because at this point the package object is created but
            // not sited yet inside Visual Studio environment. The place to do all the other
            // initialization is the Initialize method.
        }

        #region Package Members

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initialization code that rely on services provided by VisualStudio.
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();
            //Commands.RefreshConnection.RefreshConnectionCommand.Initialize(this);
            Commands.DeployWebResource.DeployWebResourceCommand.Initialize(this);
            Commands.DeployAssembly.DeployAssemblyCommand.Initialize(this);
            Commands.ManagePluginTriggers.ManagePluginTriggersCommand.Initialize(this);
            Commands.UpdateAssembly.UpdateAssemblyCommand.Initialize(this);
            Commands.RefreshSchema.RefreshSchemaCommand.Initialize(this);
            Commands.RefreshSettings.RefreshSettingsCommand.Initialize(this);
            Commands.GetSolution.GetSolutionCommand.Initialize(this);
            Commands.ImportCsvs.ImportCsvsCommand.Initialize(this);
            Commands.ClearCache.ClearCacheCommand.Initialize(this);
            Commands.ImportCustomisations.ImportCustomisationsCommand.Initialize(this);
            Commands.OpenWeb.OpenCrmWebCommand.Initialize(this);
            Commands.OpenWeb.OpenCrmSolutionCommand.Initialize(this);
            Commands.OpenWeb.OpenCrmAdvancedFindCommand.Initialize(this);
            Commands.CreateDeploymentPackage.CreateDeploymentPackageCommand.Initialize(this);
            Commands.DeployPackage.DeployPackageCommand.Initialize(this);
        }

        private XrmRecordService _xrmRecordService;

        private object _lockObject = new Object();

        public XrmRecordService GetXrmRecordService()
        {
            //simple cache + if configuration has change then create new
            var xrmConfig = VsixUtility.GetXrmConfig(this);
            lock (_lockObject)
            {
                if (_xrmRecordService != null)
                {
                    //if something in the config different clear the service
                    var oldConfig = _xrmRecordService.XrmRecordConfiguration;
                    if (oldConfig.AuthenticationProviderType != xrmConfig.AuthenticationProviderType
                        || oldConfig.DiscoveryServiceAddress != xrmConfig.DiscoveryServiceAddress
                        || oldConfig.OrganizationUniqueName != xrmConfig.OrganizationUniqueName
                        || oldConfig.Domain != xrmConfig.Domain
                        || oldConfig.Username != xrmConfig.Username
                        || oldConfig.Password == null && xrmConfig.Password != null
                        || oldConfig.Password != null && xrmConfig.Password == null
                        ||
                        (oldConfig.Password != null && xrmConfig.Password != null &&
                         oldConfig.Password.GetRawPassword() != xrmConfig.Password.GetRawPassword())
                        )
                        _xrmRecordService = null;
                }
                if (_xrmRecordService == null)
                {
                    _xrmRecordService = new XrmRecordService(xrmConfig);
                }
                return _xrmRecordService;
            }
        }

        public VisualStudioService GetVisualStudioService()
        {
            var dte = GetService(typeof(SDTE)) as DTE2;
            return new VisualStudioService(dte);
        }
        #endregion
    }
}
