using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Text.RegularExpressions;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Imaging;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace StaticSiinGroup
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

        private async void Choose_Avatar(object sender, RoutedEventArgs e)
        {
            FileOpenPicker fileOpenPicker = new FileOpenPicker
            {
                SuggestedStartLocation = PickerLocationId.PicturesLibrary,

                ViewMode = PickerViewMode.Thumbnail
            };

            fileOpenPicker.FileTypeFilter.Clear();
            fileOpenPicker.FileTypeFilter.Add(".png");
            fileOpenPicker.FileTypeFilter.Add(".jpeg");
            fileOpenPicker.FileTypeFilter.Add(".jpg");
            fileOpenPicker.FileTypeFilter.Add(".bmp");

            StorageFile file = await fileOpenPicker.PickSingleFileAsync();
            
            if (file != null)
            {
                IRandomAccessStream fileStream =
                await file.OpenAsync(FileAccessMode.Read);

                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.SetSource(fileStream);

                IBuffer buffer = await FileIO.ReadBufferAsync(file);

                CloudStorageAccount storageAccount = CloudStorageAccount.Parse("DefaultEndpointsProtocol=https;AccountName=staticsiingroup;AccountKey=Rg7RY5YLvWujqJBk+iuBKHVXudGSdGTXbcx2DyskvEH30p5YHFHL5Jj0orwyPQgY0G9WlziE1OM0CxtXqCTi5Q==;EndpointSuffix=core.windows.net");
                CloudBlobContainer cloudBlobContainer = null;

                string storageConnectionString = Environment.GetEnvironmentVariable("storageconnectionstring");

                
                    try
                    {
                        CloudBlobClient cloudBlobClient = storageAccount.CreateCloudBlobClient();

                        cloudBlobContainer = cloudBlobClient.GetContainerReference("staticsiingroup");
                        await cloudBlobContainer.CreateIfNotExistsAsync();

                        // Set the permissions so the blobs are public. 
                        BlobContainerPermissions permissions = new BlobContainerPermissions
                        {
                            PublicAccess = BlobContainerPublicAccessType.Blob
                        };
                        await cloudBlobContainer.SetPermissionsAsync(permissions);

                        CloudBlockBlob cloudBlockBlob = cloudBlobContainer.GetBlockBlobReference(file.Name.ToString());
                        await cloudBlockBlob.UploadFromFileAsync(file);
                        
                        BlobContinuationToken blobContinuationToken = null;
                        try
                        {
                            var results = await cloudBlobContainer.ListBlobsSegmentedAsync(null, blobContinuationToken);
                            blobContinuationToken = results.ContinuationToken;
                            foreach (IListBlobItem item in results.Results)
                            {
                                Debug.WriteLine(item.Uri);
                                UrlUpload.Text = item.Uri.ToString();
                                PreviewAvatar.Source = bitmapImage;
                            }
                        } catch(Exception err)
                        {
                            Debug.WriteLine(err);
                        }
                    }
                    catch (StorageException ex)
                    {
                        Debug.WriteLine("Error returned from the service: {0}", ex.Message);
                    }
                }
        }
    }
}
