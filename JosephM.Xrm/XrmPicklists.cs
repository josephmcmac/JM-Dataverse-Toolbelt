namespace JosephM.Xrm
{
    public static class XrmPicklists
    {
        #region Nested type: AppointmentState

        public static class AppointmentState
        {
            public const int Open = 0;
            public const int Completed = 1;
            public const int Cancelled = 2;
            public const int Scheduled = 2;
        }

        #endregion

        #region Nested type: CaseState

        public static class CaseState
        {
            public const int Open = 0;
            public const int Completed = 1;
            public const int Cancelled = 2;
        }

        #endregion

        #region Nested type: PhoneCallState

        public static class PhoneCallState
        {
            public const int Open = 0;
            public const int Completed = 1;
            public const int Cancelled = 2;
        }

        #endregion

        #region Nested type: State

        public static class State
        {
            public const int Active = 0;
            public const int Inactive = 1;
        }

        #endregion

        #region Nested type: CaseState

        public static class ListType
        {
            public const bool Dynamic = true;
        }

        #endregion

        #region Nested type: InvoiceState

        public static class InvoiceState
        {
            public const int Active = 0;
            public const int Inactive = 2;
            public const int Cancelled = 3;
        }

        #endregion

        public static class SavedQueryType
        {
            public const int MainApplicationView = 0;
            public const int AdvancedSearch = 1;
            public const int AssociatedView = 2;
            public const int QuickFindSearch = 4;
            public const int LookupView = 64;
        }

        public static class WorkflowType
        {
            public const int Definition = 1;
            public const int Activation = 2;
            public const int Template = 3;
        }

        public static class WorkflowCategory
        {
            public const int Workflow = 0;
            public const int Dialog = 1;
            public const int BusinessRule = 2;
            public const int Action = 3;
            public const int BusinessProcessFlow = 4;
        }
    }
}