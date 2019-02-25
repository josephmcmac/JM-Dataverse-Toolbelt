//------------------------------------------------------------------------------
// <copyright file="XrmPackage.cs" company="Company">
//     Copyright (c) Company.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using EnvDTE80;
using JosephM.Application.Application;
using JosephM.Xrm.Vsix.App;
using JosephM.Xrm.Vsix.Application;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.ComponentModel.Design;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Threading;

namespace JosephM.XRM.VSIX
{
    [ProvideAutoLoad(UIContextGuids80.SolutionExists, PackageAutoLoadFlags.BackgroundLoad)]
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)] // Info on this package for Help/About
    [Guid(PackageGuidString)]
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "pkgdef, VS and vsixmanifest are valid VS terms")]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    public sealed class XrmPackage : AsyncPackage
    {
        public const string PackageGuidString = "e6f49165-430c-4175-b821-b126db4d680e";

        #region Package Members

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initialization code that rely on services provided by VisualStudio.
        /// </summary>
        protected override async System.Threading.Tasks.Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            await base.InitializeAsync(cancellationToken, progress);

            var container = new DependencyContainer();

            var commandService = await GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            container.RegisterInstance(typeof(IMenuCommandService), commandService);


            var dte = await GetServiceAsync(typeof(SDTE)) as DTE2;
            var visualStudioService = new VisualStudioService(dte);
            container.RegisterInstance(typeof(IVisualStudioService), visualStudioService);

            //there is code in the module loading which adds buttons to the vs runtime
            //by calling a method which is apparently not allowed to be called in asynch await threads
            //this line of code before it appears to switch to the main thread which avoids the problem
            //as per these links
            //https://github.com/madskristensen/CustomCommandSample
            //https://social.msdn.microsoft.com/Forums/vstudio/en-US/dc8ba70b-0faf-4a16-96a4-1c57dac3e7a7/cant-get-olemenucommandservice-asynchronously-when-implementing-iasyncloadablepackageinitialize?forum=vsx
            await JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);

            var app = Factory.CreateJosephMXrmVsixApp(visualStudioService, container);
        }


        #endregion
    }
}
