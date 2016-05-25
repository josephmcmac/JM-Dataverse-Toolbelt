using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using JosephM.Application.ViewModel.Fakes;
using JosephM.Application.ViewModel.RecordEntry.Form;
using JosephM.Record.IService;
using JosephM.Record.Query;
using JosephM.XRM.VSIX.Commands.DeployAssembly;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using JosephM.Record.Xrm.Test;
using JosephM.Xrm.Schema;

namespace JosephM.Xrm.Vsix.Test
{
    public class JosephMVsixTests : XrmRecordTest
    {
        public static string GetTestPluginAssemblyName()
        {
            var file = new FileInfo(GetTestPluginAssemblyFile());
            return file.Name.Substring(0, file.Name.LastIndexOf(".", StringComparison.Ordinal));
        }

        public static string GetTestPluginAssemblyFile()
        {
            var rootFolder = GetRootFolder();
            var pluginAssembly = Path.Combine(rootFolder.FullName, "TestFiles", "PluginAssemblyBin",
                PluginAssemblyName + ".dll");
            return pluginAssembly;
        }

        public void DeployAssembly()
        {
            var createDialog = new DeployAssemblyDialog(new FakeDialogController(new FakeApplicationController()),
                GetTestPluginAssemblyFile(), XrmRecordService);
            createDialog.Controller.BeginDialog();

            var objectEntry = (ObjectEntryViewModel)createDialog.Controller.UiItems.First();
            objectEntry.OnSave();

            Assert.AreEqual(1, GetTestPluginAssemblyRecords().Count());
        }
        public static DirectoryInfo GetRootFolder()
        {

            var assemblyLocation = new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath;
            var fileInfo = new FileInfo(assemblyLocation);
            var rootFolder = fileInfo.Directory.Parent.Parent;
            return rootFolder;
        }

        public static string PluginAssemblyName
        {
            get { return "TestXrmSolution.Plugins"; }
        }

        public void DeleteTestPluginAssembly()
        {
            var assemblyRecords = GetTestPluginAssemblyRecords();
            foreach (var assembly in assemblyRecords)
            {
                DeletePluginTriggers(assembly);
                XrmRecordService.Delete(assembly);
            }
        }

        public void DeletePluginTriggers(IRecord assemblyRecord)
        {
            var pluginTriggers = GetPluginTriggers(assemblyRecord);
            foreach (var item in pluginTriggers)
            {
                XrmRecordService.Delete(item);
            }
        }

        public IEnumerable<IRecord> GetPluginTriggers(IRecord assemblyRecord)
        {
            var pluginTypes = XrmRecordService.RetrieveAllAndClauses(Entities.plugintype,
                new[] { new Condition(Fields.plugintype_.pluginassemblyid, ConditionType.Equal, assemblyRecord.Id) });
            if (!pluginTypes.Any())
                throw new NullReferenceException("Not Plugin Types Deployed For Assembly");

            return XrmRecordService.RetrieveAllOrClauses(Entities.sdkmessageprocessingstep,
                pluginTypes.Select(
                    pt => new Condition(Fields.sdkmessageprocessingstep_.plugintypeid, ConditionType.Equal, pt.Id)));
        }

        public IEnumerable<IRecord> GetTestPluginAssemblyRecords()
        {
            var assemblyRecords = XrmRecordService.RetrieveAllAndClauses(Schema.Entities.pluginassembly, new[]
            {
                new Condition(Fields.pluginassembly_.name, ConditionType.Equal, PluginAssemblyName)
            });
            return assemblyRecords;
        }
    }
}
