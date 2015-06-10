using System;
using System.Linq;
using JosephM.Core.Extentions;
using JosephM.Core.FieldType;
using JosephM.Record.Application.Controller;
using JosephM.Record.Application.Grid;
using JosephM.Record.Application.RecordEntry.Field;
using JosephM.Record.Application.RecordEntry.Form;
using JosephM.Record.Application.RecordEntry.Section;
using JosephM.Record.IService;
using JosephM.Record.Metadata;

namespace JosephM.Record.Application.RecordEntry.Metadata
{
    public abstract class FormFieldMetadata
    {
        protected FormFieldMetadata(string fieldName)
        {
            FieldName = fieldName;
        }

        public string FieldName { get; private set; }

        public FieldViewModelBase CreateFieldViewModel(string recordType, IRecordService recordService,
            RecordEntryViewModelBase recordForm, IApplicationController applicationController)
        {
            var field = FieldName;
            RecordFieldType fieldType;
            string label;
            var isEditable = true;
            //todo this aint right
            var isRecordServiceField = this is PersistentFormField || this is GridFieldMetadata;
            if (this is NonPersistentFormField)
            {
                fieldType = ((NonPersistentFormField) this).RecordFieldType;
                label = ((NonPersistentFormField) this).Label;
                isEditable = false;
            }
            else
            {
                fieldType = recordService.GetFieldType(field, recordType);
                label = recordService.GetFieldLabel(field, recordType);
                isEditable = recordService.IsWritable(field, recordType);
            }
            FieldViewModelBase fieldVm = null;
            switch (fieldType)
            {
                case RecordFieldType.Boolean:
                {
                    fieldVm = new BooleanFieldViewModel(field, label, recordForm)
                    {
                        IsRecordServiceField = isRecordServiceField
                    };
                    break;
                }
                case RecordFieldType.Integer:
                {
                    fieldVm = new IntegerFieldViewModel(field, label, recordForm)
                    {
                        IsRecordServiceField = isRecordServiceField
                    };
                    if (this is PersistentFormField)
                    {
                        ((IntegerFieldViewModel) fieldVm).MinValue = recordService.GetMinIntValue(field,
                            recordType);
                        ((IntegerFieldViewModel) fieldVm).MaxValue = recordService.GetMaxIntValue(field,
                            recordType);
                        fieldVm.IsNotNullable = recordService.IsNotNullable(field, recordType);
                    }
                    break;
                }
                case RecordFieldType.String:
                {
                    fieldVm = new StringFieldViewModel(field, label, recordForm)
                    {
                        MaxLength = recordService.GetMaxLength(field, recordType),
                        IsRecordServiceField = isRecordServiceField
                    };
                    break;
                }
                    //case RecordFieldType.Enum:
                    //    {
                    //        fieldVm = new EnumFieldViewModel(field, label, recordForm)
                    //        {
                    //            ItemsSource = recordService.GetPicklistKeyValues(field, recordType),
                    //            IsRecordServiceField = isRecordServiceField
                    //        };
                    //        break;
                    //    }
                case RecordFieldType.Picklist:
                case RecordFieldType.Status:
                {
                    fieldVm = new PicklistFieldViewModel(field, label, recordForm)
                    {
                        ItemsSource = recordService.GetPicklistKeyValues(field, recordType),
                        IsRecordServiceField = isRecordServiceField
                    };
                    break;
                }
                case RecordFieldType.Date:
                {
                    fieldVm = new DateFieldViewModel(field, label, recordForm)
                    {
                        IsRecordServiceField = isRecordServiceField
                    };
                    break;
                }
                case RecordFieldType.ExcelFile:
                {
                    fieldVm = new ExcelFileFieldViewModel(field, label, recordForm)
                    {
                        IsRecordServiceField = isRecordServiceField
                    };
                    break;
                }
                case RecordFieldType.Lookup:
                {
                    //the check for null here was when loading a lookup grid the rows in the lookup do not require a form
                    var targetType = recordForm.FormService == null
                        ? null
                        : recordForm.FormService.GetLookupTargetType(field, recordType, recordForm);
                    fieldVm = new LookupFieldViewModel(field, label, recordForm, targetType
                        , recordService.LookupService)
                    {
                        IsRecordServiceField = isRecordServiceField
                    };
                    break;
                }
                case RecordFieldType.Password:
                {
                    fieldVm = new PasswordFieldViewModel(field, label, recordForm)
                    {
                        IsRecordServiceField = isRecordServiceField
                    };
                    break;
                }
                case RecordFieldType.Folder:
                {
                    fieldVm = new FolderFieldViewModel(field, label, recordForm)
                    {
                        IsRecordServiceField = isRecordServiceField
                    };
                    break;
                }
                case RecordFieldType.StringEnumerable:
                {
                    fieldVm = new StringEnumerableFieldViewModel(field, label, recordForm)
                    {
                        IsRecordServiceField = isRecordServiceField
                    };
                    break;
                }
                case RecordFieldType.RecordType:
                {
                    fieldVm = new RecordTypeFieldViewModel(field, label, recordForm)
                    {
                        IsRecordServiceField = isRecordServiceField,
                        ItemsSource = recordService.GetPicklistKeyValues(field, recordType)
                            .Select(p => new RecordType(p.Key, p.Value))
                            .Where(rt => !rt.Value.IsNullOrWhiteSpace())
                            .OrderBy(rt => rt.Value)
                            .ToArray()
                    };
                    break;
                }
                case RecordFieldType.RecordField:
                {
                    var dependantValue = recordForm.FormService.GetDependantValue(field, recordType, recordForm);

                    fieldVm = new RecordFieldFieldViewModel(field, label, recordForm)
                    {
                        IsRecordServiceField = isRecordServiceField,
                        ItemsSource = recordService.GetPicklistKeyValues(field, recordType, dependantValue)
                            .Select(p => new RecordField(p.Key, p.Value))
                            .Where(rt => !rt.Value.IsNullOrWhiteSpace())
                            .OrderBy(rt => rt.Value)
                            .ToArray()
                    };
                    break;
                }
                case RecordFieldType.Enumerable:
                {
                    fieldVm = new EnumerableFieldViewModel(field, label, recordForm)
                    {
                        IsRecordServiceField = isRecordServiceField
                    };
                    break;
                }
                case RecordFieldType.Object:
                {
                    var targetType = recordForm.FormService == null
                        ? null
                        : recordForm.FormService.GetLookupTargetType(field, recordType, recordForm);
                    fieldVm = new ObjectFieldViewModel(field, label, recordForm);
                    break;
                }
                case RecordFieldType.FileRef:
                {
                    var mask = recordForm.FormService == null
                        ? null
                        : recordForm.FormService.GetDependantValue(field, recordType, recordForm);
                    fieldVm = new FileRefFieldViewModel(field, label, recordForm, mask);
                    break;
                }
            }
            if (fieldVm == null)
                fieldVm = new UnmatchedFieldViewModel(field, label, recordForm)
                {
                    IsRecordServiceField = isRecordServiceField
                };
            fieldVm.IsEditable = isEditable;
            return fieldVm;
        }
    }
}