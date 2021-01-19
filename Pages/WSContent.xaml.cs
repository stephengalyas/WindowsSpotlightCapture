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
using System.Drawing;
using Windows.Storage;
using WindowsSpotlightCapture.Objects;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Media.PlayTo;
using System.Threading.Tasks;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace WindowsSpotlightCapture.Pages
{

    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class WSContent : Page
    {
        /// <summary>
        /// Seek direction for cached image.
        /// </summary>
        private enum Direction
        {
            Forward,
            Backward
        }

        /// <summary>
        /// The index of the active image.
        /// </summary>
        int currIndex;
        /// <summary>
        /// A list of Windows Spotlight image URIs and their corresponding indices.
        /// </summary>
        private List<KeyValuePair<int, StorageFile>> images;
        /// <summary>
        /// The next image in the list of images.
        /// </summary>
        private Image imgNext;
        /// <summary>
        /// The previous image in the list of images.
        /// </summary>
        private Image imgPrev;

        public WSContent()
        {
            this.InitializeComponent();
            
        }

        private void WSContent_Loaded(object sender, RoutedEventArgs e)
        {
            GetImageUris();
            RefreshImages(Direction.Forward);
        }

        /// <summary>
        /// Gets a list of file paths of Windows Spotlight files.
        /// </summary>
        private async void GetImageUris()
        {
            imgPrev = new Image();
            imgNext = new Image();
            images = new List<KeyValuePair<int, StorageFile>>();
            currIndex = 0;

            // Get all Windows Spotlight items.
            StorageFolder tskFolder = await StorageFolder.GetFolderFromPathAsync(Configuration.WindowsSpotlightDirectory);
            IAsyncOperation<IReadOnlyList<IStorageItem>> allFiles = tskFolder.GetItemsAsync();
            await allFiles;
            IReadOnlyList<IStorageItem> imageFiles = allFiles.GetResults();

            if(imageFiles.Count == 0)
            {
                return;
            }

            currIndex = 0;

            // Copy each Windows Spotlight file (no extension) to the application's temporary directory, appending an image file extension to each file. If the image already exists, catch an exception and move to the next image.
            for (int i=0; i<imageFiles.Count; i++)
            {
                try
                {
                    StorageFile imgFile = await StorageFile.GetFileFromPathAsync(imageFiles[i].Path);
                    StorageFolder dirTemp = Windows.Storage.ApplicationData.Current.TemporaryFolder;
                    StorageFile imgNewFile = await dirTemp.CreateFileAsync(imgFile.Name + ".jpg", CreationCollisionOption.OpenIfExists);
                    await imgFile.CopyAndReplaceAsync(imgNewFile);
                    images.Add(new KeyValuePair<int, StorageFile>(i, imgNewFile));
                }
                catch (Exception) { }
            }
        }

        /// <summary>
        /// Refreshes the active, previous, and next images.
        /// </summary>
        /// <param name="direction">Determines if whether the next image in the sequence or the prior image in the sequence should be loaded.</param>
        private async void RefreshImages(Direction direction)
        {
            if (images.Count > 0)
            {
                int newCurrIndex = currIndex;

                if (direction == Direction.Forward)
                {
                    newCurrIndex = (currIndex + 1) % images.Count;
                    if (imgNext.Source != null) { imgViewer.Source = imgNext.Source; }
                }
                else if (direction == Direction.Backward)
                {
                    newCurrIndex = ((currIndex - 1) + images.Count) % images.Count;
                    if (imgPrev.Source != null) { imgViewer.Source = imgPrev.Source; }
                }

                // Load new previous and next images into memory. Use Dispatcher to perform these tasks asynchronously.
                Windows.UI.Core.CoreDispatcher dispatcher = Windows.UI.Core.CoreWindow.GetForCurrentThread().Dispatcher;
                await dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => LoadImage((newCurrIndex + 1) % images.Count, Direction.Forward));
                await dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => LoadImage(((newCurrIndex - 1) + images.Count) % images.Count, Direction.Backward));

                currIndex = newCurrIndex;
            }
        }

        /// <summary>
        /// Loads the next or previous image into memory.
        /// </summary>
        /// <param name="index">The insex of the image to load.</param>
        /// <param name="direction">If Direction.Forward, loads image into next image object, otherwise loads image into previous image object.</param>
        private async void LoadImage(int index, Direction direction)
        {
            BitmapImage bmpTemp = new BitmapImage(new Uri(images[index].Value.Path));
            if (direction == Direction.Forward)
            {
                // Update next image.
                imgNext.Source = bmpTemp;
            }
            else if (direction == Direction.Backward)
            {
                // Update previous image.
                imgPrev.Source = bmpTemp;
            }
        }

        /// <summary>
        /// User clicked the Previous Image button.
        /// </summary>
        private void btnPrev_Click(object sender, RoutedEventArgs e)
        {
            RefreshImages(Direction.Backward);
        }   // Close btnPrev_Click().

        /// <summary>
        /// User clicked the Next Image button.
        /// </summary>
        private void btnNext_Click(object sender, RoutedEventArgs e)
        {
            RefreshImages(Direction.Forward);
        }   // Close btnNext_Click().

        /// <summary>
        /// User clicked the Open Image button.
        /// </summary>
        private async void btnOpen_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await Windows.System.Launcher.LaunchFileAsync(images[currIndex].Value);
            }
            catch (Exception exc)
            {
                Windows.UI.Popups.MessageDialog md = new Windows.UI.Popups.MessageDialog("We could not open this image. Please try again.\n\nError message: " + exc.Message, exc.GetType().Name + " - Could Not Open Image");
                await md.ShowAsync();
            }
        }   // Close btnOpen_Click().

        /// <summary>
        /// User clicked the Save Image button.
        /// </summary>
        private async void btnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                StorageFolder tskFolder = await StorageFolder.GetFolderFromPathAsync((string)Windows.Storage.ApplicationData.Current.LocalSettings.Values["saveDir"]);
                await images[currIndex].Value.CopyAsync(tskFolder, images[currIndex].Value.Name, NameCollisionOption.ReplaceExisting);
            }
            catch (Exception exc)
            {
                Windows.UI.Popups.MessageDialog md = new Windows.UI.Popups.MessageDialog("We could not save this image. Please try again.\n\nError message: " + exc.Message, (exc.GetType()).Name + " - Could Not Save Image");
                await md.ShowAsync();
            }
        }   // Close btnSave_Click().

        /// <summary>
        /// User clicked the Email button in the Share button's flyout menu.
        /// </summary>
        private async void btnEmail_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var emailMsg = new Windows.ApplicationModel.Email.EmailMessage();
                await Windows.ApplicationModel.Email.EmailManager.ShowComposeNewEmailAsync(emailMsg);
            }
            catch (Exception exc)
            {
                Windows.UI.Popups.MessageDialog md = new Windows.UI.Popups.MessageDialog("We could not share this email via email. Please try again.\n\nError message: " + exc.Message, (exc.GetType()).Name + " - Could Not Save Image");
                await md.ShowAsync();
            }
        }   // Close btnShare_Click().
    }
}
