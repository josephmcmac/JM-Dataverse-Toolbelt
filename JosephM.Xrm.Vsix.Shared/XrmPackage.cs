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

namespace JosephM.Xrm.Vsix
{
    [ProvideAutoLoad(UIContextGuids80.SolutionExists, PackageAutoLoadFlags.BackgroundLoad)]
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)] // Info on this package for Help/About
    [Guid(PackageGuidString)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    public sealed class XrmPackage : AsyncPackage
    {
        public const string PackageGuidString = "e04baedb-126b-4840-91de-f9b159051606";

        protected override async System.Threading.Tasks.Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            await base.InitializeAsync(cancellationToken, progress);

            var container = new DependencyContainer();

            var menuCommandService = await GetServiceAsync(typeof(IMenuCommandService)) as IMenuCommandService;
            var xrmMenuCommandService = new XrmMenuCommandService(menuCommandService);
            container.RegisterInstance(typeof(IXrmMenuCommandService), xrmMenuCommandService);


            var dte = await GetServiceAsync(typeof(SDTE)) as DTE2;
            var visualStudioService = new VisualStudioService(dte);
            container.RegisterInstance(typeof(IVisualStudioService), visualStudioService);

            await JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);

            Factory.CreateJosephMXrmVsixApp(visualStudioService, container);

        }
    }
}
