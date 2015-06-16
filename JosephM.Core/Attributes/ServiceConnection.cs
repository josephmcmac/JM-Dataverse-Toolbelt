using System;
using System.Collections.Generic;
using System.Linq;

namespace JosephM.Core.Attributes
{
    /// <summary>
    ///     Attribute To Define A Service Connection Object. Time Required To Have Constructor For Type With Attribute
    /// </summary>
    public class ServiceConnection : Attribute
    {
        public Type ServiceType { get; private set; }

        public ServiceConnection(Type serviceType)
        {
            ServiceType = serviceType;
        }
    }
}