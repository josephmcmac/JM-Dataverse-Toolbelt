using EnvDTE;
using EnvDTE80;
using JosephM.Core.Extentions;
using JosephM.Core.Serialisation;
using JosephM.Record.IService;
using JosephM.Record.Query;
using JosephM.Record.Xrm.XrmRecord;
using JosephM.Xrm.Schema;
using JosephM.Xrm.Vsix.Utilities;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Condition = JosephM.Record.Query.Condition;

namespace JosephM.XRM.VSIX.Utilities
{
    //todo this should all be in the visual studio service
    public static class VsixUtility
    {
        public static string BuildSelectedProjectAndGetAssemblyName(IServiceProvider serviceProvider)
        {
            var dte = GetDte2(serviceProvider);
            var build = dte.Solution.SolutionBuild;
            build.Clean(true);
            build.Build(true);
            var info = build.LastBuildInfo;

            if (info == 0)
            {
                var selectedItems = dte.SelectedItems;
                foreach (SelectedItem item in selectedItems)
                {
                    var project = item.Project;
                    if (project.Name != null)
                    {
                        var assemblyName = GetProperty(project.Properties, "AssemblyName");
                        var outputPath =
                            GetProperty(project.ConfigurationManager.ActiveConfiguration.Properties,
                                "OutputPath");
                        var fileInfo = new FileInfo(project.FullName);
                        var rootFolder = fileInfo.DirectoryName;
                        var outputFolder = Path.Combine(rootFolder ?? "", outputPath);
                        var assemblyFile = Path.Combine(outputFolder, assemblyName) + ".dll";
                        return assemblyFile;
                    }
                }
            }
            return null;
        }

        public static XrmPackageSettings GetPackageSettings(DTE2 dte)
        {
            var name = "xrmpackage.xrmsettings";

            string read = GetSolutionItemText(dte, name);
            if (string.IsNullOrEmpty(read))
                return new XrmPackageSettings();
            return (XrmPackageSettings)JsonHelper.JsonStringToObject(read, typeof(XrmPackageSettings));
        }

        public static XrmRecordConfiguration GetXrmConfig(IServiceProvider serviceProvider, bool newIfNull = false)
        {
            var name = "solution.xrmconnection";

            string read = GetSolutionItemText(GetDte2(serviceProvider), name);
            if(!newIfNull && string.IsNullOrEmpty(read))
                throw new NullReferenceException(string.Format("Error reading {0} in SolutionItems", name));

            var dictionary = string.IsNullOrEmpty(read)
                ? new Dictionary<string, string>()
                : (Dictionary<string, string>)
                    JsonHelper.JsonStringToObject(read, typeof(Dictionary<string, string>));

            var xrmConfig = new XrmRecordConfiguration();
            foreach (var prop in xrmConfig.GetType().GetReadWriteProperties())
            {
                if (dictionary.ContainsKey(prop.Name))
                    xrmConfig.SetPropertyByString(prop.Name, dictionary[prop.Name]);
            }
            return xrmConfig;
        }

        private static string GetSolutionItemText(DTE2 dte, string name)
        {
            string fileName = null;
            var solutionItems = GetProject(dte.Solution as Solution2, "SolutionItems");
            if (solutionItems == null)
                return null;
            foreach (ProjectItem item in solutionItems.ProjectItems)
            {
                if (item.Name == name)
                {
                    fileName = item.FileNames[1];
                }
            }
            if (fileName == null)
                return null;
            return File.ReadAllText(fileName);
        }

        private static DTE2 GetDte2(IServiceProvider serviceProvider)
        {
            var dte = serviceProvider.GetService(typeof (SDTE)) as DTE2;
            if (dte == null)
                throw new NullReferenceException("Error dte is null");
            return dte;
        }

        public static Project GetProject(Solution2 solution, string name)
        {
            foreach (Project item in solution.Projects)
            {
                if (item.Name == name)
                    return item;
            }
            return null;
        }

