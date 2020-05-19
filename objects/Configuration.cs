using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Windows.Storage;
using Windows.Foundation;
using Windows.System.Diagnostics.TraceReporting;
using Windows.UI.Xaml.Documents;
using System.Reflection.Metadata.Ecma335;
using System.Diagnostics;
using Windows.Media.Streaming.Adaptive;
using System.Runtime.CompilerServices;

namespace WindowsSpotlightCapture.Objects
{
    /// <summary>
    /// A class used to store configuration data and method.
    /// </summary>
    internal static class Configuration
    {
        /// <summary>
        /// Returns true if the application data directory does not exist for this user.
        /// </summary>
        internal static bool IsFirstLaunch { get { return !Directory.Exists(AppDataDirectory); } }
        /// <summary>
        /// The application directory.
        /// </summary>
        internal static string AppDataDirectory { get { return Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\WindowsSpotlightCapture\"; } }
        /// <summary>
        /// App trace listener name.
        /// </summary>
        private static string TraceListenerName { get { return "WSPTracer"; } }
        /// <summary>
        /// Initializes the program.
        /// </summary>
        /// <param name="error">(out) The error message.</param>
        /// <returns>True if successful, otherwise false.</returns>
        internal static async Task<bool> Initalize()
        {
            bool success = true;
            string error = string.Empty;
            StorageFolder sf;
            Task<StorageFile> tskCreateFile = null;
            if (IsFirstLaunch)
            {  
                // Create application directory.
                try
                {
                    Directory.CreateDirectory(AppDataDirectory);
                }
                catch (Exception exc)
                {
                    success = false;
                    error = "We had a problem setting up this program for first use.\n\n" + (exc.GetType()).Name + " - " + exc.Message;
                }

                if(!success)
                {
                    return success;
                }

                // Create app data directory.
                sf = await StorageFolder.GetFolderFromPathAsync(Configuration.AppDataDirectory);

                // Create file asynchronously.
                tskCreateFile = sf.CreateFileAsync("asyncfile.txt").AsTask<StorageFile>();

                // Initalize tracer.
                success = InitalizeTracer(out error);

                // Wait for file to be created.
                await tskCreateFile;
            }
            return success;
        }
        /// <summary>
        /// Creates a trace listener for this application.
        /// </summary>
        /// <param name="error">(out) If fail, the error message.</param>
        /// <returns>True if successful, otherwise false.</returns>
        private static bool InitalizeTracer(out string error)
        {
            bool success = true;
            bool exists = false;
            error = string.Empty;
            foreach (TraceListener tl in Trace.Listeners)
            {
                if(tl.Name == Configuration.TraceListenerName)
                {
                    exists = true;
                    break;
                }
            }

            if (!exists)
            {
                try
                {
                    Trace.Listeners.Add(new TextWriterTraceListener(Configuration.TraceListenerName));
                }
                catch (Exception exc)
                {
                    success = false;
                    error = "We had a problem with our logging feature.\n\n" + (exc.GetType()).Name + " - " + exc.Message;
                }
            }

            return success;
        }   // Close InitializeTracer().

    }
}
