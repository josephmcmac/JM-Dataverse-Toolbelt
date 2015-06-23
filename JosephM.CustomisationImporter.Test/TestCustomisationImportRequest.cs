#region

using System.Collections.Generic;
using System.Linq;
using JosephM.Core.FieldType;
using JosephM.CustomisationImporter.Service;

#endregion

namespace JosephM.CustomisationImporter.Test
{
    public static class TestCustomisationImportRequest
    {
        public static IEnumerable<CustomisationImportRequest> GetTestRequests(string path)
        {
            var excelFiles = new[]
            {
                new FileReference(path + @"TestCustomisations.xls"),
                new FileReference(path + @"TestCustomisationsUpdate.xls")
            };

            return excelFiles
                .Select(ef => new CustomisationImportRequest
                {
                    ExcelFile = ef,
                    IncludeEntities = true,
                    IncludeFields = true,
                    IncludeRelationships = true,
                    UpdateViews = true,
                    UpdateOptionSets = true
                })
                .ToArray();
        }
    }
}