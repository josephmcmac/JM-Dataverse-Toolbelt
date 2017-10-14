﻿using System.Windows;
using JosephM.CodeGenerator.Xrm;
using JosephM.CustomisationExporter.Exporter;
using JosephM.CustomisationImporter.Prism;
using JosephM.InstanceComparer;
using JosephM.Prism.Infrastructure.Prism;
using JosephM.Prism.XrmModule.SavedXrmConnections;
using JosephM.Prism.XrmModule.Xrm;
using JosephM.Xrm.ImportExporter.Prism;
using JosephM.Xrm.RecordExtract.RecordExtract;
using JosephM.RecordCounts.Exporter;
using JosephM.Prism.XrmModule.Crud;

namespace JosephM.Xrm.DeveloperTool
{
    /// <summary>
    ///     Interaction logic for App.xaml
    /// </summary>
    public partial class App : System.Windows.Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var prism = new PrismApplication("JosephM Xrm Developer Tool 2016");
            prism.AddModule<XrmModuleModule>();
            prism.AddModule<SavedXrmConnectionsModule>();
            prism.AddModule<XrmImporterExporterModule>();
            prism.AddModule<XrmSolutionImporterExporterModule>();
            prism.AddModule<XrmCodeGeneratorModule>();
            prism.AddModule<XrmRecordExtractModule>();
            prism.AddModule<CustomisationExporterModule>();
            prism.AddModule<CustomisationImportModule>();
            prism.AddModule<InstanceComparerModule>();
            prism.AddModule<RecordCountsModule>();
            prism.AddModule<XrmCrudModule>();
            prism.Run();
        }
    }
}