using JosephM.Application.ViewModel.Fakes;
using JosephM.Application.ViewModel.RecordEntry.Form;
using JosephM.Core.Attributes;
using JosephM.Core.FieldType;
using JosephM.ObjectMapping;
using JosephM.Record.Attributes;
using JosephM.Record.Extentions;
using JosephM.Record.Xrm.XrmRecord;
using JosephM.Xrm.Schema;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace JosephM.XrmModule.Test
{
    [TestClass]
    public class XrmLookupServiceTests : XrmModuleTest
    {
        [TestMethod]
        public void XrmLookupServiceVerifyDoesNotCrashIfConnectionDoesNotWork()
        {
            try
            {
                var solution = ReCreateTestSolution();
                var testEntryObject = new TestXrmObjectEntryClass()
                {
                    XrmLookupField = solution.ToLookup()
                };
                var classSelfMapper = new ClassSelfMapper();
                var newConnection = classSelfMapper.Map(GetXrmRecordConfiguration());
                newConnection.WebUrl = "Foo";
                var newService = new XrmRecordService(newConnection, ServiceFactory);
                var objectEntryViewModel = new ObjectEntryViewModel(null, null, testEntryObject, FakeFormController.CreateForObject(testEntryObject, new FakeApplicationController(), newService));
                objectEntryViewModel.LoadFormSections();
                Assert.IsNotNull(testEntryObject.XrmLookupField);
                Assert.IsNotNull(testEntryObject.XrmLookupFieldCascaded);
            }
            catch(FakeUserMessageException)
            {
            }
        }

        /// <summary>
        ///verifies a cascade on load does not cause a fatal error if
        ///the record referenced by a lookup field with a cascade does not exist
        /// </summary>
        [TestMethod]
        public void XrmLookupServiceVerifyDoesNotCrashIfReferencedRecordDeleted()
        {
            //create object referencing a deleted record
            var solution = ReCreateTestSolution();
            var testEntryObject = new TestXrmObjectEntryClass()
            {
                XrmLookupField = solution.ToLookup()
            };
            XrmRecordService.Delete(solution);

            //verify the form loads and the invalid value is lceared
            var objectEntryViewModel = new ObjectEntryViewModel(null, null, testEntryObject, FakeFormController.CreateForObject(testEntryObject, new FakeApplicationController(), XrmRecordService));
            objectEntryViewModel.LoadFormSections();
            Assert.IsNull(testEntryObject.XrmLookupField);

            //lets just verify the cascade worked if we had not deleted it
            solution = ReCreateTestSolution();
            testEntryObject = new TestXrmObjectEntryClass()
            {
                XrmLookupField = solution.ToLookup()
            };
            objectEntryViewModel = new ObjectEntryViewModel(null, null, testEntryObject, FakeFormController.CreateForObject(testEntryObject, new FakeApplicationController(), XrmRecordService));
            objectEntryViewModel.LoadFormSections();
            Assert.IsNotNull(testEntryObject.XrmLookupField);
            Assert.IsNotNull(testEntryObject.XrmLookupFieldCascaded);
        }

        public class TestXrmObjectEntryClass
        {
            [ReferencedType(Entities.solution)]
            [LookupCondition(Fields.solution_.ismanaged, false)]
            [LookupCondition(Fields.solution_.isvisible, true)]
            [LookupFieldCascade(nameof(XrmLookupFieldCascaded), Fields.solution_.version)]
            [UsePicklist(Fields.solution_.uniquename)]
            public Lookup XrmLookupField { get; set; }

            public string XrmLookupFieldCascaded { get; set; }
        }
    }
}