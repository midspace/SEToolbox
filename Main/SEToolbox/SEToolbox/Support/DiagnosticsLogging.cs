namespace SEToolbox.Support
{
    using System.Diagnostics;

    public static partial class DiagnosticsLogging
    {
        private const string EventLogName = "Application";
        private const string EventSourceName = "SEToolbox.exe";

        #region CreateLog

        public static bool CreateLog()
        {
            return CreateLog(EventSourceName, EventLogName);
        }

        public static bool CreateLog(string source, string log)
        {
            try
            {
                if (!EventLog.SourceExists(source))
                {
                    EventLog.CreateEventSource(source, log);
                    return true;
                }

                // Log already exists, means its okay to start using it.
                return true;
            }
            catch
            {
                return false;
            }
        }

        #endregion

        #region RemoveLog

        public static bool RemoveLog()
        {
            return RemoveLog(EventSourceName);
        }

        public static bool RemoveLog(string source)
        {
            try
            {
                if (EventLog.SourceExists(source))
                {
                    EventLog.DeleteEventSource(source);
                }

                // Log has been remove, or already removed.
                return true;
            }
            catch
            {
                // Could not access log to remove it.
                return false;
            }
        }

        #endregion

        #region LoggingSourceExists

        public static bool LoggingSourceExists()
        {
            try
            {
                return EventLog.SourceExists(EventSourceName);
            }
            catch
            {
                return false;
            }
        }

        #endregion
    }
}
