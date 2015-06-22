using System;
using JosephM.Core.FieldType;
using JosephM.Core.Test;
using JosephM.Core.Utility;
using JosephM.Prism.XrmModule.Test;
using JosephM.Record.Xrm.Test;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JosephM.Xrm.CustomisationExporter.Test
{
    [TestClass]
    public class CustomisationExporterTest : XrmModuleTest
    {
        [TestMethod]
        public void CustomisationExporterTestExport()
        {
            FileUtility.DeleteFiles(TestingFolder);

            var request = new CustomisationExporterRequest();
            request.AllRecordTypes = true;
            request.DuplicateManyToManyRelationshipSides = true;
            request.ExportEntities = true;
            request.ExportFields = true;
            request.ExportOptionSets = true;
            request.ExportRelationships = true;
            request.ExportSharedOptionSets = true;
            request.IncludeOneToManyRelationships = true;
            request.SaveToFolder = new Folder(TestingFolder);

            //just add the statement verify loads correctly to viewmodel
            var viewModel = CreateObjectEntryViewModel(request);
            //run the export and verify no errors thrown
            var service = new CustomisationExporterService(XrmRecordService);
            var response = service.Execute(request, Controller);
            Assert.IsFalse(response.HasError);
        }
    }
}
