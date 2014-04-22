namespace SEToolbox.Support
{
    using log4net;
    using System;

    public static partial class DiagnosticsLogging
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #region Log

        public static void LogInfo(string message)
        {
            log.Info(message);
        }

        public static void LogException(Exception exception)
        {
            log.Fatal("Unhandled Exception", exception);
        }

        #endregion
    }
}
