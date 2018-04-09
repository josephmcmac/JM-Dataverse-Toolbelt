#region

using JosephM.Wpf.Application;
using Microsoft.Practices.Unity;
using Prism.Modularity;
using Prism.Unity;
using System;
using System.Collections.Generic;
using System.Windows;

#endregion

namespace JosephM.Prism.Infrastructure.Prism
{
    /// <summary>
    ///     Extention Of The UnityBootstrapper To Load The Custom Modules And Infrastucture Required by The Application
    ///     framework
    /// </summary>
    internal class UnityBootstrapperExtention : UnityBootstrapper
    {
        public UnityBootstrapperExtention(PrismApplication application, IEnumerable<Type> modules)
        {
            Modules = modules;
            PrismApplication = application;
        }

        private PrismApplication PrismApplication { get; set; }

        private IEnumerable<Type> Modules { get; set; }

        protected override DependencyObject CreateShell()
        {
            return Container.Resolve<Shell>();
        }

        protected override void InitializeShell()
        {
            base.InitializeShell();

            this.InitialiseLoadShell();

            System.Windows.Application.Current.MainWindow = (Window)Shell;
            System.Windows.Application.Current.MainWindow.DataContext = PrismApplication;
            System.Windows.Application.Current.MainWindow.Show();
        }

        protected override void ConfigureModuleCatalog()
        {
            foreach (var module in Modules)
                AddModule(module);
        }

        private void AddModule(Type xrmModuleType)
        {
            ModuleCatalog.AddModule(new ModuleInfo
            {
                ModuleName = xrmModuleType.FullName,
                ModuleType = xrmModuleType.AssemblyQualifiedName,
                InitializationMode = InitializationMode.WhenAvailable
            });
        }

        protected override void ConfigureContainer()
        {
            base.ConfigureContainer();
            this.RegisterInfrastructure(PrismApplication.ApplicationName);
        }
    }
}