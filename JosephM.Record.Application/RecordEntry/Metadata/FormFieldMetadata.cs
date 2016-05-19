using System;
using System.Linq;
using JosephM.Application.Application;
using JosephM.Application.ViewModel.RecordEntry.Field;
using JosephM.Application.ViewModel.RecordEntry.Form;
using JosephM.Core.Extentions;
using JosephM.Core.FieldType;
using JosephM.Record.Extentions;
using JosephM.Record.IService;
using JosephM.Record.Metadata;

namespace JosephM.Application.ViewModel.RecordEntry.Metadata
{
    public abstract class FormFieldMetadata
    {
        public virtual bool IsNonPersistentField
        {
            get { return false; }
        }

        protected FormFieldMetadata(string fieldName)
        {
            FieldName = fieldName;
            Order = int.MaxValue;
        }

        public int Order { get; set; }

        public string FieldName { get; private set; }

        public FieldViewModelBase CreateFieldViewModel(string recordType, IRecordService recordService,
            RecordEntryViewModelBase recordForm, IApplicationController applicationController)
        {
            var field = FieldName;
            try
            {
                RecordFieldType fieldType;
                string label;
                var isEditable = true;
                //this not quite right haven't needed to change yet though
                var isNonPersistent = this is NonPersistentFormField;
                var isRecordServiceField = !isNonPersistent;
                if (isNonPersistent)
                {
                    fieldType = ((NonPersistentFormField) this).RecordFieldType;
                    label = ((NonPersistentFormField) this).Label;
                    isEditable = false;
                }
                else
                {
                    fieldType = recordService.GetFieldType(field, recordType);
                    label = recordService.GetFieldLabel(field, recordType);
                    isEditable = recordService.GetFieldMetadata(field, recordType).Writeable;
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
                            ((IntegerFieldViewModel) fieldVm).MinValue =
                                Convert.ToInt32(recordService.GetFieldMetadata(field,
                                    recordType).MinValue);
                            ((IntegerFieldViewModel) fieldVm).MaxValue =
                                Convert.ToInt32(recordService.GetFieldMetadata(field,
                                    recordType).MaxValue);
                            fieldVm.IsNotNullable = recordService.GetFieldMetadata(field, recordType).IsNonNullable;
                        }
                        break;
                    }
                    case RecordFieldType.String:
                    {
                        fieldVm = new StringFieldViewModel(field, label, recordForm)
                        {
                            MaxLength = recordService.GetMaxLength(field, recordType),
                            IsRecordServiceField = isRecordServiceField,
                            IsMultiline = recordService.GetFieldMetadata(field, recordType).IsMultiline()
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
                    case RecordFieldType.Lookup:
                    {
                        //the check for null here was when loading a lookup grid the rows in the lookup do not require a form
                        var targetType = recordForm.FormService == null
                            ? null
                            : recordForm.FormService.GetLookupTargetType(field, recordType, recordForm);
                            var usePicklist = recordForm.FormService == null
                                    ? false
                                    : recordForm.FormService.UsePicklist(field, recordType);
                            fieldVm = new LookupFieldViewModel(field, label, recordForm, targetType, usePicklist)
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
                            ItemsSource =
                                recordService.GetPicklistKeyValues(field, recordType, recordForm.ParentFormReference,
                                    recordForm.GetRecord())
                                    .Select(p =>  new RecordType(p.Key, p.Value))
                                    .Where(rt => !rt.Value.IsNullOrWhiteSpace())
                                    .OrderBy(rt => rt.Value)
                                    .ToArray()
                        };
                        break;
                    }
                    case RecordFieldType.RecordField:
                    {
                        //okay need to use recordForm.ParentReference for grid rows
                        //to get the correct lookup service property
                        var dependantValue = recordForm.FormService.GetDependantValue(field, recordType, recordForm);

                        fieldVm = new RecordFieldFieldViewModel(field, label, recordForm)
                        {
                            IsRecordServiceField = isRecordServiceField,
                            ItemsSource =
                                recordService.GetPicklistKeyValues(field, recordType, recordForm.ParentFormReference == null
                                        ? dependantValue
                                        : dependantValue + ":" + recordForm.ParentFormReference, recordForm.GetRecord()).Select(r => new RecordField(r.Key, r.Value))
                                        
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
                    case RecordFieldType.BigInt:
                    {
                        fieldVm = new BigIntFieldViewModel(field, label, recordForm)
                        {
                            IsRecordServiceField = isRecordServiceField
                        };
                        if (this is PersistentFormField)
                        {
                            ((BigIntFieldViewModel) fieldVm).MinValue =
                                Convert.ToInt64(recordService.GetFieldMetadata(field,
                                    recordType).MinValue);
                            ((BigIntFieldViewModel) fieldVm).MaxValue =
                                Convert.ToInt64(recordService.GetFieldMetadata(field,
                                    recordType).MaxValue);
                            fieldVm.IsNotNullable = recordService.GetFieldMetadata(field, recordType).IsNonNullable;
                        }
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
            catch (Exception ex)
            {
                throw new Exception(String.Format("Error creating view model for field {0} in type {1}: {2}", field, recordType, ex.Message), ex);
            }
        }
    }
}