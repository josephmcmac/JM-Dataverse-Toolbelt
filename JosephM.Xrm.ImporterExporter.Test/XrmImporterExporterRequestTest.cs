using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
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
    public class XrmImporterExporterRequestTest : XrmRecordTest
    {
        [TestMethod]
        public void XrmImporterExporterRequestCheckSerialise()
        {
            //to do refactor this generic and could use to validate all types

            var me = new XrmImporterExporterRequest();
            var fileName = Path.Combine(TestingFolder, "testobject.xml");
            if(File.Exists(fileName))
                File.Delete(fileName);
            Assert.IsFalse(File.Exists(fileName));
            PopulateObject(me);
            var controller = new FakeApplicationController();
            controller.SeralializeObjectToFile(me, fileName);

            Assert.IsTrue(File.Exists(fileName));
        }

        [TestMethod]
        public void XrmImporterExporterRequest()
        {
            PrepareTests();

            Assert.IsNotNull(TestAccount);


            var req = new XrmImporterExporterRequest();
            req.FolderPath = new Folder(TestingFolder);
            req.ImportExportTask = ImportExportTask.ExportXml;

            var exportType = new ImportExportRecordType();
            exportType.RecordType = new RecordType("activitypointer", "Activity");
            req.RecordTypes = new[] {exportType};

            var mainViewModel = new ObjectEntryViewModel(() => { }, () => { }, req, FormController.CreateForObject(req, new FakeApplicationController(), XrmRecordService));
            var recordTypeGrid = mainViewModel.SubGrids.First(r => r.ReferenceName == "RecordTypes");
            var recordType = recordTypeGrid.GridRecords.First();
            var recordTypeEditViewModel = recordTypeGrid.GetEditRowViewModel(recordType);

            var excludeFieldsGrid = recordTypeEditViewModel.SubGrids.First(sg => sg.ReferenceName == "ExcludeFields");
            excludeFieldsGrid.AddRow();

            var row = excludeFieldsGrid.GridRecords.First();
            Assert.IsTrue(row.GetRecordFieldFieldViewModel("RecordField").ItemsSource.Any());

            //todo comment some of this and check if tidy up
            //this bit validates creating child forms
            //including the dpendant lookup types etc. cascade to childr forms
            var recordTypeViewModel = recordTypeEditViewModel.GetRecordTypeFieldViewModel("RecordType");
            recordTypeViewModel.Value = new RecordType("account", "Account");
            var exportTypeViewModel = recordTypeEditViewModel.GetPicklistFieldFieldViewModel("Type");
            exportTypeViewModel.ValueObject = ExportType.SpecificRecords;
            Assert.IsFalse(excludeFieldsGrid.GridRecords.Any());

            excludeFieldsGrid.AddRow();
            excludeFieldsGrid.AddRow();
            var row1 = excludeFieldsGrid.GridRecords.First();
            var field1 = row1.GetRecordFieldFieldViewModel("RecordField");
            var option1 = field1.ItemsSource.First();
            field1.Value = new RecordField(option1.Key, option1.Value);
            var row2 = excludeFieldsGrid.GridRecords.ElementAt(1);
            var field2 = row2.GetRecordFieldFieldViewModel("RecordField");
            var option2 = field2.ItemsSource.ElementAt(1);
            field2.Value = new RecordField(option2.Key, option2.Value);

            var specificRecordsViewModel = recordTypeEditViewModel.GetSubGridViewModel("OnlyExportSpecificRecords");
            specificRecordsViewModel.AddRow();
            var specifcRow = specificRecordsViewModel.GridRecords.First();
            var specificRowRecordLookup = specifcRow.GetLookupFieldFieldViewModel("Record");
            //so above we set the forms type to account so the record lookup in the grid should have target type of account (due to lookupfor attribute)
            Assert.AreEqual("account", specificRowRecordLookup.RecordTypeToLookup);
            //when open an edit row it should retain that record type
            var editSpecifcRow = specificRecordsViewModel.GetEditRowViewModel(specifcRow);
            var sections = editSpecifcRow.FormSectionsAsync;
            var editSpecificRowRecordLookup = editSpecifcRow.GetLookupFieldFieldViewModel("Record");
            Assert.AreEqual("account", editSpecificRowRecordLookup.RecordTypeToLookup);
            editSpecificRowRecordLookup.EnteredText = TestAccount.GetStringField("name");
            editSpecificRowRecordLookup.Search();
            Assert.IsTrue(editSpecificRowRecordLookup.LookupGridViewModel.GridRecords.Any());
            editSpecificRowRecordLookup.OnRecordSelected(editSpecificRowRecordLookup.LookupGridViewModel.GridRecords.First().Record);
            editSpecifcRow.OnSave();
            specifcRow = specificRecordsViewModel.GridRecords.First();
            specificRowRecordLookup = specifcRow.GetLookupFieldFieldViewModel("Record");
            Assert.IsNotNull(specificRowRecordLookup.EnteredText);

            Assert.IsTrue(recordTypeEditViewModel.Validate());
            recordTypeEditViewModel.OnSave();

            recordTypeGrid = mainViewModel.SubGrids.First(r => r.ReferenceName == "RecordTypes");
            recordType = recordTypeGrid.GridRecords.First();

        }
    }
}