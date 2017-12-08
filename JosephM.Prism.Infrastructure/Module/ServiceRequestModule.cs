using JosephM.Application.Application;
using JosephM.Application.Prism.Module;
using JosephM.Core.Extentions;
using JosephM.Core.Service;
using JosephM.Prism.Infrastructure.Dialog;
using System;
using System.Collections.Generic;
using System.Linq;

namespace JosephM.Prism.Infrastructure.Module
{
    /// <summary>
    ///     Base Class For A Module Which Plugs An Implemented Services Main Operation Into The Application
    /// </summary>
    /// <typeparam name="TService"></typeparam>
    /// <typeparam name="TRequest"></typeparam>
    /// <typeparam name="TResponse"></typeparam>
    /// <typeparam name="TDialog"></typeparam>
    /// <typeparam name="TResponseItem"></typeparam>
    public abstract class ServiceRequestModule<TDialog, TService, TRequest, TResponse, TResponseItem> : DialogModule<TDialog>, ICommandLineExecutable
        where TDialog : ServiceRequestDialog<TService, TRequest, TResponse, TResponseItem>
        where TService : ServiceBase<TRequest, TResponse, TResponseItem>
        where TRequest : ServiceRequestBase, new()
        where TResponse : ServiceResponseBase<TResponseItem>, new()
        where TResponseItem : ServiceResponseItem
    {
        public override string MainOperationName
        {
            get
            {
                var typeName = (typeof(TRequest)).GetDisplayName();
                return typeName.EndsWith(" Request") ? typeName.Substring(0, typeName.Length - 8) : typeName;
            }
        }

        string ICommandLineExecutable.CommandName { get => MainOperationName; }
        string ICommandLineExecutable.Description { get => OperationDescription; }

        void ICommandLineExecutable.Command()
        {
            DialogCommand();
        }

        IDictionary<string, Action<string>> ICommandLineExecutable.GetArgs()
        {
            return new Dictionary<string, Action<string>>
                {
                    { "Request", (s) =>
                        {
                            //load the saved object into the unity container
                            //so when the request dialog runs it loads it
                            var settingsManager = ApplicationController.ResolveType(typeof(ISettingsManager)) as ISettingsManager;
                            if(settingsManager == null)
                                throw new NullReferenceException("settingsManager");
                            var savedSettings = settingsManager.Resolve<SavedSettings>(typeof(TRequest));
                            var matchingName = savedSettings
                                .SavedRequests
                                .Where(o => (string)o.GetPropertyValue(nameof(ServiceRequestBase.Name)) == s)
                                .ToArray();
                            if(!matchingName.Any())
                            {
                                throw new NullReferenceException(string.Format("Could Not Find Saved {0} Object With {1} = '{2}'", typeof(TRequest).Name, nameof(ServiceRequestBase.Name), s));
                            }
                            ApplicationController.RegisterInstance(typeof(TRequest), matchingName.First());
                        }
                    },
                    { "LogPath", (s) => { ApplicationController.LogPath = s; } }
                };
        }
    }
}