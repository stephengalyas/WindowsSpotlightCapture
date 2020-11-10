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
        /// <summary>
        /// Used to track the progress of loading the next image.
        /// </summary>
        private Task tskLoadNextImage;
        /// <summary>
        /// Used to track the progress of loading the previous image.
        /// </summary>
        private Task tskLoadPrevImage;
        ///// <summary>
        ///// Used to store the current active in the list.
        ///// </summary>
        //private KeyValuePair<int, Image> activeImage;
        ///// <summary>
        ///// Used to store the previous image in the list.
        ///// </summary>
        //private KeyValuePair<int, Image> prevImage;
        ///// <summary>
        ///// Used to store the next image in the list.
        ///// </summary>
        //private KeyValuePair<int, Image> nextImage;

        public WSContent()
        {
            this.InitializeComponent();
            imgPrev = imgNext = null;
            images = new List<KeyValuePair<int, StorageFile>>();
            tskLoadNextImage = tskLoadPrevImage = null;
            currIndex = -1;

            GetImageUris();
            RefreshImages(Direction.Forward);
        }

        /// <summary>
        /// Gets a list of file paths of Windows Spotlight files.
        /// </summary>
        /// <returns></returns>
        private async void GetImageUris()
        {
            IAsyncOperation<IReadOnlyList<IStorageItem>> allFiles = Configuration.WindowsSpotlightDirectory.GetItemsAsync();

            // While the system is retrieving files, reset the controls on the form.
            imgViewer = null;

            await allFiles;
            IReadOnlyList<IStorageItem> imageFiles = allFiles.GetResults();

            if(imageFiles.Count == 0)
            {
                return;
            }

            currIndex = -1;
            for (int i=0; i<imageFiles.Count; i++)
            {
                images.Add(new KeyValuePair<int, StorageFile>(i, StorageFile.GetFileFromPathAsync(imageFiles[i].Path).GetResults()));
            }
        }

        /// <summary>
        /// Refreshes the active, previous, and next images.
        /// </summary>
        /// <param name="direction"></param>
        private void RefreshImages(Direction direction)
        {
            if (images.Count > 0)
            {
                int newCurrIndex = currIndex;

                // If previous and next images are still being loaded into memory on their threads, wait for these processes to finish.
                if (tskLoadNextImage != null && !tskLoadNextImage.IsCompleted)
                {
                    tskLoadNextImage.Wait();
                }
                if (tskLoadPrevImage != null && !tskLoadPrevImage.IsCompleted)
                {
                    tskLoadPrevImage.Wait();
                }

                if (direction == Direction.Forward)
                {
                    newCurrIndex = (currIndex + 1) % images.Count;
                    imgViewer = imgNext;
                }
                else if (direction == Direction.Backward)
                {
                    newCurrIndex = ((currIndex - 1) + images.Count) % images.Count;
                    imgViewer = imgPrev;
                }

                // Load new previous and next images on separate threads.
                tskLoadNextImage = Task.Run(() => { LoadImage((newCurrIndex + 1) % images.Count, Direction.Forward); });
                tskLoadPrevImage = Task.Run(() => { LoadImage(((newCurrIndex - 1) + images.Count) % images.Count, Direction.Backward); });

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
            using (IRandomAccessStream strImage = await images[index].Value.OpenAsync(FileAccessMode.Read, StorageOpenOptions.AllowReadersAndWriters))
            {
                BitmapImage bmpTemp = new BitmapImage();
                bmpTemp.SetSource(strImage);
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
        }

        /*
        /// <summary>
        /// Loads the next image in a direction into a KeyValuePair that stores the Image object and its corresponding index in the list of image files available. 
        /// </summary>
        /// <param name="img"></param>
        /// <param name="currIndex"></param>
        private void LoadImage(ref KeyValuePair<int,Image> img, int currIndex, Direction direction)
        {
            int imagesChecked = 0;
            int totalFiles = imageFiles.Count;  // Ensures that the loop will eventually exit.
            while (imagesChecked < totalFiles && img.Value.Source == null)
            {
                imagesChecked++;    // Allows the loop to exit if we have iterated through all images and cannot find a next valid image.
                BitmapImage bmp = new BitmapImage();
                bmp.ImageFailed += (s, e) => (s as BitmapImage).UriSource = null;   // Not a valid image? Set source equal to null.
                bmp.UriSource = new Uri(imageFiles[currIndex].Path);
                // If ImageFailed event is fired off, move to the next image.
                if (bmp.UriSource == null)
                {
                    // Move to next image in forward direction.
                    if (direction == Direction.Forward)
                    {
                        currIndex = (currIndex + 1) % totalFiles;   // Circular forward loop.
                    }
                    else if (direction == Direction.Backward)
                    {
                        currIndex = ((currIndex - 1) + totalFiles) % totalFiles;   // Circular backward loop.
                    }
                }
                else
                {
                    // Valid image. Load it into the Image file.
                    img = new KeyValuePair<int, Image>(currIndex, new Image() { Source = bmp });    // Creates new reference to image with its corresponding index in the list.
                }
            }   // Exit while loop.
        }
        */

        ///// <summary>
        ///// Error thrown if an image cannot be opened.
        ///// </summary>
        ///// <param name="sender"></param>
        ///// <param name="e"></param>
        //private void Image_ImageFailed(object sender, ExceptionRoutedEventArgs e)
        //{
        //
        //}
        //
        ///// <summary>
        ///// An image was successfully opened.
        ///// </summary>
        //private void Image_ImageOpened(object sender, RoutedEventArgs e)
        //{
        //
        //}

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
                System.Diagnostics.Process.Start(images[currIndex].Value.Path);
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
                await images[currIndex].Value.CopyAsync(Configuration.SavedPhotosDirectory, images[currIndex].Value.Name, NameCollisionOption.ReplaceExisting);
            }
            catch (Exception exc)
            {
                Windows.UI.Popups.MessageDialog md = new Windows.UI.Popups.MessageDialog("We could not save this image. Please try again.\n\nError message: " + exc.Message, (exc.GetType()).Name + " - Could Not Save Image");
                await md.ShowAsync();
            }
        }   // Close btnSave_Click().

        /// <summary>
        /// User clicked the Share Image button.
        /// </summary>
        private void btnShare_Click(object sender, RoutedEventArgs e)
        {

        }   // Close btnShare_Click().
    }
}
