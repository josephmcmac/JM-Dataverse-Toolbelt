using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using EnvDTE;
using EnvDTE80;
using JosephM.Core.Extentions;
using JosephM.Core.Serialisation;
using JosephM.Record.IService;
using JosephM.Record.Query;
using JosephM.Record.Xrm.XrmRecord;
using Microsoft.VisualStudio.Shell.Interop;
using Condition = JosephM.Record.Query.Condition;

namespace JosephM.XRM.VSIX.Utilities
{
    public static class VsixUtility
    {
        public static XrmRecordConfiguration GetXrmConfig(IServiceProvider serviceProvider)
        {
            string fileName = null;
            var dte = serviceProvider.GetService(typeof(SDTE)) as DTE2;
            if(dte == null)
                throw new NullReferenceException("Error dte is null");
            var solutionItems = GetProject(dte.Solution as Solution2, "SolutionItems");
            if (solutionItems == null)
                return null;
            foreach (ProjectItem item in solutionItems.ProjectItems)
            {
                if (item.Name == "solution.xrmconnection")
                {
                    fileName = item.FileNames[1];
                }   
            }
            if(fileName == null)
                throw new NullReferenceException("Could not find solution.xrmconnection in SolutionItems folder");
            var read = File.ReadAllText(fileName);
            var dictionary =
                (Dictionary<string, string>)
                    JsonHelper.JsonStringToObject(read, typeof(Dictionary<string, string>));

            var xrmConfig = new XrmRecordConfiguration();
            foreach (var prop in xrmConfig.GetType().GetReadWriteProperties())
            {
                xrmConfig.SetPropertyByString(prop.Name, dictionary[prop.Name]);
            }
            return xrmConfig;
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

        public static Project AddSolutionFolder(Solution2 solution, string folder)
        {
            foreach (Project item in solution.Projects)
            {
                if (item.Name == folder)
                    return item;
            }
            return solution.AddSolutionFolder(folder);
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
            return newItem;
        }

        public static string GetProperty(EnvDTE.Properties properties, string name)
        {
            foreach (Property prop in properties)
            {
                if (prop.Name == name)
                {
                    return prop.Value;
                }
            }
            return null;
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
                                .ToArray();
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
    }
}
