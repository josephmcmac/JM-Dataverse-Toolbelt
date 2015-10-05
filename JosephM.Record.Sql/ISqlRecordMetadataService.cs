using JosephM.Record.IService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JosephM.Record.Extentions;
using JosephM.Record.Metadata;

namespace JosephM.Record.Sql
{
    public interface ISqlRecordMetadataService : IRecordService
    {
        IEnumerable<RecordMetadata> RecordMetadata { get; }
        void RefreshSource();
        T LoadToObject<T>(string id);
        object LoadToObject(string id, Type type);
        string SaveObject(object instance);
        string SaveTypedObject(object instance, string id, IStoredObjectFields fieldConfig);
    }
}
