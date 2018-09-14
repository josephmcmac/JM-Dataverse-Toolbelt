using System;
using System.Collections.Generic;
using System.Linq;
using JosephM.Core.Service;
using Microsoft.Xrm.Sdk;

namespace JosephM.Deployment.DataImport
{
    public class DataImportResponse : ServiceResponseBase<DataImportResponseItem>
    {
        private List<Entity> _createdEntities = new List<Entity>();
        private List<Entity> _updatedEntities = new List<Entity>();

        public DataImportResponse()
        {
        }

        public void AddCreated(Entity thisEntity)
        {
            _createdEntities.Add(thisEntity);
        }

        public void AddUpdated(Entity thisEntity)
        {
            _updatedEntities.Add(thisEntity);
        }

        public IEnumerable<ImportedRecords> GetImportSummary()
        {
            var results = new List<ImportedRecords>();
            var createdGroup = _createdEntities
                .GroupBy(e => e.LogicalName)
                .ToDictionary(g => g.Key, g => g.Count());
            foreach (var item in createdGroup)
                results.Add(new ImportedRecords()
                {
                    Type = item.Key,
                    Created = item.Value
                });
            var updatedGroup = _updatedEntities
                .GroupBy(e => e.LogicalName)
                .ToDictionary(g => g.Key, g => g.Count());
            foreach (var item in updatedGroup)
            {
                if(!results.Any(r => r.Type == item.Key))
                {
                    results.Add(new ImportedRecords()
                    {
                        Type = item.Key
                    });
                }
                results.First(r => r.Type == item.Key).Updated = item.Value;
            }
            return results;
        }
    }
}