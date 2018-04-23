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
using System.Collections.Generic;

namespace JosephM.Application.ViewModel.RecordEntry.Metadata
{
    public abstract class FormFieldMetadata
    {
        public virtual bool IsNonPersistentField
        {
            get { return false; }
        }

        protected FormFieldMetadata(string fieldName, string otherType = null, bool displayLabel = true)
        {
            FieldName = fieldName;
            Order = int.MaxValue;
            OtherType = otherType;
            DisplayLabel = displayLabel;
        }

        public int Order { get; set; }

        public string FieldName { get; private set; }

        public string OtherType { get; private set; }

        public bool DisplayLabel { get; set; }

        public FieldViewModelBase CreateFieldViewModel(string recordType, IRecordService recordService,
            RecordEntryViewModelBase recordForm, IApplicationController applicationController, RecordFieldType? explicitFieldType = null, string explicitLookupTargetType = null, IEnumerable<PicklistOption> explicitPicklistOptions = null)
        {
            var field = FieldName;
            try
            {
                RecordFieldType? fieldType = explicitFieldType;
                string label;
                var isEditable = true;
                //this not quite right haven't needed to change yet though
                var isNonPersistent = this is NonPersistentFormField;
                var isRecordServiceField = !isNonPersistent;
                if (isNonPersistent)
                {
                    if (!explicitFieldType.HasValue)
                        fieldType = ((NonPersistentFormField)this).RecordFieldType;
                    label = ((NonPersistentFormField) this).Label;
                    isEditable = false;
                }
                else
                {
                    if (!explicitFieldType.HasValue)
                        fieldType = recordService.GetFieldType(field, recordType);
                    label = recordService.GetFieldLabel(field, recordType);
                    isEditable = string.IsNullOrWhiteSpace(recordForm.GetRecord().Id) ? recordService.GetFieldMetadata(field, recordType).Createable : recordService.GetFieldMetadata(field, recordType).Writeable;
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
                        if (this is PersistentFormField && !explicitFieldType.HasValue)
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
                    case RecordFieldType.Memo:
                    case RecordFieldType.String:
                    {
                        fieldVm = new StringFieldViewModel(field, label, recordForm)
                        {
                            IsRecordServiceField = isRecordServiceField,
                            IsMultiline = recordService.GetFieldMetadata(field, recordType).IsMultiline()
                        };
                        if (!explicitFieldType.HasValue)
                        {
                                ((StringFieldViewModel)fieldVm).MaxLength = recordService.GetMaxLength(field, recordType);
                        }
                        break;
                    }
                    case RecordFieldType.Picklist:
                    case RecordFieldType.Status:
                    case RecordFieldType.State:
                        {
                            var fieldMetadata = recordService.GetFieldMetadata(field, recordType);
                            if (fieldMetadata.IsMultiSelect)
                            {
                                fieldVm = new PicklistMultiSelectFieldViewModel(field, label, recordForm)
                                {
                                    IsRecordServiceField = isRecordServiceField
                                };
                                ((PicklistMultiSelectFieldViewModel)fieldVm).SetItemsSource(recordService.GetPicklistKeyValues(field, recordType));
                            }
                            else
                            {
                                fieldVm = new PicklistFieldViewModel(field, label, recordForm)
                                {
                                    ItemsSource = explicitPicklistOptions ?? recordService.GetPicklistKeyValues(field, recordType),
                                    IsRecordServiceField = isRecordServiceField
                                };
                            }
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
                    case RecordFieldType.Customer:
                    case RecordFieldType.Owner:
                        {
                            //the check for null here was when loading a lookup grid the rows in the lookup do not require a form
                            var targetType = explicitLookupTargetType;
                            if (targetType == null)
                            {
                                targetType = recordForm.FormService == null
                                ? null
                                : recordForm.FormService.GetLookupTargetType(field, recordType, recordForm);
                            }

                            var usePicklist = recordForm.FormService == null
                                    ? false
                                    : recordForm.FormService.UsePicklist(field, recordType);
                            fieldVm = new LookupFieldViewModel(field, label, recordForm, targetType, usePicklist, isEditable)
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
                            var fieldMetadata = recordService.GetFieldMetadata(field, recordType);
                            var itemsSource = recordService.GetPicklistKeyValues(field, recordType, recordForm.ParentFormReference == null
                                            ? dependantValue
                                            : dependantValue + ":" + recordForm.ParentFormReference, recordForm.GetRecord())
                                                .Select(r => new RecordField(r.Key, r.Value))
                                                .ToArray();
                            if (fieldMetadata.IsMultiSelect)
                            {
                                fieldVm = new RecordFieldMultiSelectFieldViewModel(field, label, recordForm)
                                {
                                    IsRecordServiceField = isRecordServiceField
                                };
                                ((RecordFieldMultiSelectFieldViewModel)fieldVm).SetItemsSource(itemsSource);
                            }
                            else
                            {
                                fieldVm = new RecordFieldFieldViewModel(field, label, recordForm)
                                {
                                    IsRecordServiceField = isRecordServiceField,
                                    ItemsSource = itemsSource
                                };
                            }
                            break;
                    }
                    case RecordFieldType.Enumerable:
                    {
                        //need grid fields
                        //need linked record type
                        fieldVm = new EnumerableFieldViewModel(field, label, recordForm, OtherType)
                        {
                            IsRecordServiceField = isRecordServiceField
                        };
                        break;
                    }
                    case RecordFieldType.Object:
                    {
                            //var usePicklist = recordForm.FormService == null
                            //    ? false
                            //    : recordForm.FormService.UsePicklist(field, recordType);
                            fieldVm = new ObjectFieldViewModel(field, label, recordForm, true);
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
                        if (this is PersistentFormField && !explicitFieldType.HasValue)
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
                    case RecordFieldType.Decimal:
                        {
                            fieldVm = new DecimalFieldViewModel(field, label, recordForm)
                            {
                                IsRecordServiceField = isRecordServiceField
                            };
                            if (this is PersistentFormField && !explicitFieldType.HasValue)
                            {
                                ((DecimalFieldViewModel)fieldVm).MinValue =
                                    recordService.GetFieldMetadata(field, recordType).MinValue;
                                ((DecimalFieldViewModel)fieldVm).MaxValue =
                                    recordService.GetFieldMetadata(field, recordType).MaxValue;
                                fieldVm.IsNotNullable = recordService.GetFieldMetadata(field, recordType).IsNonNullable;
                            }
                            break;
                        }
                    case RecordFieldType.Double:
                        {
                            fieldVm = new DoubleFieldViewModel(field, label, recordForm)
                            {
                                IsRecordServiceField = isRecordServiceField
                            };
                            if (this is PersistentFormField && !explicitFieldType.HasValue)
                            {
                                ((DoubleFieldViewModel)fieldVm).MinValue =
                                    Convert.ToDouble(recordService.GetFieldMetadata(field, recordType).MinValue);
                                ((DoubleFieldViewModel)fieldVm).MaxValue =
                                    Convert.ToDouble(recordService.GetFieldMetadata(field, recordType).MaxValue);
                                fieldVm.IsNotNullable = recordService.GetFieldMetadata(field, recordType).IsNonNullable;
                            }
                            break;
                        }
                    case RecordFieldType.Money:
                        {
                            fieldVm = new MoneyFieldViewModel(field, label, recordForm)
                            {
                                IsRecordServiceField = isRecordServiceField
                            };
                            if (this is PersistentFormField && !explicitFieldType.HasValue)
                            {
                                ((MoneyFieldViewModel)fieldVm).MinValue =
                                    recordService.GetFieldMetadata(field, recordType).MinValue;
                                ((MoneyFieldViewModel)fieldVm).MaxValue =
                                    recordService.GetFieldMetadata(field, recordType).MaxValue;
                                fieldVm.IsNotNullable = recordService.GetFieldMetadata(field, recordType).IsNonNullable;
                            }
                            break;
                        }
                    case RecordFieldType.Url:
                        {
                            fieldVm = new UrlFieldViewModel(field, label, recordForm)
                            {
                                IsRecordServiceField = isRecordServiceField
                            };
                            break;
                        }
                    case RecordFieldType.ActivityParty:
                        {
                            fieldVm = new ActivityPartyFieldViewModel(field, label, recordForm)
                            {
                                IsRecordServiceField = isRecordServiceField
                            };
                            break;
                        }
                }
                if (fieldVm == null)
                    fieldVm = new UnmatchedFieldViewModel(field, label, recordForm)
                    {
                        IsRecordServiceField = isRecordServiceField
                    };
                fieldVm.IsEditable = isEditable;
                fieldVm.DisplayLabel = DisplayLabel;
                if (!explicitFieldType.HasValue)
                {
                    var metadata = recordService.GetFieldMetadata(field, recordType);
                    fieldVm.Description = metadata.Description;
                }
                return fieldVm;
            }
            catch (Exception ex)
            {
                throw new Exception(String.Format("Error creating view model for field {0} in type {1}: {2}", field, recordType, ex.Message), ex);
            }
        }
    }
}