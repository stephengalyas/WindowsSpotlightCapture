using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Storage;
using Windows.Foundation.Diagnostics;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace WindowsSpotlightCapture.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Settings : Page
    {
        public Settings()
        {
            this.InitializeComponent();
            cmbLoggingLevel.ItemsSource = Enum.GetNames(typeof(LoggingLevel));
        }

        /// <summary>
        /// On page navigation, load settings.
        /// </summary>
        private void Settings_Loaded(object sender, RoutedEventArgs e)
        {
            RefreshSettings();
        }

        /// <summary>
        /// Refreshes settings controls' states.
        /// </summary>
        private void RefreshSettings()
        {
            try
            {
                ApplicationDataCompositeValue adcvLogging = ApplicationData.Current.LocalSettings.Values["logging"] as ApplicationDataCompositeValue;
                tsLoggingEnabled.IsOn = (adcvLogging["enabled"] != null ? (bool)adcvLogging["enabled"] : false);
                txtLoggingPath.Text = (adcvLogging["path"] != null ? (string)adcvLogging["path"] : String.Empty);
                if (adcvLogging["level"] != null)
                {
                    for (int i = 0; i < cmbLoggingLevel.Items.Count; i++)
                    {
                        if (((LoggingLevel)Enum.Parse(typeof(LoggingLevel), (string)cmbLoggingLevel.Items[i])) == ((LoggingLevel)Enum.Parse(typeof(LoggingLevel), (string)adcvLogging["level"])))
                        {
                            cmbLoggingLevel.SelectedItem = cmbLoggingLevel.Items[i];
                            break;
                        }
                    }
                }
                else { cmbLoggingLevel.SelectedItem = null; }
                txtLoggingPath.Text = (string)adcvLogging["loggingDir"];
                txtSaveLocationPath.Text = (string)Windows.Storage.ApplicationData.Current.LocalSettings.Values["saveDir"];
            }
            catch { }
        }

        /// <summary>
        /// User toggled "Logging - Enabled" switch.
        /// </summary>
        private void tsLoggingEnabled_Toggled(object sender, RoutedEventArgs e)
        {
            ApplicationDataCompositeValue adcvLogging = ApplicationData.Current.LocalSettings.Values["logging"] as ApplicationDataCompositeValue;
            adcvLogging["enabled"] = tsLoggingEnabled.IsOn;
            ApplicationData.Current.LocalSettings.Values["logging"] = adcvLogging;
            Objects.LogHandler.Enabled = tsLoggingEnabled.IsOn;
        }

        private void cmbLoggingLevel_DropDownClosed(object sender, object e)
        {
            ApplicationDataCompositeValue adcvLogging = ApplicationData.Current.LocalSettings.Values["logging"] as ApplicationDataCompositeValue;
            adcvLogging["level"] = cmbLoggingLevel.SelectedValue.ToString().Trim();
            ApplicationData.Current.LocalSettings.Values["logging"] = adcvLogging;
            tbLoggingLevelRestart.Visibility = Visibility.Visible;  // Display restart message.
        }

        private async void btnLoggingPath_Click(object sender, RoutedEventArgs e)
        {
            Windows.Storage.Pickers.FolderPicker fp = new Windows.Storage.Pickers.FolderPicker()
            {
                CommitButtonText = "Select",
                ViewMode = Windows.Storage.Pickers.PickerViewMode.List,
                SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary
            };
            fp.FileTypeFilter.Add("*"); // All directories.
            StorageFolder sfSelectedDir = await fp.PickSingleFolderAsync();
            if (sfSelectedDir != null)
            {
                txtLoggingPath.Text = sfSelectedDir.Path;
                ApplicationDataCompositeValue adcvLogging = ApplicationData.Current.LocalSettings.Values["logging"] as ApplicationDataCompositeValue;
                adcvLogging["loggingDir"] = txtLoggingPath.Text;
                ApplicationData.Current.LocalSettings.Values["logging"] = adcvLogging;
                Objects.LogHandler.UserDefLogDir = txtLoggingPath.Text;
            }
        }

        private async void btnSaveLocationPath_Click(object sender, RoutedEventArgs e)
        {
            Windows.Storage.Pickers.FolderPicker fp = new Windows.Storage.Pickers.FolderPicker()
            {
                CommitButtonText = "Select",
                ViewMode = Windows.Storage.Pickers.PickerViewMode.List,
                SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.PicturesLibrary
            };
            fp.FileTypeFilter.Add("*"); // All directories.
            StorageFolder sfSelectedDir = await fp.PickSingleFolderAsync();
            if (sfSelectedDir != null)
            {
                txtSaveLocationPath.Text = sfSelectedDir.Path;
                string saveDir = txtSaveLocationPath.Text;
                if (!saveDir.EndsWith(@"\")) { saveDir = saveDir + @"\"; }
                Windows.Storage.ApplicationData.Current.LocalSettings.Values["saveDir"] = saveDir;
                Objects.LogHandler.UserDefLogDir = txtSaveLocationPath.Text;
            }
        }
    }
}
