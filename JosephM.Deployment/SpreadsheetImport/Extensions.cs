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
                        response.AddInvalidReason($"There Are Multiple Maps To {item.Key}. Multiple Imports To The Same Type Are Not Supported Where Alt Match Keys Are Used For That Type");
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
                                response.AddInvalidReason($"The Alt Match Key {key.TargetField} Is Not Included In The Field Mappings. Alt Match Keys Need To Be Included In The Source To Target Field Mappings");
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
                    response.AddInvalidReason($"When Updates Only Either Match By Name Must Be Set Or All Mappings Must Have Alt Match Keys Defined");
                }
            }
            return response;
        }
    }
}
