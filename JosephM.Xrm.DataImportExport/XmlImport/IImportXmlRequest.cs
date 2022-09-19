using JosephM.Core.Log;
using Microsoft.Xrm.Sdk;
using System.Collections.Generic;

namespace JosephM.Xrm.DataImportExport.XmlExport
{
    public interface IImportXmlRequest
    {
        void ClearLoadedEntities();
        IDictionary<string, Entity> GetOrLoadEntitiesForImport(LogController logController);
    }
}