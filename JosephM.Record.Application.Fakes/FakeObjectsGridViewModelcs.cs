#region

using System;
using JosephM.Application.ViewModel.Grid;

#endregion

namespace JosephM.Application.ViewModel.Fakes
{
    public class FakeObjectsGridSectionViewModel : ObjectsGridSectionViewModel
    {
        public FakeObjectsGridSectionViewModel()
            : base("Fake Heading",
                new Error[]
                {
                    new Error
                    {
                        ErrorType = "Fake",
                        Exception =
                            new Exception(
                                "qqqqqqqqqqqqqqqqqq qqqqqqqqqqqqqq qqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqq\nddddddddddddd")
                    },
                    new Error
                    {
                        ErrorType = "Fake 2",
                        Exception =
                            new Exception(
                                "qqqqqqqqqqqqqqqqqq qqqqqqqqqqqqqq qqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqq")
                    },
                    new Error
                    {
                        ErrorType = "Fake 3",
                        Exception =
                            new Exception(
                                "qqqqqqqqqqqqqqqqqq qqqqqqqqqqqqqq qqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqq")
                    },
                    new Error
                    {
                        ErrorType = "Fake 4",
                        Exception =
                            new Exception(
                                "qqqqqqqqqqqqqqqqqq qqqqqqqqqqqqqq qqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqq")
                    }
                },
                new FakeApplicationController())
        {
        }

        public class Error
        {
            public string ErrorType { get; set; }
            public Exception Exception { get; set; }
        }
    }
}