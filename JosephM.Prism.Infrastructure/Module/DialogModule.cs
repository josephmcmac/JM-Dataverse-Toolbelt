using JosephM.Application;
using JosephM.Application.Modules;
using JosephM.Application.ViewModel.Dialog;
using JosephM.Core.Extentions;
using JosephM.Core.Service;
using JosephM.Prism.Infrastructure.Dialog;
using System;
using System.Linq;

namespace JosephM.Prism.Infrastructure.Module
{
    public class DialogModule<TDialog> : ModuleBase
    {
        public override void RegisterTypes()
        {
            RegisterTypeForNavigation<TDialog>();
        }

        public override void InitialiseModule()
        {
            AddOption(MainOperationName, DialogCommand);

            var genericServiceRequestDialogtype = typeof(ServiceRequestDialog<,,,>).GetGenericTypeDefinition();
            if (typeof(TDialog).BaseType.IsGenericType && typeof(TDialog).BaseType.GetGenericTypeDefinition() == genericServiceRequestDialogtype)
            {
                var requestTypes = typeof(TDialog).BaseType.GetGenericArguments().Where(t => t.IsSubclassOf(typeof(ServiceRequestBase)));
                if (requestTypes == null || !requestTypes.Any())
                    throw new NullReferenceException(string.Format("Could not find generic {0} argument for type {1}", typeof(ServiceRequestBase).Name, typeof(TDialog).Name));

                //okay want to add something for this type to save/autoload
                AddSetting("Saved " + requestTypes.First().GetDisplayName(), () =>
                    {
                        var uri = new UriQuery();
                        uri.Add("Type", requestTypes.First().AssemblyQualifiedName);
                        ApplicationController.NavigateTo(typeof(SavedRequestDialog), uri);
                    });
            }
        }

        protected virtual string MainOperationName
        {
            get { return (typeof(TDialog)).GetDisplayName(); }
        }

        public void DialogCommand()
        {
            NavigateTo<TDialog>();
        }
    }
}