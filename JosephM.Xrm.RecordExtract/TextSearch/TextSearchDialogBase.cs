using JosephM.Application.Desktop.Module.ServiceRequest;
using JosephM.Application.ViewModel.Dialog;
using JosephM.Record.IService;
using System.Collections.Generic;
using System.Linq;

namespace JosephM.Xrm.RecordExtract.TextSearch
{
    public abstract class TextSearchDialogBase<TTextSearchService> :
        ServiceRequestDialog<TTextSearchService, TextSearchRequest, TextSearchResponse, TextSearchResponseItem>
        where TTextSearchService : TextSearchService
    {
        protected TextSearchDialogBase(TTextSearchService service, IDialogController dialogController,
            IRecordService recordService)
            : base(service, dialogController, recordService)
        {
            SetTabLabel("Text Search");
        }

        protected override IDictionary<string, string> GetPropertiesForCompletedLog()
        {
            var dictionary = base.GetPropertiesForCompletedLog();
            void addProperty(string name, string value)
            {
                if (!dictionary.ContainsKey(name))
                    dictionary.Add(name, value);
            }
            addProperty("Strip HTML", Request.StripHtmlTagsPriorToSearch.ToString());
            addProperty("Search All Types", Request.SearchAllTypes.ToString());
            addProperty("Search Operator", Request.Operator.ToString());
            addProperty("Search Term Count", Request.SearchTerms.Count().ToString());
            addProperty("Generate Document", Request.GenerateDocument.ToString());
            addProperty("Type Exclusion Count", (Request.OtherExclusions?.Count() ?? 0).ToString());
            addProperty("Field Exclusion Count", (Request.FieldExclusions?.Count() ?? 0).ToString());
            addProperty("Custom HTML Field Count", (Request.CustomHtmlFields?.Count() ?? 0).ToString());
            if (Response.Summary != null)
            {
                foreach (var summaryItem in Response.Summary)
                {
                    addProperty($"{summaryItem.RecordTypeSchemaName}.{summaryItem.MatchedFieldSchemaName} Match Count", summaryItem.NumberOfMatches.ToString());
                }
            }
            return dictionary;
        }
    }
}