        public static string GetFile(IServiceProvider serviceProvider, string name)
        {
            var solutionService = serviceProvider.GetService(typeof(SVsSolution)) as IVsSolution;

            if (solutionService == null)
                throw new NullReferenceException("Error solutionService is null");

            string s1;
            string s2;
            string s3;
            solutionService.GetSolutionInfo(out s1, out s2, out s3);

            var files = Directory.GetFiles(s1, name, SearchOption.AllDirectories);
            if (!files.Any())
                throw new Exception(string.Format("Error could not find file {0} in solution directory", name));
            if (files.Count() > 1)
                throw new Exception(string.Format("Error more than one file was found in solution directory with name {0}", name));

            var fileName = files.First();
            return fileName;
        }



        public static ProjectItem AddProjectItem(ProjectItems projectItems, string file)
        {
            var fileInfo = new FileInfo(file);
            var fileName = fileInfo.Name;

            foreach (ProjectItem item in projectItems)
            {
                if (item.Name == fileName)
                    return item;
            }
            var newItem = projectItems.AddFromFile(file);
            if (newItem.IsOpen)
            {
                var document = newItem.Document;
                if (document != null)
                {
                    document.Close();
                }
            }
            return newItem;
        }

        //public static ProjectItem AddSolutionItem<T>(Solution2 solution, string name, T objectToJson, string directory)
        //{
        //    var json = JsonHelper.ObjectToJsonString(objectToJson);
        //    return AddSolutionItem(solution, name, json, directory);
        //}

        //public static ProjectItem AddSolutionItem(Solution2 solution, string name, string content, string directory)
        //{
        //    if(solution == null)
        //        throw new NullReferenceException("Solution Is Null");

        //    var project = AddSolutionFolder(solution, "SolutionItems");
        //    var solutionItemsFolder = directory + @"\SolutionItems";
        //    FileUtility.WriteToFile(solutionItemsFolder, name, content);
        //    return AddProjectItem(project.ProjectItems, Path.Combine(solutionItemsFolder, name));
        //}

        public static string GetProperty(EnvDTE.Properties properties, string name)
        {
            foreach (Property prop in properties)
            {
                if (prop.Name == name)
                {
                    return prop.Value == null ? null : prop.Value.ToString();
                }
            }
            return null;
        }

        public static void SetProperty(EnvDTE.Properties properties, string name, object value)
        {
            foreach (Property prop in properties)
            {
                if (prop.Name == name)
                {
                    prop.Value = value;
                    return;
                }
            }
            throw new NullReferenceException(string.Format("Could not find property {0}", name));
        }

        public static LoadToCrmResponse LoadIntoCrm(XrmRecordService service, IEnumerable<IRecord> records, string matchField)
        {
            var response = new LoadToCrmResponse();
            if (records.Any())
            {
                var type = records.First().Type;
                var matchFields =
                    records.Select(r => r.GetField(matchField))
                            .Where(f => f != null)
                            .Select(f => service.ConvertToQueryValue(matchField, type, f))
                            .ToArray();

               var matchingRecords = !matchFields.Any()
                    ? new IRecord[0]
                    : service.RetrieveAllOrClauses(type,
                    matchFields.Select(s => new Condition(matchField, ConditionType.Equal, s)))
                    .ToArray();

                foreach (var record in records)
                {

                    try
                    {
                        var matchingItems =
                            matchingRecords.Where(r => service.FieldsEqual(r.GetField(matchField), record.GetField(matchField)))
                        .ToArray();
                        if (matchingItems.Any())
                        {
                            var matchingItem = matchingItems.First();
                            record.Id = matchingItem.Id;
                            var changedFields = record
                                .GetFieldsInEntity()
                                .Where(f => !service.FieldsEqual(record.GetField(f), matchingItem.GetField(f)))
                                .ToList();

                            //added this for plugin types where workflow activity
                            //do not update the in/out arguments
                            //explicitly setting the pluginassemblyid seems to refresh them
                            if (record.Type == "plugintype"
                                && record.GetBoolField("isworkflowactivity")
                                && record.ContainsField("pluginassemblyid")
                                && !changedFields.Contains("pluginassemblyid"))
                            {
                                changedFields.Add("pluginassemblyid");
                            }

                            if (changedFields.Any())
                            {
                                service.Update(record, changedFields);
                                response.AddUpdated(record);
                            }
                        }
                        else
                        {
                            record.Id = service.Create(record, null);
                            response.AddCreated(record);
                        }
                    }
                    catch (Exception ex)
                    {
                        response.AddError(record, ex);
                    }
                }
            }

            return response;
        }

