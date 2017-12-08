#region

using JosephM.Application;
using JosephM.Application.Application;
using JosephM.Application.ViewModel.Dialog;
using JosephM.Application.ViewModel.HTML;
using JosephM.Application.ViewModel.Navigation;
using JosephM.Core.AppConfig;
using JosephM.Core.Extentions;
using JosephM.Core.Log;
using Microsoft.Practices.Prism.Regions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using Extentions = JosephM.Application.ViewModel.Extentions.Extentions;
using MessageBox = System.Windows.MessageBox;

#endregion

namespace JosephM.Prism.Infrastructure.Console
{
    /// <summary>
    ///     Implementation Of IApplicationController For The Prism Application
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


        public override void Remove(string regionName, object item)
        {
            
        }

        public override IEnumerable<object> GetObjects(string regionName)
        {
            throw new NotImplementedException();
        }

        public override void RequestNavigate(string regionName, Type type, UriQuery uriQuery)
        {
            var resolvedType = Container.ResolveType(type);
            //at this stage for a consol4e application this will only happen
            //for a dialog so lets begin it
            if(resolvedType is DialogViewModel)
            {
                ((DialogViewModel)resolvedType).Controller.BeginDialog();
            }
        }

        private void ProcessNavigationResult(NavigationResult navigationResult)
        {
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

        public override void OpenRecord(string recordType, string fieldMatch, string fieldValue,
            Type maintainViewModelType)
        {
        }

        public override void NavigateTo(Type type, UriQuery uriQuery)
        {
        }

        public override string GetSaveFileName(string initialFileName, string extention)
        {
            throw new NotImplementedException();
        }

        public override string GetSaveFolderName()
        {
            throw new NotImplementedException();
        }

        public override void SeralializeObjectToFile(object theObject, string fileName)
        {
        }


        public override void OpenHelp(string fileName)
        {
        }

        public override void ThrowException(Exception ex)
        {
            throw ex;
        }

    }
}