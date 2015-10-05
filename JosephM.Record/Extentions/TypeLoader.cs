using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JosephM.Core.AppConfig;
using JosephM.Core.Extentions;
using JosephM.Core.FieldType;
using JosephM.Record.Connections;
using JosephM.Record.IService;

namespace JosephM.Record.Extentions
{
    public static class TypeLoader
    {
        public static IRecordService LoadServiceForConnection(object connection, Type serviceType)
        {
            var connectionType = connection.GetType();
            if (!serviceType.HasConstructorFor(connectionType))
            {
                throw new NullReferenceException(
                    string.Format(
                        "The Type {0} Does Not Have A Constructor For Type {1}",
                        serviceType.Name, connectionType.Name));
            }
            return (IRecordService)serviceType.CreateFromConstructorFor(connection);
        }

        public static IRecordService LoadService(Lookup lookup, IRecordService recordService, IStoredObjectFields objectConfig, IResolveObject objectResolver)
        {
            var connection = recordService.LoadObject(lookup, objectConfig);
            var type = connection.GetType();
            var connectionConfigs = objectResolver.ResolveType(typeof(ConnectionConfigs)) as ConnectionConfigs;
            if (connectionConfigs == null)
                throw new NullReferenceException(string.Format("Error loading {0}. The resolved value is null", typeof(ConnectionConfigs).Name));
            var matchingConfig = connectionConfigs.ConnectionTypes.Where(c => type == c.ConnectionType);
            if (!matchingConfig.Any())
                throw new NullReferenceException(string.Format("Error loading {0}. No {1} has ConnectionType of {2}. Need to add the {1} to the {3} object", typeof(IRecordService).Name, typeof(ConnectionConfig).Name, type.Name, typeof(ConnectionConfigs).Name));
            return LoadServiceForConnection(connection, matchingConfig.First().ServiceType);
        }
    }
}