        public class LoadToCrmResponse
        {
            private List<IRecord> _updated = new List<IRecord>();
            private List<IRecord> _created = new List<IRecord>();
            private Dictionary<IRecord, Exception> _errors = new Dictionary<IRecord, Exception>();

            public void AddCreated(IRecord record)
            {
                _created.Add(record);
            }
            public void AddUpdated(IRecord record)
            {
                _updated.Add(record);
            }

            public void AddError(IRecord record, Exception ex)
            {
                _errors[record] = ex;
            }

            public IEnumerable<IRecord> Updated { get { return _updated; } }

            public IEnumerable<IRecord> Created { get { return _created; } }

            public Dictionary<IRecord, Exception> Errors { get { return _errors; } }
        }

        public static DeleteInCrmResponse DeleteInCrm(XrmRecordService service, IEnumerable<IRecord> records)
        {
            var response = new DeleteInCrmResponse();
            if (records.Any())
            {
                foreach (var record in records)
                {

                    try
                    {
                        service.Delete(record);
                        response.AddDeleted(record);
                    }
                    catch (Exception ex)
                    {
                        response.AddError(record, ex);
                    }
                }
            }

            return response;
        }

        public class DeleteInCrmResponse
        {
            private List<IRecord> _deleted = new List<IRecord>();
            private Dictionary<IRecord, Exception> _errors = new Dictionary<IRecord, Exception>();

            public void AddDeleted(IRecord record)
            {
                _deleted.Add(record);
            }

            public void AddError(IRecord record, Exception ex)
            {
                _errors[record] = ex;
            }

            public IEnumerable<IRecord> Deleted { get { return _deleted; } }

            public Dictionary<IRecord, Exception> Errors { get { return _errors; } }
        }

        public static void AddSolutionComponents(XrmRecordService xrmRecordService, XrmPackageSettings settings, int componentType, IEnumerable<IRecord> itemsToAdd)
        {
            if (settings.AddToSolution)
            {
                var solutionId = settings.Solution.Id;
                var solution = xrmRecordService.Get(Entities.solution, solutionId);

                var xrmService = xrmRecordService.XrmService;
                var currentComponentIds = xrmRecordService.RetrieveAllAndClauses(Entities.solutioncomponent, new[]
                    {
                            new Condition(Fields.solutioncomponent_.componenttype, ConditionType.Equal, componentType),
                            new Condition(Fields.solutioncomponent_.solutionid, ConditionType.Equal, solution.Id)
                        }, null)
                            .Select(r => r.GetIdField(Fields.solutioncomponent_.objectid))
                            .ToList();
                foreach (var item in itemsToAdd)
                {

                    if (!currentComponentIds.Contains(item.Id))
                    {
                        var addRequest = new AddSolutionComponentRequest()
                        {
                            AddRequiredComponents = false,
                            ComponentType = componentType,
                            ComponentId = new Guid(item.Id),
                            SolutionUniqueName = solution.GetStringField(Fields.solution_.uniquename)
                        };
                        xrmService.Execute(addRequest);
                        currentComponentIds.Add(item.Id);
                    }
                }
            }
        }

        public static void AddXrmConnectionToSolution(XrmRecordConfiguration config, IVisualStudioService visualStudioService)
        {
            var connectionFileName = "solution.xrmconnection";
            var file = visualStudioService.AddSolutionItem(connectionFileName, config);

            foreach (var item in visualStudioService.GetSolutionProjects())
            {
                if (item.Name.EndsWith(".Test"))
                {
                    var linkedConnectionItem = item.AddProjectItem(file);
                    linkedConnectionItem.SetProperty("CopyToOutputDirectory", 1);
                }
            }
        }
    }
}
