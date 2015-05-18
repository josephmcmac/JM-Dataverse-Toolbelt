using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using JosephM.Core.Attributes;
using JosephM.Record.Application.Fakes;
using JosephM.Record.Application.RecordEntry;
using JosephM.Record.Application.RecordEntry.Form;
using JosephM.Record.Application.RecordEntry.Metadata;
using JosephM.Record.Service;

namespace JosephM.Record.Application.Test
{
    [TestClass]
    public class ObjectEntryViewModelTests : RecordApplicationTests
    {
        [TestMethod]
        public void ObjectEntryViewModelSaveAndLoadTests()
        {
            var objectToEnter = new TestViewModelValidationObject();
            var viewModel = LoadToObjectEntryViewModel(objectToEnter);

            Assert.IsFalse(viewModel.Validate());
            //check the string property validates as required
            var requiredStringViewModel = viewModel.GetFieldViewModel("RequiredString");
            var requiredSubGrid = viewModel.GetSubGridViewModel("RequiredIEnumerableProperty");

            requiredStringViewModel.ValueObject = "Populate";
            requiredSubGrid.AddRow();
            var gridRow1 = requiredSubGrid.GridRecords.First();
            var notRequiredGridFieldViewModel = gridRow1.GetFieldViewModel("NotRequiredString");
            Assert.IsFalse(notRequiredGridFieldViewModel.HasErrors);
            var requiredGridFieldViewModel = gridRow1.GetFieldViewModel("RequiredString");
            requiredGridFieldViewModel.ValueObject = "Something";

            viewModel.SaveObject(TestingFolder);

            objectToEnter = new TestViewModelValidationObject();
            viewModel = LoadToObjectEntryViewModel(objectToEnter);
            viewModel.LoadObject(Path.Combine(TestingFolder, objectToEnter.GetType().Name + ".xml"));

            requiredStringViewModel = viewModel.GetFieldViewModel("RequiredString");
            Assert.IsNotNull(requiredStringViewModel.ValueObject);
            requiredSubGrid = viewModel.GetSubGridViewModel("RequiredIEnumerableProperty");
            Assert.IsTrue(requiredSubGrid.GridRecords.Any());
        }
    }
}