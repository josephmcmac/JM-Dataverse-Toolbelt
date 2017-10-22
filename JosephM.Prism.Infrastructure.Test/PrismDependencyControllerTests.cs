using System.Linq;
using JosephM.Application.Application;
using JosephM.Application.ViewModel.Dialog;
using JosephM.Application.ViewModel.Fakes;
using JosephM.Core.AppConfig;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using JosephM.Core.Test;
using JosephM.Core.Utility;
using JosephM.Prism.Infrastructure.Dialog;
using JosephM.Prism.Infrastructure.Module;
using JosephM.Prism.Infrastructure.Prism;
using JosephM.Record.Application.Fakes;
using JosephM.Core.Service;
using JosephM.Core.Attributes;
using JosephM.Core.Log;
using System;
using Microsoft.Practices.Unity;
using System.Collections;
using JosephM.Application.ViewModel.Grid;
using System.Collections.Generic;

namespace JosephM.Prism.Infrastructure.Test
{
    [TestClass]
    public class PrismDependencyControllerTests
    {
        [TestMethod]
        public void PrismDependencyControllerTestsObjectKeyTest()
        {
            //this is a test to verify for a module injecting custom grid functions for a type 
            var container = new PrismDependencyContainer(new UnityContainer());

            var type1List = new[] { new CustomGridFunction(typeof(TestResolveType).AssemblyQualifiedName, "Label", () => { }) };
            var type2List = new[] { new CustomGridFunction(typeof(TestResolveType2).AssemblyQualifiedName, "Label 2", () => { }) };

            container.RegisterInstance(typeof(List<CustomGridFunction>), typeof(TestResolveType).AssemblyQualifiedName, type1List);
            container.RegisterInstance(typeof(List<CustomGridFunction>), typeof(TestResolveType2).AssemblyQualifiedName, type2List);

            Assert.AreEqual(typeof(TestResolveType).AssemblyQualifiedName, ((List<CustomGridFunction>)container.ResolveInstance(typeof(IEnumerable<CustomGridFunction>), typeof(TestResolveType).AssemblyQualifiedName)).First().Id);
            Assert.AreEqual(typeof(TestResolveType2).AssemblyQualifiedName, ((List<CustomGridFunction>)container.ResolveInstance(typeof(IEnumerable<CustomGridFunction>), typeof(TestResolveType2).AssemblyQualifiedName)).First().Id);
        }

        public class TestResolveType
        {
            public int Int { get; set; }
        }

        public class TestResolveType2
        {
            public int Int { get; set; }
        }
    }
}