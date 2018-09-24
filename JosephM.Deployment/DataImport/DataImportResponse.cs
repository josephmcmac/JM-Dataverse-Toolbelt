using System;
using System.Collections.Generic;
using System.Linq;
using JosephM.Core.Service;
using Microsoft.Xrm.Sdk;

namespace JosephM.Deployment.DataImport
{
    public class DataImportResponse : ServiceResponseBase<DataImportResponseItem>
    {
        private Dictionary<string, Dictionary<Guid, Entity>> _createdEntities = new Dictionary<string, Dictionary<Guid, Entity>>();
        private Dictionary<string, Dictionary<Guid, Entity>> _updatedEntities = new Dictionary<string, Dictionary<Guid, Entity>>();

        public DataImportResponse()
        {
        }

        public void AddCreated(Entity thisEntity)
        {
            if (!_createdEntities.ContainsKey(thisEntity.LogicalName))
                _createdEntities.Add(thisEntity.LogicalName, new Dictionary<Guid, Entity>());
            if (!_createdEntities[thisEntity.LogicalName].ContainsKey(thisEntity.Id))
                _createdEntities[thisEntity.LogicalName].Add(thisEntity.Id, thisEntity);
        }

        public void AddUpdated(Entity thisEntity)
        {
            if (_createdEntities.ContainsKey(thisEntity.LogicalName)
                && _createdEntities[thisEntity.LogicalName].ContainsKey(thisEntity.Id))
            {
                //already added as created
                return;
            }
            if (!_updatedEntities.ContainsKey(thisEntity.LogicalName))
                _updatedEntities.Add(thisEntity.LogicalName, new Dictionary<Guid, Entity>());
            if (!_updatedEntities[thisEntity.LogicalName].ContainsKey(thisEntity.Id))
                _updatedEntities[thisEntity.LogicalName].Add(thisEntity.Id, thisEntity);
        }

        public IEnumerable<ImportedRecords> GetImportSummary()
        {
            var results = new List<ImportedRecords>();
            foreach (var item in _createdEntities)
                results.Add(new ImportedRecords()
                {
                    Type = item.Key,
                    Created = item.Value.Count
                });
            foreach (var item in _updatedEntities)
            {
                if(!results.Any(r => r.Type == item.Key))
                {
                    results.Add(new ImportedRecords()
                    {
                        Type = item.Key,
                    });
                }
                results.First(r => r.Type == item.Key).Updated = item.Value.Count;
            }
            return results;
        }
    }
}