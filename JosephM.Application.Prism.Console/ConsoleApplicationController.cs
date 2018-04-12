using JosephM.Application.Application;
using JosephM.Application.ViewModel.Dialog;
using JosephM.Core.AppConfig;
using JosephM.Core.Log;
using System;
using System.Collections.Generic;

namespace JosephM.Application.Desktop.Console
{
    /// <summary>
    ///     Implementation Of IApplicationController For The Console Application
    /// </summary>
    public class ConsoleApplicationController : ApplicationControllerBase
    {
        public override bool RunThreadsAsynch => false;

        public IUserInterface UserInterface { get; set; }

        public ConsoleApplicationController(string applicationName, IDependencyResolver container)
            : base(applicationName, container)
        {
            UserInterface = new ConsoleUserInterface(true);
        }

        public void WriteToConsole(string message)
        {
            UserInterface.LogMessage(message);
        }


        public override void Remove(object item)
        {
            
        }

        public override IEnumerable<object> GetObjects()
        {
            throw new NotImplementedException();
        }

        public override void NavigateTo(Type type, UriQuery uriQuery = null)
        {
            var resolvedType = Container.ResolveType(type);
            //at this stage for a consol4e application this will only happen
            //for a dialog so lets begin it
            if(resolvedType is DialogViewModel)
            {
                ((DialogViewModel)resolvedType).Controller.BeginDialog();
            }
        }

        public override void UserMessage(string message)
        {
            UserInterface.LogMessage(message);
            UserInterface.LogMessage("Press Any Key To Continue");
            System.Console.ReadKey();
        }

        public override bool UserConfirmation(string message)
        {
            throw new NotImplementedException();
        }

        public override string GetSaveFileName(string initialFileName, string extention)
        {
            throw new NotImplementedException();
        }

        public override string GetSaveFolderName()
        {
            throw new NotImplementedException();
        }

        public override void ThrowException(Exception ex)
        {
            throw ex;
        }

    }
}