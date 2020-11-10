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
        internal static bool IsFirstLaunch { get { return Windows.Storage.ApplicationData.Current.LocalSettings.Values["installInfo"] == null; } }      // If no settings have been saved, then this is the first launch.
        /// <summary>
        /// The directory in the My Pictures directory that is used to store selected Windows Spotlight photos.
        /// </summary>
        internal static StorageFolder SavedPhotosDirectory {  get { return StorageFolder.GetFolderFromPathAsync(Windows.Storage.KnownFolders.PicturesLibrary.Path + @"\WindowsSpotlight\").GetResults(); } }
        /// <summary>
        /// The directory used to store Windows Spotlight photos.
        /// </summary>
        internal static StorageFolder WindowsSpotlightDirectory { get { return StorageFolder.GetFolderFromPathAsync(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\Local\Packages\Microsoft.Windows.ContentDeliveryManager_cw5n1h2txyewy\LocalState\Assets\").GetResults(); } }
        /// <summary>
        /// Initializes the program.
        /// </summary>
        /// <param name="error">(out) The error message.</param>
        /// <returns>True if successful, otherwise false.</returns>
        internal static async Task<Tuple<bool,string>> Initalize()
        {
            bool success = true;
            string error = String.Empty;
            if (IsFirstLaunch)
            {  
                // Create folder in pictures directory.
                try
                {
                    await Windows.Storage.KnownFolders.PicturesLibrary.CreateFolderAsync("WindowsSpotlight", CreationCollisionOption.OpenIfExists);   // Tries to create the Windows Spotlight folder. If it already exists, does nothing.
                }
                catch (Exception exc)
                {
                    success = false;
                    error = "We were unable to create the folder that will be used to store your selected files\n\n" + (exc.GetType()).Name + " - " + exc.Message;
                }

                if (!success)
                {
                    return new Tuple<bool, string>(success, error);
                }

                if(success)
                {
                    // Save installation data.
                    Windows.Storage.ApplicationDataCompositeValue installInfo = new ApplicationDataCompositeValue();
                    installInfo["installed"] = true;
                    installInfo["installDate"] = DateTime.Now;
                    Windows.Storage.ApplicationData.Current.LocalSettings.Values["installInfo"] = installInfo;
                }

            }
            return new Tuple<bool, string>(success, error);
        }
        /// <summary>
        /// Creates a trace listener for this application.
        /// </summary>
        /// <param name="error">(out) If fail, the error message.</param>
        /// <returns>True if successful, otherwise false.</returns>
        //private static bool InitalizeTracer(out string error)
        //{
        //    bool success = true;
        //    bool exists = false;
        //    error = string.Empty;
        //    foreach (TraceListener tl in Trace.Listeners)
        //    {
        //        if(tl.Name == Configuration.TraceListenerName)
        //        {
        //            exists = true;
        //            break;
        //        }
        //    }
        //
        //    if (!exists)
        //    {
        //        try
        //        {
        //
        //            Trace.Listeners.Add(new TextWriterTraceListener(Configuration.TraceListenerPath, Configuration.TraceListenerName));
        //        }
        //        catch (Exception exc)
        //        {
        //            success = false;
        //            error = "We had a problem with our logging feature.\n\n" + (exc.GetType()).Name + " - " + exc.Message;
        //        }
        //    }
        //
        //    return success;
        //}   // Close InitializeTracer().

    }
}
