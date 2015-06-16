using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using JosephM.Prism.XrmModule.Test;
using JosephM.Record.Application.Fakes;
using JosephM.Record.Application.RecordEntry;
using JosephM.Record.Application.RecordEntry.Field;
using JosephM.Record.Application.RecordEntry.Form;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using JosephM.Core.FieldType;
using JosephM.Core.Utility;
using JosephM.Record.Application.SettingTypes;
using JosephM.Record.Xrm.Test;
using JosephM.Record.Xrm.XrmRecord;
using JosephM.Xrm.ImportExporter.Service;

namespace JosephM.Xrm.ImporterExporter.Test
{
    [TestClass]
    public class XrmSolutionImporterExporterRequestTest : XrmModuleTest
    {

        [TestMethod]
        public void XrmImporterExporterRequestForSolutionExport()
        {
            PrepareTests();

            Assert.IsNotNull(TestAccount);

            var req = new XrmSolutionImporterExporterRequest();
            req.FolderPath = new Folder(TestingFolder);
            req.ImportExportTask = SolutionImportExportTask.ExportSolutions;

            var mainViewModel = new ObjectEntryViewModel(() => { }, () => { }, req, FormController.CreateForObject(req, CreateFakeApplicationController(), XrmRecordService));
            var solutionGrid = mainViewModel.SubGrids.First(r => r.ReferenceName == "SolutionExports");
            solutionGrid.AddRow();

            var row = solutionGrid.GridRecords.First();
            var connectionField = row.GetObjectFieldFieldViewModel("Connection");
            connectionField.Search();
            Assert.IsTrue(connectionField.LookupGridViewModel.GridRecords.Any());
            connectionField.SetValue(connectionField.LookupGridViewModel.GridRecords.First().GetRecord());

            var solutionField = row.GetLookupFieldFieldViewModel("Solution");
            solutionField.Search();
            Assert.IsTrue(solutionField.LookupGridViewModel.GridRecords.Any());
            solutionField.SetValue(solutionField.LookupGridViewModel.GridRecords.First().GetRecord());
        }
    }
}