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
        internal static bool IsFirstLaunch { get { return Windows.Storage.ApplicationData.Current.LocalSettings.Values.Count == 0; } }      // If no settings have been saved, then this is the first launch.
        /// <summary>
        /// The Windows Spotlight directory.
        /// </summary>
        internal static string WindowsSpotlightDirectory { get { return Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + @"\AppData\Local\Packages\Microsoft.Windows.ContentDeliveryManager_cw5n1h2txyewy\LocalState\Assets\"; } }

        /// <summary>
        /// Initializes the program.
        /// </summary>
        /// <param name="error">(out) The error message.</param>
        /// <returns>True if successful, otherwise false.</returns>
        internal static async Task<Tuple<bool, string>> Initalize()
        {
            bool success = true;
            string error = String.Empty;

            if (!success)
            {
                return new Tuple<bool, string>(success, error);
            }

            // Set application settings defaults.
            if (Windows.Storage.ApplicationData.Current.LocalSettings.Values["installDate"] == null) { Windows.Storage.ApplicationData.Current.LocalSettings.Values["installDate"] = DateTime.Now.ToString(); }
            if (Windows.Storage.ApplicationData.Current.LocalSettings.Values["lastOpened"] == null) { Windows.Storage.ApplicationData.Current.LocalSettings.Values["lastOpened"] = DateTime.Now.ToString(); }
            if (Windows.Storage.ApplicationData.Current.LocalSettings.Values["saveDir"] == null) { Windows.Storage.ApplicationData.Current.LocalSettings.Values["saveDir"] = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + @"\Pictures\WindowsSpotlight\"; }
            ApplicationDataCompositeValue adcvLogging = Windows.Storage.ApplicationData.Current.LocalSettings.Values["logging"] == null ? new ApplicationDataCompositeValue() : ApplicationData.Current.LocalSettings.Values["logging"] as ApplicationDataCompositeValue;
            if (adcvLogging["enabled"] == null) { adcvLogging["enabled"] = false; }
            if (adcvLogging["level"] == null || String.IsNullOrWhiteSpace((string)adcvLogging["level"])) { adcvLogging["level"] = Enum.GetName(typeof(Windows.Foundation.Diagnostics.LoggingLevel), Windows.Foundation.Diagnostics.LoggingLevel.Verbose); }
            if (adcvLogging["loggingDir"] == null) { adcvLogging["loggingDir"] = ApplicationData.Current.TemporaryFolder.Path; }
            ApplicationData.Current.LocalSettings.Values["logging"] = adcvLogging;

            // Create folder in default location.
            try
            {
                if (!Directory.Exists((string)ApplicationData.Current.LocalSettings.Values["saveDir"]))
                {
                    string parentDir = string.Empty;
                    string[] splitDir = ((string)ApplicationData.Current.LocalSettings.Values["saveDir"]).Trim().Split(@"\", StringSplitOptions.RemoveEmptyEntries);
                    for (int i=0; i<Math.Max(splitDir.Length - 1, 1); i++)  // If parent directory is root of drive, length - 1 = 0 so use Math.Max to make sure loop iterates at least one..
                    {
                        parentDir += (splitDir[i] + @"\");
                    }
                    StorageFolder sfSaveLoc = await StorageFolder.GetFolderFromPathAsync(parentDir);
                    await sfSaveLoc.CreateFolderAsync(splitDir[splitDir.Length - 1], CreationCollisionOption.OpenIfExists);
                }
            }
            catch (Exception exc)
            {
                success = false;
                ApplicationData.Current.LocalSettings.Values["saveDir"] = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + @"\Pictures\WindowsSpotlight\";
                error = "We were unable to create the folder that will be used to store your selected files. The default folder will be used.\n\n" + (exc.GetType()).Name + " - " + exc.Message;
            }

            LogHandler.Initalize();
            LogHandler.Write("Finished initializing program", Windows.Foundation.Diagnostics.LoggingLevel.Verbose, System.Reflection.MethodBase.GetCurrentMethod());
            return new Tuple<bool, string>(success, error);
        }
    }
}
