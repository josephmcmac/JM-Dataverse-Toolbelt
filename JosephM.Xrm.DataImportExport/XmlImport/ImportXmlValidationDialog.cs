using JosephM.Application.ViewModel.Dialog;
using JosephM.Application.ViewModel.Shared;
using JosephM.Core.Attributes;
using JosephM.Core.Log;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace JosephM.Xrm.DataImportExport.XmlExport
{
    public class ImportXmlValidationDialog : DialogViewModel
    {
        public ImportXmlValidationDialog(DialogViewModel parentDialog, IImportXmlRequest importRequest)
            : base(parentDialog)
        {
            ImportRequest = importRequest;
        }

        public IImportXmlRequest ImportRequest { get; }

        protected override void CompleteDialogExtention()
        {
        }

        protected override void LoadDialogExtention()
        {
            LoadingViewModel.IsLoading = false;
            IsProcessing = true;

            var progressControlViewModel = new ProgressControlViewModel(ApplicationController);
            Controller.LoadToUi(progressControlViewModel);

            var logController = new LogController(progressControlViewModel);
            var entities = ImportRequest.GetOrLoadEntitiesForImport(logController);

            Controller.RemoveFromUi(progressControlViewModel);
            IsProcessing = false;

            var indexThem = new SortedDictionary<string, SortedDictionary<Guid, List<KeyValuePair<string, Entity>>>>();
            foreach(var fileEntity in entities)
            {
                if (!indexThem.ContainsKey(fileEntity.Value.LogicalName))
                    indexThem.Add(fileEntity.Value.LogicalName, new SortedDictionary<Guid, List<KeyValuePair<string, Entity>>>());
                if (!indexThem[fileEntity.Value.LogicalName].ContainsKey(fileEntity.Value.Id))
                    indexThem[fileEntity.Value.LogicalName].Add(fileEntity.Value.Id, new List<KeyValuePair<string, Entity>>());
                indexThem[fileEntity.Value.LogicalName][fileEntity.Value.Id].Add(fileEntity);
            }
            var duplicates = new DuplicateImportXmlRecords();
            foreach(var type in indexThem.Keys)
            {
                var thisTypeDictionary = indexThem[type];
                foreach (var id in thisTypeDictionary.Keys)
                {
                    if (thisTypeDictionary[id].Count > 1)
                    {
                        duplicates.AddDuplicates(thisTypeDictionary[id]);
                    }
                }
            }
            if (duplicates.Duplicates.Any())
            {
                ImportRequest.ClearLoadedEntities();
                AddObjectToUi(duplicates, cancelAction: Controller.Close);
            }
            else
            {
                StartNextAction();
            }
        }

        [Instruction("There was an error validating XML files for import - multiple files were identified with the same record type and id. You will need to remove duplicates and rerun the process")]
        public class DuplicateImportXmlRecords
        {
            private List<DuplicateImportXmlRecord> _duplicates;

            public DuplicateImportXmlRecords()
            {
                _duplicates = new List<DuplicateImportXmlRecord>();
            }

            public IEnumerable<DuplicateImportXmlRecord> Duplicates { get { return _duplicates; } }

            public void AddDuplicates(List<KeyValuePair<string, Entity>> list)
            {
                foreach(var item in list)
                {
                    _duplicates.Add(new DuplicateImportXmlRecord(item.Key, item.Value.LogicalName, item.Value.Id.ToString()));
                }
            }

            [DoNotAllowGridOpen]
            public class DuplicateImportXmlRecord
            {
                public DuplicateImportXmlRecord(string fileName, string recordType, string recordId)
                {
                    FileName = new FileInfo(fileName).Name;
                    RecordType = recordType;
                    RecordId = recordId;
                }

                [DisplayOrder(30)]
                [GridWidth(600)]
                public string FileName { get; set; }
                [DisplayOrder(10)]
                [GridWidth(200)]
                public string RecordType { get; set; }
                [DisplayOrder(20)]
                [GridWidth(260)]
                public string RecordId { get; set; }
            }
        }
    }
}
