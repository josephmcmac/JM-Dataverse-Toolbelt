using JosephM.Application.ViewModel.SettingTypes;
using JosephM.Core.FieldType;
using JosephM.Core.Utility;
using JosephM.CustomisationExporter.Exporter;
using JosephM.Xrm.Schema;
using JosephM.XrmModule.Test;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Threading;

namespace JosephM.CustomisationExporter.Test
{
    [TestClass]
    public class CustomisationExporterTest : XrmModuleTest
    {
        [TestMethod]
        public void CustomisationExporterTestExport()
        {
            FileUtility.DeleteFiles(TestingFolder);
            Assert.IsFalse(FileUtility.GetFiles(TestingFolder).Any());

            //okay script through generation of the three types

            //create test application with module loaded
            var testApplication = CreateAndLoadTestApplication<CustomisationExporterModule>();

            //xlsx

            //first script generation of C# entities and fields
            var request = new CustomisationExporterRequest
            {
                IncludeAllRecordTypes = true,
                DuplicateManyToManyRelationshipSides = true,
                Entities = true,
                Fields = true,
                FieldOptionSets = true,
                Relationships = true,
                SharedOptionSets = true,
                IncludeOneToManyRelationships = true,

                Solutions = true,
                Workflows = true,
                PluginAssemblies = true,
                PluginTriggers = true,
                SecurityRoles = true,
                FieldSecurityProfiles = true,
                Users = true,
                Teams = true,
                Reports = true,
                WebResources = true,
                FormsAndDashboards = true,

                SaveToFolder = new Folder(TestingFolder)
            };

            var response = testApplication.NavigateAndProcessDialog<CustomisationExporterModule, CustomisationExporterDialog, CustomisationExporterResponse>(request);
            Assert.IsFalse(response.HasError);
            Assert.IsTrue(FileUtility.GetFiles(TestingFolder).Any());

            request.IncludeAllRecordTypes = true;
            request.DuplicateManyToManyRelationshipSides = false;
            request.Entities = true;
            request.Fields = false;
            request.FieldOptionSets = false;
            request.Relationships = true;
            request.SharedOptionSets = false;
            request.IncludeOneToManyRelationships = false;

            request.Solutions = false;
            request.Workflows = false;
            request.PluginAssemblies = false;
            request.PluginTriggers = false;
            request.SecurityRoles = false;
            request.FieldSecurityProfiles = false;
            request.Users = false;
            request.Teams = false;
            request.Reports = false;
            request.WebResources = false;
            request.FormsAndDashboards = false;

            Thread.Sleep(1000);
            FileUtility.DeleteFiles(TestingFolder);
            
            response = testApplication.NavigateAndProcessDialog<CustomisationExporterModule, CustomisationExporterDialog, CustomisationExporterResponse>(request);
            Assert.IsFalse(response.HasError);
            Assert.IsTrue(FileUtility.GetFiles(TestingFolder).Any());

            request.IncludeAllRecordTypes = false;
            request.DuplicateManyToManyRelationshipSides = true;
            request.Entities = true;
            request.Fields = true;
            request.FieldOptionSets = true;
            request.Relationships = true;
            request.SharedOptionSets = true;
            request.IncludeOneToManyRelationships = true;
            request.RecordTypes = new[]
            {
                new RecordTypeSetting(Entities.account, Entities.account),
                new RecordTypeSetting(Entities.contact, Entities.contact)
            };

            Thread.Sleep(1000);
            FileUtility.DeleteFiles(TestingFolder);

            response = testApplication.NavigateAndProcessDialog<CustomisationExporterModule, CustomisationExporterDialog, CustomisationExporterResponse>(request);
            Assert.IsFalse(response.HasError);
            Assert.IsTrue(FileUtility.GetFiles(TestingFolder).Any());

            request = new CustomisationExporterRequest
            {
                Format = CustomisationExporterRequest.FileFormat.Csv,
                IncludeAllRecordTypes = true,
                DuplicateManyToManyRelationshipSides = true,
                Entities = true,
                Fields = true,
                FieldOptionSets = true,
                Relationships = true,
                SharedOptionSets = true,
                IncludeOneToManyRelationships = true,

                Solutions = true,
                Workflows = true,
                PluginAssemblies = true,
                PluginTriggers = true,
                SecurityRoles = true,
                FieldSecurityProfiles = true,
                Users = true,
                Teams = true,
                Reports = true,
                WebResources = true,
                FormsAndDashboards = true,

                SaveToFolder = new Folder(TestingFolder)
            };

            Thread.Sleep(1000);
            FileUtility.DeleteFiles(TestingFolder);

            response = testApplication.NavigateAndProcessDialog<CustomisationExporterModule, CustomisationExporterDialog, CustomisationExporterResponse>(request);
            Assert.IsFalse(response.HasError);
            Assert.IsTrue(FileUtility.GetFiles(TestingFolder).Any());
        }
    }
}