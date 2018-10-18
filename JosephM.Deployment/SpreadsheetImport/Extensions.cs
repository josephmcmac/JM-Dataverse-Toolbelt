using JosephM.Core.Extentions;
using JosephM.Core.Service;
using System;
using System.Collections.Generic;
using System.Linq;

namespace JosephM.Deployment.SpreadsheetImport
{
    public static class Extensions
    {
        public static IsValidResponse Validate(this IEnumerable<IMapSpreadsheetImport> imports, bool matchByName, bool updateOnly)
        {
            var response = new IsValidResponse();
            if(imports != null)
            {
                foreach(var item in imports.GroupBy(i => i.TargetType))
                {
                    if(item.Count() > 1 && item.Any(i => i.AltMatchKeys != null && i.AltMatchKeys.Any()))
                    {
                        response.AddInvalidReason($"There Are Multiple Maps To {item.First().TargetTypeLabel}. Multiple Imports To The Same Type Are Not Supported Where {typeof(IMapSpreadsheetImport).GetProperty(nameof(IMapSpreadsheetImport.AltMatchKeys)).GetDisplayName()} Are Used For That Type");
                    }
                }
                foreach(var map in imports)
                {
                    if(map.AltMatchKeys != null)
                    {
                        foreach(var key in map.AltMatchKeys)
                        {
                            if(map.FieldMappings == null || !map.FieldMappings.Any(fm => fm.TargetField == key.TargetField))
                            {
                                response.AddInvalidReason($"{key.TargetFieldLabel} Is Not Included In The {typeof(IMapSpreadsheetImport).GetProperty(nameof(IMapSpreadsheetImport.FieldMappings)).GetDisplayName()}. All {typeof(IMapSpreadsheetImport).GetProperty(nameof(IMapSpreadsheetImport.AltMatchKeys)).GetDisplayName()} Need To Be Included In The {typeof(IMapSpreadsheetImport).GetProperty(nameof(IMapSpreadsheetImport.FieldMappings)).GetDisplayName()}");
                            }
                        }
                    }
                }
            }
            if(updateOnly)
            {
                if(!matchByName
                    && !imports.All(i => i.AltMatchKeys != null && i.AltMatchKeys.Any()))
                {
                    response.AddInvalidReason($"When Updates Only Either Match By Name Must Be Set Or All Mappings Must Have {typeof(IMapSpreadsheetImport).GetProperty(nameof(IMapSpreadsheetImport.AltMatchKeys)).GetDisplayName()} Defined");
                }
            }
            return response;
        }
    }
}
