using JosephM.Application.Modules;
using JosephM.Application.ViewModel.Grid;
using JosephM.Application.ViewModel.Navigation;
using JosephM.Application.ViewModel.RecordEntry.Form;
using JosephM.Application.ViewModel.RecordEntry.Metadata;
using JosephM.Core.Extentions;
using JosephM.Core.AppConfig;
using JosephM.Record.Extentions;
using JosephM.Record.IService;
using JosephM.Record.Metadata;
using JosephM.Record.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using JosephM.Application.Application;
using JosephM.Application.ViewModel.Shared;

namespace JosephM.Application.ViewModel.Extentions
{
    public static class Extentions
    {
        public static void AddNavigationEvent(this ModuleBase module, Action<object> action)
        {
            //okay this one is autmatically created by the unity container 
            //but iteratively add and resolve 2 items and verify they are retained in the resolved list
            var events = module.ApplicationController.ResolveType<NavigationEvents>();
            events.AddEvent(action);
            module.ApplicationController.RegisterInstance(typeof(NavigationEvents), events);
        }

        public static void AddCustomFormFunction(this ModuleBase module, CustomFormFunction customFormFunction, Type type)
        {
            //okay this one is autmatically created by the unity container 
            //but iteratively add and resolve 2 items and verify they are retained in the resolved list
            var customFormFunctions = (CustomFormFunctions)module.ApplicationController.ResolveInstance(typeof(CustomFormFunctions), type.AssemblyQualifiedName);
            customFormFunctions.AddFunction(customFormFunction);
            module.ApplicationController.RegisterInstance(typeof(CustomFormFunctions), type.AssemblyQualifiedName, customFormFunctions);
        }
        public static void AddCustomGridFunction(this ModuleBase module, CustomGridFunction customGridFunction, Type type)
        {
            //okay this one is autmatically created by the unity container 
            //but iteratively add and resolve 2 items and verify they are retained in the resolved list
            var customGridFunctions = (CustomGridFunctions)module.ApplicationController.ResolveInstance(typeof(CustomGridFunctions), type.AssemblyQualifiedName);
            customGridFunctions.AddFunction(customGridFunction);
            module.ApplicationController.RegisterInstance(typeof(CustomGridFunctions), type.AssemblyQualifiedName, customGridFunctions);
        }

        public static void AddOnChangeFunction(this ModuleBase module, OnChangeFunction onChangeFunction, Type type)
        {
            //okay this one is autmatically created by the unity container 
            //but iteratively add and resolve 2 items and verify they are retained in the resolved list
            var customFunctions = (OnChangeFunctions)module.ApplicationController.ResolveInstance(typeof(OnChangeFunctions), type.AssemblyQualifiedName);
            customFunctions.AddFunction(onChangeFunction);
            module.ApplicationController.RegisterInstance(typeof(OnChangeFunctions), type.AssemblyQualifiedName, customFunctions);
        }

        public static void AddAutocompleteFunction(this ModuleBase module, AutocompleteFunction autocompleteFunction, Type type, string property)
        {
            var functions = (AutocompleteFunctions)module.ApplicationController.ResolveInstance(typeof(AutocompleteFunctions), type.AssemblyQualifiedName);
            functions.AddAutocompleteFunction(property, autocompleteFunction);
            module.ApplicationController.RegisterInstance(typeof(AutocompleteFunctions), type.AssemblyQualifiedName, functions);
        }

        public static IEnumerable<GridFieldMetadata> GetGridFields(this IRecordService recordService, string recordType,
            ViewType preferredViewType)
        {
            var view = GetView(recordService, recordType, preferredViewType);
            return view
                .Fields
                .Select(f => new GridFieldMetadata(f))
                .ToArray();
        }

        public static ViewMetadata GetView(this IRecordService recordService, string recordType, ViewType preferredViewType)
        {
            var savedViews = recordService.GetViews(recordType);
            if (savedViews != null)
            {
                var matchingViews = savedViews.Where(v => v.ViewType == preferredViewType);
                if (matchingViews.Any())
                    return matchingViews.First();
                if (savedViews.Any())
                    return savedViews.First();
            }
            if (preferredViewType == ViewType.LookupView)
            {
                var primaryField = recordService.GetPrimaryField(recordType);
                if (primaryField.IsNullOrWhiteSpace())
                    throw new NullReferenceException(string.Format("No primary field defined for type {0}", recordType));
                return new ViewMetadata(new[] { new ViewField(recordService.GetPrimaryField(recordType), 10, 200) });
            }
            var fields = recordService.GetFields(recordType).Select(f => new ViewField(f, 1, 200));
            return new ViewMetadata(fields.Take(10));
        }

