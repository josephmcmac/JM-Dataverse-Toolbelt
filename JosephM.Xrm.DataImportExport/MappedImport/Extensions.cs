using JosephM.Core.Extentions;
using JosephM.Core.Service;
using System;
using System.Collections.Generic;
using System.Linq;

namespace JosephM.Xrm.DataImportExport.MappedImport
{
    public static class Extensions
    {
        public static IsValidResponse Validate(this IEnumerable<IMapSourceImport> imports, bool matchByName, bool updateOnly)
        {
            var response = new IsValidResponse();
            if(imports != null)
            {
                foreach(var item in imports.GroupBy(i => i.TargetType))
                {
                    if(item.Count() > 1 && item.Any(i => i.AltMatchKeys != null && i.AltMatchKeys.Any()))
                    {
                        response.AddInvalidReason($"There Are Multiple Maps To {item.First().TargetTypeLabel}. Multiple Imports To The Same Type Are Not Supported Where {typeof(IMapSourceImport).GetProperty(nameof(IMapSourceImport.AltMatchKeys)).GetDisplayName()} Are Used For That Type");
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
                                response.AddInvalidReason($"{key.TargetFieldLabel} Is Not Included In The {typeof(IMapSourceImport).GetProperty(nameof(IMapSourceImport.FieldMappings)).GetDisplayName()}. All {typeof(IMapSourceImport).GetProperty(nameof(IMapSourceImport.AltMatchKeys)).GetDisplayName()} Need To Be Included In The {typeof(IMapSourceImport).GetProperty(nameof(IMapSourceImport.FieldMappings)).GetDisplayName()}");
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
                    response.AddInvalidReason($"When Updates Only Either Match By Name Must Be Set Or All Mappings Must Have {typeof(IMapSourceImport).GetProperty(nameof(IMapSourceImport.AltMatchKeys)).GetDisplayName()} Defined");
                }
            }
            return response;
        }
    }
}
