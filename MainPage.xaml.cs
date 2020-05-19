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
            MessageDialog md;
            
            if (Configuration.IsFirstLaunch)
            {
                md = new MessageDialog("This is the first time you have launched this app!", "First Launch");
            }
            else
            {
                md = new MessageDialog("This is not the first time you have launched this app!", "Not First Launch");
            }

            Task<bool> tskInitialize = Configuration.Initalize();
            IAsyncOperation<IUICommand> uiAction = md.ShowAsync();
            
        }
    }
}