        public static IEnumerable<string> GetStringQuickfindFields(this IRecordService recordService, string recordType)
        {
            var results = new List<String>();
            results.AddRange(recordService.GetQuickfindFields(recordType).Where(f => recordService.IsString(f, recordType)));
            if (!results.Any())
                results.Add(recordService.GetPrimaryField(recordType));
            return results;
        }

        public static GetGridRecordsResponse GetGridRecordPage(this DynamicGridViewModel gridViewModel, IEnumerable<Condition> conditions, IEnumerable<SortExpression> sorts)
        {
            var sortList = sorts == null ? new List<SortExpression>() : sorts.ToList();
            if (gridViewModel.GetLastSortExpression() != null)
                sortList.Insert(0, gridViewModel.GetLastSortExpression());
            var records = gridViewModel.RecordService.GetFirstX(gridViewModel.RecordType,
                gridViewModel.CurrentPageCeiling + 1, null, conditions, sortList);
            var hasMoreRows = records.Count() > gridViewModel.CurrentPageCeiling;
            records = records.Skip(gridViewModel.CurrentPageFloor).Take(gridViewModel.PageSize).ToArray();
            return new GetGridRecordsResponse(records, hasMoreRows);
        }

        public static GetGridRecordsResponse GetGridRecordPage(this DynamicGridViewModel gridViewModel, QueryDefinition query)
        {
            query.Top = gridViewModel.CurrentPageCeiling + 1;
            if (gridViewModel.GetLastSortExpression() != null)
                query.Sorts.Insert(0, gridViewModel.GetLastSortExpression());
            var records = gridViewModel.RecordService.RetreiveAll(query);
            records.PopulateEmptyLookups(gridViewModel.RecordService, null);
            var hasMoreRows = records.Count() > gridViewModel.CurrentPageCeiling;
            records = records.Skip(gridViewModel.CurrentPageFloor).Take(gridViewModel.PageSize).ToArray();
            return new GetGridRecordsResponse(records, hasMoreRows);
        }

        public static GetGridRecordsResponse GetGridRecord(this DynamicGridViewModel gridViewModel, IEnumerable<IRecord> allRecords, bool ignorePaging)
        {
            var hasMoreRows = allRecords.Count() > gridViewModel.CurrentPageCeiling;
            var sortExpression = gridViewModel.GetLastSortExpression();
            if (sortExpression != null)
            {
                var sortField = sortExpression.FieldName;
                var newList = new List<IRecord>();
                foreach (var item in allRecords)
                {
                    var value1 = item.GetField(sortField);
                    if (value1 == null)
                    {
                        newList.Insert(0, item);
                        continue;
                    }
                    foreach (var item2 in newList)
                    {
                        var value2 = item2.GetField(sortField);
                        if (value2 == null)
                        {
                            continue;
                        }
                        else if (!(value1 is Enum) && value1 is IComparable)
                        {
                            if (((IComparable)value1).CompareTo(value2) < 0)
                            {
                                newList.Insert(newList.IndexOf(item2), item);
                                break;
                            }
                            else
                                continue;
                        }
                        var sortString1 = value1.ToString();
                        var sortString2 = value2.ToString();
                        if (value1 is Enum)
                            sortString1 = ((Enum)value1).GetDisplayString();
                        if (value2 is Enum)
                            sortString2 = ((Enum)value2).GetDisplayString();
                        if (String.Compare(sortString1, sortString2, StringComparison.Ordinal) < 0)
                        {
                            newList.Insert(newList.IndexOf(item2), item);
                            break;
                        }
                    }
                    if (!newList.Contains(item))
                        newList.Add(item);
                }
                if (sortExpression.SortType == SortType.Descending)
                    newList.Reverse();
                allRecords = newList;
            }
            if (!ignorePaging)
                allRecords = allRecords.Skip(gridViewModel.CurrentPageFloor).Take(gridViewModel.PageSize).ToArray();
            return new GetGridRecordsResponse(allRecords, hasMoreRows);
        }

        public static HorizontalJustify GetHorizontalJustify(this RecordFieldType fieldType, bool isReadonly)
        {
            if (new[] { RecordFieldType.Boolean, RecordFieldType.Url }.Contains(fieldType))
            {
                return HorizontalJustify.Middle;
            }
            else if (isReadonly && new[] { RecordFieldType.ManagedProperty, RecordFieldType.Object, RecordFieldType.BigInt, RecordFieldType.Password, RecordFieldType.Uniqueidentifier, RecordFieldType.Status, RecordFieldType.State, RecordFieldType.Owner, RecordFieldType.Customer, RecordFieldType.Lookup, RecordFieldType.Date, RecordFieldType.RecordType, RecordFieldType.RecordField, RecordFieldType.Picklist, RecordFieldType.Integer, RecordFieldType.Decimal, RecordFieldType.Double, RecordFieldType.Money }.Contains(fieldType))
            {
                return HorizontalJustify.Middle;
            }
            else
            {
                return HorizontalJustify.Left;
            }
        }
    }
}
