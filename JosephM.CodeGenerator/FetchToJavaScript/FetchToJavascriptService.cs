using JosephM.Core.Log;
using JosephM.Core.Service;
using System;
using System.Collections.Generic;
using System.Linq;

namespace JosephM.CodeGenerator.FetchToJavascript
{
    public class FetchToJavascriptService : ServiceBase<FetchToJavascriptRequest, FetchToJavascriptResponse, ServiceResponseItem>
    {
        public override void ExecuteExtention(FetchToJavascriptRequest request, FetchToJavascriptResponse response,
            ServiceRequestController controller)
        {
            response.Javascript = WriteFetchToJavascript(request, controller.Controller);
        }

        private string WriteFetchToJavascript(FetchToJavascriptRequest request, LogController controller)
        {
            var stringCharacter = "\"";
            var variableName = "xml";

            var fetch = request.Fetch;
            if (fetch != null)
                fetch = fetch.Replace("\"", "\\\"");
            var splitLines = fetch
                .Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)
                .ToArray();

            var conversionList = new List<string>();
            for (var i = 0; i < splitLines.Length; i++)
            {

                if (i == 0)
                    conversionList.Add(string.Format("var {0} = {1}{2}{1};", variableName, stringCharacter, splitLines[i]));
                else
                    conversionList.Add(string.Format("{0} = {0} + {1}{2}{1};", variableName, stringCharacter, splitLines[i]));
            }
            return string.Join(Environment.NewLine, conversionList);
        }
    }
}