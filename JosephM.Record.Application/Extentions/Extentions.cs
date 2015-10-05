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

namespace JosephM.Application.ViewModel.Extentions
{
    public static class Extentions
    {
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

        public static Uri ToPrismNavigationUriType(Type type, UriQuery uriQuery)
        {
            var prismQuery = new Microsoft.Practices.Prism.UriQuery();
            if (uriQuery != null)
            {
                foreach (var arg in uriQuery.Arguments)
                    prismQuery.Add(arg.Key, arg.Value);
            }
            var uri = new Uri(type.FullName + prismQuery, UriKind.Relative);
            return uri;
        }
    }
}
