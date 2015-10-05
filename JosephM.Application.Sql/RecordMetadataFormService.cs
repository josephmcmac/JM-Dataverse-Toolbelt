using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JosephM.Application.ViewModel.Grid;
using JosephM.Application.ViewModel.RecordEntry;
using JosephM.Application.ViewModel.RecordEntry.Form;
using JosephM.Application.ViewModel.RecordEntry.Metadata;
using JosephM.Record.Extentions;
using JosephM.Record.IService;
using JosephM.Record.Metadata;

namespace JosephM.Migration.Prism.Module.Sql
{
    public class RecordMetadataFormService : FormServiceBase
    {
        private IEnumerable<RecordMetadata> RecordMetadata { get; set; }

        public RecordMetadataFormService(IEnumerable<RecordMetadata> recordMetadata)
        {
            RecordMetadata = recordMetadata;
        }

        public override FormMetadata GetFormMetadata(string recordType)
        {
            var matchingRecordMetadata = RecordMetadata.Where(r => r.SchemaName == recordType);
            if(!matchingRecordMetadata.Any())
                throw new NullReferenceException(string.Format("No {0} has name of {1}", typeof(RecordMetadata).Name, recordType));

            var recordMetadata = matchingRecordMetadata.First();

            var referencingKeys = RecordMetadata
                .SelectMany(r => r.Fields)
                    .Where(f => f is LookupFieldMetadata)
                    .Cast<LookupFieldMetadata>()
                    .Where(f => f.ReferencedRecordType == recordType);

            var sections = new List<FormSection>();
            sections.Add(new FormFieldSection("Details", recordMetadata.Fields.Select(f => new PersistentFormField(f.SchemaName)).ToArray()));
            foreach (var field in referencingKeys)
            {
                var referencingTypeMetadata = RecordMetadata.First(m => m.SchemaName == field.RecordType);
                var gridFields = new List<GridFieldMetadata>();
                var view = referencingTypeMetadata.Views != null && referencingTypeMetadata.Views.Any()
                    ? referencingTypeMetadata.Views.First()
                    : null;
                if (view != null)
                {
                    gridFields.AddRange(view.Fields.Select(f => new GridFieldMetadata(f)));
                }
                else
                {
                    foreach (var referencingTypeFields in referencingTypeMetadata.Fields)
                    {
                        
                        {
                            var gridField = new GridFieldMetadata(referencingTypeFields.SchemaName);
                            gridFields.Add(gridField);
                        }
                    }
                }
                foreach (var gridField in gridFields.ToArray())
                {
                    if (gridField.FieldName == field.SchemaName || gridField.FieldName == referencingTypeMetadata.PrimaryKeyName)
                        gridFields.Remove(gridField);
                }
                var gridSection = new SubGridSection(field.RecordType, field.RecordType, field.SchemaName, gridFields);
                sections.Add(gridSection);
            }
            return new FormMetadata(sections);

        }

        public override RecordEntryFormViewModel GetEditRowViewModel(string subGridName, RecordEntryViewModelBase parentForm, Action<IRecord> onSave,
            Action onCancel, GridRowViewModel gridRow)
        {
            var viewModel =
                new OpenSqlRecordViewModel(new FormController(parentForm.RecordService,
                    parentForm.FormService, parentForm.ApplicationController), onCancel);
            var lookup = gridRow.Record.ToLookup();
            var record = parentForm.RecordService.Get(lookup.RecordType, lookup.Id);
            viewModel.SetRecord(record);
            return viewModel;
        }
    }
}
