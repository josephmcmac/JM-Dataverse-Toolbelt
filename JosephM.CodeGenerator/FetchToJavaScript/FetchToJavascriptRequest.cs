using JosephM.Application.ViewModel.SettingTypes;
using JosephM.Core.Attributes;
using JosephM.Core.FieldType;
using JosephM.Core.Service;
using JosephM.Record.Attributes;
using JosephM.Record.Metadata;
using JosephM.Record.Query;
using System.Collections.Generic;

namespace JosephM.CodeGenerator.FetchToJavascript
{
    public class FetchToJavascriptRequest : ServiceRequestBase
    {
        [Multiline]
        [RequiredProperty]
        public string Fetch { get; set; }
    }
}