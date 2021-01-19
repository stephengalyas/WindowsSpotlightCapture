using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.ServiceModel.Channels;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using WindowsSpotlightCapture.Objects;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace WindowsSpotlightCapture
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// User selected a menu item.
        /// </summary>
        private void nvMain_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs e)
        {
            if (e.SelectedItemContainer.Tag != null)
            {
                switch (e.SelectedItemContainer.Tag.ToString())
                {
                    case "WSContent":
                        {
                            // Load Windows Spotlight page.
                            svMainFrame.Navigate(typeof(Pages.WSContent), e.RecommendedNavigationTransitionInfo);
                            break;
                        }
                    case "SavedPhotos":
                        {
                            // Load Local Photos page.
                            svMainFrame.Navigate(typeof(Pages.SavedPictures), e.RecommendedNavigationTransitionInfo);
                            break;
                        }
                    case "Settings":
                        {
                            // Load Local Photos page.
                            svMainFrame.Navigate(typeof(Pages.Settings), e.RecommendedNavigationTransitionInfo);
                            break;
                        }
                }   // Close switch.
            }
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            // Must call on a separate thread. Otherwise, app hangs on splash screen.
            Task.Run(() =>
            {
                MessageDialog md;

                if (Configuration.IsFirstLaunch)
                {
                    md = new MessageDialog("This is the first time you have launched this app!", "First Launch");
                }
                else
                {
                    md = new MessageDialog("This is not the first time you have launched this app!", "Not First Launch");
                }

                IAsyncOperation<IUICommand> uiAction = md.ShowAsync();
                Task<Tuple<bool, string>> tskInitialize = Configuration.Initalize();
                tskInitialize.Wait();
                if (tskInitialize.Result.Item1 == false)
                {
                    // Error initializing application.
                    md = new MessageDialog("We could not initialize the program. Please try again.\n\nError message: " + tskInitialize.Result.Item2, "Could Not Initialize Program");
                    uiAction = md.ShowAsync();
                    Windows.ApplicationModel.Core.CoreApplication.Exit();   // Exit the app.
                }
            });
        }

        /// <summary>
        /// Handles invocation of Settings button.
        /// </summary>
        private void nvMain_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        {
            if(args.IsSettingsInvoked)
            {
                // Load Local Photos page.
                svMainFrame.Navigate(typeof(Pages.Settings));
            }
        }
    }
}
