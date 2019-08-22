using JosephM.Core.Log;
using Microsoft.Xrm.Sdk;
using System.Collections.Generic;

namespace JosephM.Deployment.ImportXml
{
    public interface IImportXmlRequest
    {
        void ClearLoadedEntities();
        IDictionary<string, Entity> GetOrLoadEntitiesForImport(LogController logController);
    }
}