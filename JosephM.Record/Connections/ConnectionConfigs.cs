using JosephM.Core.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JosephM.Record.Connections
{
    //todo wont serialise/deserialise passwords correctly for different users
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
