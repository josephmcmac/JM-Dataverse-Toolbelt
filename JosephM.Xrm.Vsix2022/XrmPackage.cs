using EnvDTE80;
using JosephM.Application.Application;
using JosephM.Xrm.Vsix.App;
using JosephM.Xrm.Vsix.App.Application;
using JosephM.Xrm.Vsix.Application;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.ComponentModel.Design;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace JosephM.Xrm.Vsix
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
    [ProvideAutoLoad(UIContextGuids80.SolutionExists, PackageAutoLoadFlags.BackgroundLoad)]
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)] // Info on this package for Help/About
    [Guid(PackageGuidString)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    public sealed class XrmPackage : AsyncPackage
    {
        /// <summary>
        /// JosephM.Xrm.VsixPackage GUID string.
        /// </summary>
        public const string PackageGuidString = "e04baedb-126b-4840-91de-f9b159051606";

        #region Package Members

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initialization code that rely on services provided by VisualStudio.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token to monitor for initialization cancellation, which can occur when VS is shutting down.</param>
        /// <param name="progress">A provider for progress updates.</param>
        /// <returns>A task representing the async work of package initialization, or an already completed task if there is none. Do not return null from this method.</returns>
        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            await base.InitializeAsync(cancellationToken, progress);

            var container = new DependencyContainer();

            var menuCommandService = await GetServiceAsync(typeof(IMenuCommandService)) as IMenuCommandService;
            var xrmMenuCommandService = new XrmMenuCommandService(menuCommandService);
            container.RegisterInstance(typeof(IXrmMenuCommandService), xrmMenuCommandService);


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
