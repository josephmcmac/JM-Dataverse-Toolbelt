#region

using JosephM.Application.Application;
using JosephM.Application.Options;
using JosephM.Core.AppConfig;
using JosephM.Core.Attributes;
using System;
using System.IO;
using System.Reflection;

#endregion

namespace JosephM.Application.Modules
{
    /// <summary>
    ///     Base Class For Implementing Modules To Plug Into The Application Framework
    /// </summary>
    public abstract class ActionModuleBase : ModuleBase
    {
        public abstract string MainOperationName { get; }

        public abstract void DialogCommand();

        public string OperationDescription
        {
            get
            {
                var attribute = GetType().GetCustomAttribute<MyDescription>();
                return attribute?.Text;
            }
        }
    }
}