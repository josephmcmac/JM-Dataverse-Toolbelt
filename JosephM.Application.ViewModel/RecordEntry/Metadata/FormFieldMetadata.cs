﻿using System;
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
        public string AltRecordType { get; set; }
        public string AliasedFieldName { get; set; }

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

        public bool DoNotLimitDisplayHeight { get; set; }

        public bool AlwaysLeftAlign { get; set; }

        public int? EditableFormWidth { get; set; }

        public FieldViewModelBase CreateFieldViewModel(string recordType, IRecordService recordService,
            RecordEntryViewModelBase recordForm, IApplicationController applicationController, RecordFieldType? explicitFieldType = null, string explicitLookupTargetType = null, IEnumerable<PicklistOption> explicitPicklistOptions = null, bool explicitMultiline = false, bool explicitMultiselect = false)
        {
            var field = FieldName;
            recordType = AltRecordType ?? recordType;
            try
            {
                RecordFieldType? fieldType = explicitFieldType;

                if (!explicitFieldType.HasValue)
                    fieldType = recordService.GetFieldType(field, recordType);
                var label = recordService.GetFieldLabel(field, recordType);
                var thisFieldEditable = string.IsNullOrWhiteSpace(recordForm.GetRecord().Id) ? recordService.GetFieldMetadata(field, recordType).Createable : recordService.GetFieldMetadata(field, recordType).Writeable;
                var fixReadOnly = !thisFieldEditable;

                FieldViewModelBase fieldVm = null;
                switch (fieldType)
                {
                    case RecordFieldType.Boolean:
                    case RecordFieldType.ManagedProperty:
                        {
                            var picklist = explicitPicklistOptions ?? recordService.GetPicklistKeyValues(field, recordType);
                            fieldVm = new BooleanFieldViewModel(field, label, recordForm, picklist);
                            break;
                        }
                    case RecordFieldType.Integer:
                    {
                        var picklist = explicitPicklistOptions ?? recordService.GetPicklistKeyValues(field, recordType);
                        fieldVm = new IntegerFieldViewModel(field, label, recordForm, picklist);
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
                            IsMultiline = explicitMultiline || recordService.GetFieldMetadata(field, recordType).IsMultiline(),
                            DoNotLimitDisplayHeight = DoNotLimitDisplayHeight,
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
                            if (explicitMultiselect || fieldMetadata.IsMultiSelect)
                            {
                                fieldVm = new PicklistMultiSelectFieldViewModel(field, label, recordForm);
                                ((PicklistMultiSelectFieldViewModel)fieldVm).SetItemsSource(explicitPicklistOptions ?? recordService.GetPicklistKeyValues(field, recordType));
                            }
                            else
                            {
                                fieldVm = new PicklistFieldViewModel(field, label, recordForm)
                                {
                                    ItemsSource = explicitPicklistOptions ?? recordService.GetPicklistKeyValues(field, recordType)
                                };
                            }
                            break;
                        }
                    case RecordFieldType.Date:
                    {
                        fieldVm = new DateFieldViewModel(field, label, recordForm)
                        {
                            IncludeTime = !recordForm.IsReadOnly ||recordForm.RecordService.GetFieldMetadata(FieldName, recordType).IncludeTime
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
                            thisFieldEditable = thisFieldEditable && recordForm.FormService != null && recordForm.FormService.AllowLookupFunctions;
                            fieldVm = new LookupFieldViewModel(field, label, recordForm, targetType, usePicklist, thisFieldEditable);
                        break;
                    }
                    case RecordFieldType.Password:
                    {
                        fieldVm = new PasswordFieldViewModel(field, label, recordForm);
                        break;
                    }
                    case RecordFieldType.Folder:
                    {
                        fieldVm = new FolderFieldViewModel(field, label, recordForm);
                        break;
                    }
                    case RecordFieldType.StringEnumerable:
                    {
                        fieldVm = new StringEnumerableFieldViewModel(field, label, recordForm);
                        break;
                    }
                    case RecordFieldType.RecordType:
                    {
                        var usePicklist = recordForm.FormService == null
                                ? false
                                : recordForm.FormService.UsePicklist(field, recordType);
                        fieldVm = new RecordTypeFieldViewModel(field, label, recordForm, usePicklist);
                        try
                        {
                            ((RecordTypeFieldViewModel)fieldVm).ItemsSource =
                            explicitPicklistOptions != null
                            ? explicitPicklistOptions.Select(p => new RecordType(p.Key, p.Value)).Where(rt => !string.IsNullOrWhiteSpace(rt.Value)).OrderBy(rt => rt.Value).ToArray()
                            : recordService.GetPicklistKeyValues(field, recordType, recordForm.ParentFormReference,
                                recordForm.GetRecord())
                                .Select(p => new RecordType(p.Key, p.Value))
                                .OrderBy(rt => rt.Value)
                                .ToArray();
                        }
                        catch(Exception ex)
                        {
                            fieldVm.AddError($"Error Loading Picklist Options\n\n{ex.DisplayString()}");
                        }
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
                                fieldVm = new RecordFieldMultiSelectFieldViewModel(field, label, recordForm);
                                ((RecordFieldMultiSelectFieldViewModel)fieldVm).SetItemsSource(itemsSource);
                            }
                            else
                            {
                                fieldVm = new RecordFieldFieldViewModel(field, label, recordForm)
                                {
                                    ItemsSource = itemsSource
                                };
                            }
                            break;
                    }
                    case RecordFieldType.Enumerable:
                    {
                        //need grid fields
                        //need linked record type
                        fieldVm = new EnumerableFieldViewModel(field, label, recordForm, OtherType);
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
                        fieldVm = new BigIntFieldViewModel(field, label, recordForm);
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
                            fieldVm = new DecimalFieldViewModel(field, label, recordForm);
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
                            fieldVm = new DoubleFieldViewModel(field, label, recordForm);
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
                            fieldVm = new MoneyFieldViewModel(field, label, recordForm);
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
                            fieldVm = new UrlFieldViewModel(field, label, recordForm);
                            break;
                        }
                    case RecordFieldType.ActivityParty:
                        {
                            fieldVm = new ActivityPartyFieldViewModel(field, label, recordForm);
                            break;
                        }
                    case RecordFieldType.Uniqueidentifier:
                        {
                            fieldVm = new UniqueIdentifierFieldViewModel(field, label, recordForm);
                            break;
                        }
                }
                if (fieldVm == null)
                {
                    fieldVm = new UnmatchedFieldViewModel(field, label, recordForm);
                }
                fieldVm.IsEditable = thisFieldEditable;
                fieldVm.FixedReadOnly = fixReadOnly;
                fieldVm.DisplayLabel = DisplayLabel;
                fieldVm.AltRecordType = AltRecordType;
                fieldVm.AliasedFieldName = AliasedFieldName;
                if (EditableFormWidth.HasValue)
                {
                    fieldVm.EditableFormWidth = EditableFormWidth.Value;
                }
                if (!explicitFieldType.HasValue)
                {
                    var metadata = recordService.GetFieldMetadata(field, recordType);
                    fieldVm.Description = metadata.Description;
                }
                return fieldVm;
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Error creating view model for field {0} in type {1}: {2}", field, recordType, ex.Message), ex);
            }
        }
    }
}