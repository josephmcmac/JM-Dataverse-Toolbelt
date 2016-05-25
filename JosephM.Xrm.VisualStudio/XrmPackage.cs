//------------------------------------------------------------------------------
// <copyright file="Command1Package.cs" company="Company">
//     Copyright (c) Company.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.InteropServices;
using EnvDTE80;
using JosephM.Record.Xrm.XrmRecord;
using JosephM.XRM.VSIX.Commands.DeployAssembly;
using JosephM.XRM.VSIX.Commands.DeployWebResource;
using JosephM.XRM.VSIX.Commands.ManagePluginTriggers;
using JosephM.XRM.VSIX.Commands.RefreshConnection;
using JosephM.XRM.VSIX.Commands.RefreshSchema;
using JosephM.XRM.VSIX.Commands.RefreshSettings;
using JosephM.XRM.VSIX.Commands.UpdateAssembly;
using JosephM.XRM.VSIX.Utilities;

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
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [Guid(XrmPackage.PackageGuidString)]
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "pkgdef, VS and vsixmanifest are valid VS terms")]
    public sealed class XrmPackage : Package
    {
        /// <summary>
        /// Command1Package GUID string.
        /// </summary>
        public const string PackageGuidString = "cff1c42d-f82d-45f0-8649-3fc60c66d950";

        /// <summary>
        /// Initializes a new instance of the <see cref="RefreshSchemaCommand"/> class.
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
            RefreshSchemaCommand.Initialize(this);
            RefreshConnectionCommand.Initialize(this);
            DeployWebResourceCommand.Initialize(this);
            DeployAssemblyCommand.Initialize(this);
            ManagePluginTriggersCommand.Initialize(this);
            UpdateAssemblyCommand.Initialize(this);
            RefreshSettingsCommand.Initialize(this);

            base.Initialize();
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
