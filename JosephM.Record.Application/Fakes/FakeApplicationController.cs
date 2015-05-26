#region

using System;
using JosephM.Record.Application.Controller;

#endregion

namespace JosephM.Record.Application.Fakes
{
    /// <summary>
    ///     Object for access to the main UI thread and adding or removing UI items
    /// </summary>
    public class FakeApplicationController : ApplicationControllerBase
    {
        public FakeApplicationController()
            : base("Test Script Application")
        {
            
        }

        public override void Remove(string regionName, object item)
        {
        }


        public override void AddToRegion(string regionName, object navigationErrorViewModel)
        {
        }

        public override void RequestNavigate(string regionName, Uri uri)
        {
        }

        public override void UserMessage(string message)
        {
        }


        public override bool UserConfirmation(string message)
        {
            return true;
        }

        public override void OpenRecord(string recordType, string fieldMatch, string fieldValue,
            Type maintainViewModelType)
        {
            throw new NotImplementedException();
        }

        public void OpenRecord(string entityType, string fieldMatch, string fieldValue)
        {
        }

        public override void DoOnMainThread(Action action)
        {
            action();
        }

        public override void DoOnAsyncThread(Action action)
        {
            action();
        }

        public override void ThrowException(Exception ex)
        {
            throw new ApplicationException("Unexpected Error throw By Application", ex);
        }

        public override string GetSaveFileName(string initialFileName, string extention)
        {
            throw new NotImplementedException();
        }
    }
}