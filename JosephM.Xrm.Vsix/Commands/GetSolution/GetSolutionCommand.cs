//------------------------------------------------------------------------------
// <copyright file="Command1.cs" company="Company">
//     Copyright (c) Company.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using JosephM.XRM.VSIX.Dialogs;
using JosephM.XRM.VSIX.Utilities;
using JosephM.Application.ViewModel.Dialog;
using JosephM.Xrm.ImportExporter.Service;
using JosephM.ObjectMapping;
using JosephM.Prism.XrmModule.SavedXrmConnections;
using JosephM.Core.FieldType;
using System.Windows;
using JosephM.Core.Extentions;

namespace JosephM.XRM.VSIX.Commands.GetSolution
{
    internal sealed class GetSolutionCommand : CommandBase
    {
        public override int CommandId
        {
            get { return 0x0107; }
        }

        private GetSolutionCommand(XrmPackage package)
            : base(package)
        {
        }

        public static GetSolutionCommand Instance { get; private set; }


        public static void Initialize(XrmPackage package)
        {
            Instance = new GetSolutionCommand(package);
        }

        public override void MenuItemCallback(object sender, EventArgs e)
        {
            try
            {
                DoDialog();
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.DisplayString());
            }
        }

        private void DoDialog()
        {
            var settings = VsixUtility.GetPackageSettings(GetDte2());
            if (settings == null)
                settings = new XrmPackageSettings();

            if (settings.Solution == null)
                throw new NullReferenceException("Solution is not populated in the package settings");

            var xrmRecordService = GetXrmRecordService();
            var visualStudioService = GetVisualStudioService();

            var mapper = new ClassSelfMapper();
            var savedConnection = new SavedXrmRecordConfiguration();
            mapper.Map(xrmRecordService.XrmRecordConfiguration, savedConnection);

            var folderPath = visualStudioService.SolutionDirectory + "/Customisations";

            var request = new XrmSolutionImporterExporterRequest()
            {
                ImportExportTask = SolutionImportExportTask.ExportSolutions,
                FolderPath = new Folder(folderPath),
                SolutionExports = new[]
                   {
                       new SolutionExport()
                       {
                            Connection = savedConnection,
                            ExportAsManaged = false,
                            Solution = settings.Solution
                       }
                   }
            };

            var controller = new DialogController(new VsixApplicationController("VSIX", null));

            var service = new XrmSolutionImporterExporterService();

            var dialog = new GetSolutionDialog(service, request, controller, xrmRecordService, settings, visualStudioService);

            DialogUtility.LoadDialog(dialog);
        }
    }
}
