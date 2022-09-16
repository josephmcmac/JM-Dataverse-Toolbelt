using JosephM.Core.Log;
using JosephM.Record.IService;
using System.Collections.Generic;

namespace JosephM.Xrm.TextSearch
{
    internal class TextSearchContainer
    {
        public TextSearchRequest Request { get; set; }
        public TextSearchResponse Response { get; set; }
        public LogController Controller { get; set; }

        private Dictionary<string, Dictionary<string, List<string>>> Matches { get; set; }

        public TextSearchContainer(TextSearchRequest request, TextSearchResponse response, LogController controller)
        {
            Request = request;
            Response = response;
            Controller = controller;
            Matches = new Dictionary<string, Dictionary<string, List<string>>>();
            Response.SetMatchDictionary(Matches);
        }

        public void AddMatchedRecord(string matchedField, IRecord record)
        {
            if (!Matches.ContainsKey(record.Type))
                Matches.Add(record.Type, new Dictionary<string, List<string>>());
            if (!Matches[record.Type].ContainsKey(matchedField))
                Matches[record.Type].Add(matchedField, new List<string>());
            Matches[record.Type][matchedField].Add(record.Id);
        }
    }
}