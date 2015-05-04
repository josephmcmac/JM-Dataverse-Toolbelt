using System;
using System.Collections;
using System.Collections.Generic;
using JosephM.Prism.Infrastructure.Module;

namespace JosephM.Prism.Infrastructure.Attributes
{
    [AttributeUsage(
        AttributeTargets.Class,
        AllowMultiple = true)]
    public class DependantModuleAttribute : Attribute
    {
        public Type DependantModule { get; private set; }

        public DependantModuleAttribute(Type module)
        {
            DependantModule = module;
        }
    }
}
