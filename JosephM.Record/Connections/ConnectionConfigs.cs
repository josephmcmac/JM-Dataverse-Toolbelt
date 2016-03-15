using JosephM.Core.Attributes;
using System;
using System.Collections.Generic;

namespace JosephM.Record.Connections
{
    public class ConnectionConfigs
    {
        public void AddConfig(Type connectionType, Type serviceType)
        {
            _connectionTypes.Add(new ConnectionConfig(connectionType, serviceType));
        }

        private List<ConnectionConfig> _connectionTypes = new List<ConnectionConfig>();
        public IEnumerable<ConnectionConfig> ConnectionTypes { get { return _connectionTypes; } }
    }

    public class ConnectionConfig
    {
        [Hidden]
        public Type ConnectionType { get; set; }
        [Hidden]
        public Type ServiceType { get; set; }

        public string ConnectionTypeName
        {
            get
            {
                return ConnectionType.Name;
            }
        }

        public override string ToString()
        {
            return ConnectionType.Name;
        }

        public ConnectionConfig(Type connectionType, Type serviceType)
        {
            ConnectionType = connectionType;
            ServiceType = serviceType;
        }
    }
}
