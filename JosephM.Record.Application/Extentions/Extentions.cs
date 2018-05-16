using System;
using System.Collections.Generic;
using System.Linq;
using JosephM.Application.ViewModel.Grid;
using JosephM.Application.ViewModel.RecordEntry.Metadata;
using JosephM.Core.Extentions;
using JosephM.Record.Extentions;
using JosephM.Record.IService;
using JosephM.Record.Metadata;
using JosephM.Record.Query;
using JosephM.Core.Service;
using JosephM.Application.ViewModel.TabArea;
using JosephM.Application.Application;
using JosephM.ObjectMapping;
using JosephM.Core.AppConfig;
using JosephM.Record.Service;
using JosephM.Application.ViewModel.RecordEntry;
using JosephM.Application.ViewModel.RecordEntry.Form;
using JosephM.Application.Modules;

namespace JosephM.Application.ViewModel.Extentions
{
    public static class Extentions
    {
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

        public static IEnumerable<GridFieldMetadata> GetGridFields(this IRecordService recordService, string recordType,
            ViewType preferredViewType)
        {
            var view = GetView(recordService, recordType, preferredViewType);
            return view
                .Fields
                .Select(f => new GridFieldMetadata(f))
                .ToArray();
        }

        public static ViewMetadata GetView(this IRecordService recordService, string recordType,  ViewType preferredViewType)
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
            return new ViewMetadata(recordService.GetFields(recordType).Select(f => new ViewField(f, 1, 200)));
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
    }
}
