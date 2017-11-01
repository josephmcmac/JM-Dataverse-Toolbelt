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
            LogController controller)
        {
            response.Javascript = WriteFetchToJavascript(request, controller);
        }

        private string WriteFetchToJavascript(FetchToJavascriptRequest request, LogController controller)
        {
            var fetch = request.Fetch;
            var splitLines = fetch
                .Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)
                .ToArray();

            var stringCharacter = "'";
            var variableName = "fetchXml";

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