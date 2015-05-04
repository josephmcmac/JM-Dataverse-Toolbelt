namespace JosephM.Record.Xrm
{
    public static class XrmRecordOptions
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
    }
}