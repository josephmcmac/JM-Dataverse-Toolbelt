using JosephM.Application.Application;
using JosephM.Application.ViewModel.Grid;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace JosephM.Application.Desktop.Test
{
    [TestClass]
    public class DependencyContainerTests
    {
        [TestMethod]
        public void DependencyContainerTestsObjectKeyTest()
        {
            //this is a test to verify for a module injecting custom grid functions for a type 

            //create unity container
            var container = new DependencyContainer();

            //verify each type gets its dedicated list of custom functions returned
            var type1List = new CustomGridFunctions();
            type1List.AddFunction(new CustomGridFunction(typeof(TestResolveType).AssemblyQualifiedName, "Label", () => { }));
            var type2List = new CustomGridFunctions();
            type2List.AddFunction(new CustomGridFunction(typeof(TestResolveType2).AssemblyQualifiedName, "Label 2", () => { }));

            container.RegisterInstance(typeof(CustomGridFunctions), typeof(TestResolveType).AssemblyQualifiedName, type1List);
            container.RegisterInstance(typeof(CustomGridFunctions), typeof(TestResolveType2).AssemblyQualifiedName, type2List);

            Assert.AreEqual(typeof(TestResolveType).AssemblyQualifiedName, ((CustomGridFunctions)container.ResolveInstance(typeof(CustomGridFunctions), typeof(TestResolveType).AssemblyQualifiedName)).CustomFunctions.First().Id);
            Assert.AreEqual(typeof(TestResolveType2).AssemblyQualifiedName, ((CustomGridFunctions)container.ResolveInstance(typeof(CustomGridFunctions), typeof(TestResolveType2).AssemblyQualifiedName)).CustomFunctions.First().Id);

            //okay this one is autmatically created by the unity container 
            //but iteratively add and resolve 2 items and verify they are retained in the resolved list
            var createdByContainer = (CustomGridFunctions)container.ResolveInstance(typeof(CustomGridFunctions), typeof(TestResolveTypeNotRegistered).AssemblyQualifiedName);
            Assert.AreEqual(0, createdByContainer.CustomFunctions.Count());

            createdByContainer.AddFunction(new CustomGridFunction(typeof(TestResolveTypeNotRegistered).AssemblyQualifiedName, "Label X", () => { }));
            container.RegisterInstance(typeof(CustomGridFunctions), typeof(TestResolveTypeNotRegistered).AssemblyQualifiedName, type1List);
            createdByContainer = (CustomGridFunctions)container.ResolveInstance(typeof(CustomGridFunctions), typeof(TestResolveTypeNotRegistered).AssemblyQualifiedName);
            Assert.AreEqual(1, createdByContainer.CustomFunctions.Count());

            createdByContainer.AddFunction(new CustomGridFunction(typeof(TestResolveTypeNotRegistered).AssemblyQualifiedName, "Label Y", () => { }));
            container.RegisterInstance(typeof(CustomGridFunctions), typeof(TestResolveTypeNotRegistered).AssemblyQualifiedName, type1List);
            createdByContainer = (CustomGridFunctions)container.ResolveInstance(typeof(CustomGridFunctions), typeof(TestResolveTypeNotRegistered).AssemblyQualifiedName);
            Assert.AreEqual(2, createdByContainer.CustomFunctions.Count());
        }

        public class TestResolveType
        {
            public int Int { get; set; }
        }

        public class TestResolveType2
        {
            public int Int { get; set; }
        }

        public class TestResolveTypeNotRegistered
        {
            public int Int { get; set; }
        }
    }
}