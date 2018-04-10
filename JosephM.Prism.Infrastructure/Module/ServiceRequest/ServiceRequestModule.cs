using JosephM.Application.Application;
using JosephM.Application.Prism.Module.CommandLine;
using JosephM.Application.Prism.Module.Dialog;
using JosephM.Core.Extentions;
using JosephM.Core.Service;
using System;
using System.Collections.Generic;
using System.Linq;

namespace JosephM.Application.Prism.Module.ServiceRequest
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

        Type ICommandLineExecutable.RequestType { get => typeof(TRequest); }

        void ICommandLineExecutable.Command()
        {
            DialogCommand();
        }

        IEnumerable<CommandLineArgument> ICommandLineExecutable.GetArgs()
        {
            return new CommandLineArgument[]
                {
                    new CommandLineArgument("Request", "name Of The Saved Reequiest To Process", (s) =>
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
                        }),
                    new CommandLineArgument("LogPath", "Path To Output Any Logs Into", (s) => { ApplicationController.LogPath = s; })
                };
        }
    }
}