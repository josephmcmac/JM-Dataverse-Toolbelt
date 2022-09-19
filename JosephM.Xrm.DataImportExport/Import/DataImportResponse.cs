﻿using JosephM.Core.Attributes;
using Microsoft.Xrm.Sdk;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace JosephM.Xrm.DataImportExport.Import
{
    public class DataImportResponse : INotifyPropertyChanged
    {
        private List<ImportingRecords> _importedRecords = new List<ImportingRecords>();

        public DataImportResponse(IEnumerable<Entity> entitiesToProcess, IEnumerable<DataImportResponseItem> loadExistingErrorsIntoSummary)
        {
            var types = entitiesToProcess.Select(e => e.LogicalName).Distinct().ToArray();
            _importedRecords.AddRange(types.OrderBy(s => s).Select(s => new ImportingRecords() { Type = s, Total = entitiesToProcess.Count(e => e.LogicalName == s) }));
            if(loadExistingErrorsIntoSummary != null)
            {
                foreach(var item in loadExistingErrorsIntoSummary)
                {
                    if(item.Entity != null)
                    {
                        var summaryItem = GetImportForType(item.Entity);
                        summaryItem.AddError();
                    }
                }
            }
        }

        public IEnumerable<ImportingRecords> ImportedRecords
        {
            get
            {
                return _importedRecords;
            }
        }

        [Hidden]
        public IEnumerable<DataImportResponseItem> ResponseItems
        {
            get
            {
                return _errors;
            }
        }

        public void AddCreated(Entity thisEntity)
        {
            var importObject = GetImportForType(thisEntity.LogicalName);
            importObject.AddedCreated(thisEntity);
        }

        public ImportingRecords GetImportForType(string logicalName)
        {
            if (!_importedRecords.Any(ir => ir.Type == logicalName))
                _importedRecords.Add(new ImportingRecords() { Type = logicalName });
            return _importedRecords.First(ir => ir.Type == logicalName);
        }

        public void AddUpdated(Entity thisEntity)
        {
            var importObject = GetImportForType(thisEntity.LogicalName);
            importObject.AddedUpdated(thisEntity);
        }

        public IEnumerable<ImportedRecords> GetImportSummary()
        {
            return _importedRecords.Select(i => new ImportedRecords
            {
                Type = i.Type,
                Total = i.Total,
                Created = i.Created,
                Updated = i.Updated,
                NoChange = i.NoChange,
                Errors = i.Errors,
            }).ToArray();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        public void AddImportError(Entity entity, DataImportResponseItem dataImportResponseItem)
        {
            var importObject = GetImportForType(entity.LogicalName);
            importObject.AddError();
            _errors.Add(dataImportResponseItem);
        }

        public void AddImportError(DataImportResponseItem dataImportResponseItem)
        {
            _errors.Add(dataImportResponseItem);
        }

        private List<DataImportResponseItem> _errors = new List<DataImportResponseItem>();

        internal void AddFieldForRetry(Entity thisEntity, string field)
        {
            var importObject = GetImportForType(thisEntity.LogicalName);
            importObject.AddFieldForRetry(thisEntity, field);
        }

        internal void AddSkippedNoChange(Entity thisEntity)
        {
            var importObject = GetImportForType(thisEntity.LogicalName);
            importObject.AddSkippedNoChange(thisEntity);
        }

        internal void RemoveFieldForRetry(Entity thisEntity, string field)
        {
            var importObject = GetImportForType(thisEntity.LogicalName);
            importObject.RemoveFieldForRetry(thisEntity, field);
        }

        internal void RemoveFieldForRetry(Entity thisEntity)
        {
            var importObject = GetImportForType(thisEntity.LogicalName);
            importObject.RemoveForRetry(thisEntity);
        }
    }
}