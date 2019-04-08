using System;

namespace JosephM.Application.ViewModel.Fakes
{
    public class FakeUserMessageException : Exception
    {
        public FakeUserMessageException(Exception ex)
            : base("Fake User Message", ex)
        {

        }
    }
}
