using System;
using System.Collections.Generic;
using System.Linq;

namespace JosephM.Core.Attributes
{
    /// <summary>
    ///     Attribute To Define A Property As Cascading The Record Type To Another Property
    ///     Initally Used For Cacading A selected Record Type To A Record Field Or Lookup Property
    /// </summary>
    [AttributeUsage(
        AttributeTargets.Property,
        AllowMultiple = false)]
    public class AllowDownload : Attribute
    {
    }
}