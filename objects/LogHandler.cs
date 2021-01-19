using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation.Diagnostics;

namespace WindowsSpotlightCapture.Objects
{
    /// <summary>
    /// Handles application logging.
    /// </summary>
    internal static class LogHandler
    {
        /// <summary>
        /// Used to enable/disable the logging feature.
        /// </summary>
        internal static bool Enabled { get; set; }
        /// <summary>
        /// The user-defined directory to store log files to be sent to the developers.
        /// </summary>
        internal static string UserDefLogDir;
        /// <summary>
        /// Tracks if the logging feature has been initialized.
        /// </summary>
        internal static bool Initialized { get; private set; }
        /// <summary>
        /// Handles saving log data to a file.
        /// </summary>
        private static LoggingSession lsMain;   // Not using FileLoggingSession as we want to queue log messgaes in memory and not write each log entry to disk.
        /// <summary>
        /// Handles writing log messages to a logging session.
        /// </summary>
        private static LoggingChannel lcMain;
        /// <summary>
        /// The file used to store logging data.
        /// </summary>
        private static string LOG_FILE { get { return "WSC_" + DateTime.Now.Year.ToString() + DateTime.Now.Month.ToString() + DateTime.Now.Day.ToString() + ".txt"; } }

        internal static bool Initalize()
        {
            lsMain = new LoggingSession("WSC_" + DateTime.Now.Year.ToString() + DateTime.Now.Month.ToString() + DateTime.Now.Day.ToString());
            lcMain = new LoggingChannel("WSC_Channel_" + DateTime.Now.Year.ToString() + DateTime.Now.Month.ToString() + DateTime.Now.Day.ToString(), new LoggingChannelOptions());
            lsMain.AddLoggingChannel(lcMain, LoggingLevel.Verbose);    // Write() method below handles whcih messages are written to the logging channel.

            // Check if logging is enabled in settings.
            Windows.Storage.ApplicationDataCompositeValue adcvLogging = Windows.Storage.ApplicationData.Current.LocalSettings.Values["logging"] as Windows.Storage.ApplicationDataCompositeValue;
            Enabled = (adcvLogging != null && adcvLogging["enabled"] != null && (bool)adcvLogging["enabled"] == true);
            UserDefLogDir = (adcvLogging["loggingDir"] != null) ? (string)adcvLogging["loggingDir"] : String.Empty;
            
            Initialized = true;
            return true;
        }

        /// <summary>
        /// When the application is getting ready to close, write logging data to file(s).
        /// </summary>
        /// <returns>An empty task.</returns>
        internal static async Task Close()
        {
            try
            {
                if (Enabled)
                {
                    // Logging is enabled.
                    Windows.Storage.StorageFolder userOutputDir = await Windows.Storage.StorageFolder.GetFolderFromPathAsync(UserDefLogDir);
                    await userOutputDir.CreateFileAsync(LOG_FILE, Windows.Storage.CreationCollisionOption.OpenIfExists);
                    await lsMain.SaveToFileAsync(userOutputDir, LOG_FILE);
                }
            }
            catch (Exception exc)
            {
                Windows.UI.Popups.MessageDialog md = new Windows.UI.Popups.MessageDialog("We could save the log file to the specified directory.\n\nError message: " + exc.Message, (exc.GetType()).Name + " - Could Not Save Log File");
                await md.ShowAsync();
            }
        }

        /// <summary>
        /// Write message to log.
        /// </summary>
        /// <param name="message">The log message.</param>
        /// <param name="level">The logging level.</param>
        /// <param name="methodInfo">Info about the method from which this function was called.</param>
        /// <param name="exc">(optional) Exception info.</param>
        internal static void Write(string message, LoggingLevel level, System.Reflection.MethodBase methodInfo = null, Exception exc = null)
        {
            if (!Enabled) { return; }    // Is logging is not enabled, immediately exit the method.

            if (level < (LoggingLevel)Enum.Parse(typeof(LoggingLevel), (string)((Windows.Storage.ApplicationData.Current.LocalSettings.Values["logging"] as Windows.Storage.ApplicationDataCompositeValue)["level"])))
            {
                return;     // Logging level is less than user-defined logging level. Do not add log entry.
            }
            StringBuilder logEntry = new StringBuilder();
            logEntry.Append(DateTime.Now.ToString()).Append(" --- ");
            if (methodInfo != null)
            {
                logEntry.Append(methodInfo.DeclaringType.Name).Append(" --- ").Append(methodInfo.Name).Append(" --- ");
            }
            if (exc != null)
            {
                logEntry.Append(exc.GetType().Name).Append(" --- ").Append(exc.Message).Append(" --- ");
            }
            logEntry.Append(message).Append(Environment.NewLine);

#if DEBUG
            // Always write to console if in debug mode.
            if (level == LoggingLevel.Critical || level == LoggingLevel.Error) { Console.Error.Write(logEntry); }
            else { Console.Write(logEntry); }
#endif

            lcMain.LogMessage(logEntry.ToString(), level);

        }

    }
}
