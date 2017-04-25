namespace $safeprojectname$.Xrm
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
            public const bool Static = false;
            public const bool Dynamic = true;
        }

        public static class ListMemberType
        {
            public const int Contact = 2;
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

        public static class EmailStatus
        {
            public const int Draft = 1;
        }

        public static class EntitlementState
        {
            public const int Draft = 0;
            public const int Active = 1;
            public const int Cancelled = 2;
            public const int Expired = 3;
        }

        public static class OrderStateState
        {
            public const int Active = 0;
            public const int Submitted = 1;
            public const int Cancelled = 2;
            public const int Fulfilled = 3;
            public const int Invoiced = 4;
        }

        public static class List
        {
            public static class Type
            {
                public const bool Static = false;
                public const bool Dynamic = true;
            }
        }

        public static class AsynchOperationState
        {
            public const int Suspended = 1;
            public const int Completed = 3;
        }

        public static class ProductState
        {
            public const int Active = 0;
            public const int Retired = 1;
            public const int Draft = 2;
            public const int UnderRevision = 3;
        }

        public static class ProductAssociationState
        {
            public const int Active = 0;
            public const int Inactive = 1;
            public const int Draft = 2;
            public const int UnderRevision = 3;
        }

        public static class TaskState
        {
            public const int Open = 0;
            public const int Completed = 1;
            public const int Cancelled = 2;
        }

        public static class OpportunityStates
        {
            public const int Open = 0;
            public const int Won = 1;
            public const int Lost = 2;
        }

        public static class QuoteState
        {
            public const int Draft = 0;
            public const int Active = 1;
            public const int Won = 2;
            public const int Closed = 3;
        }
    }
}