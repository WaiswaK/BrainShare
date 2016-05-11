using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Networking.Connectivity;
using Windows.Storage;
using BrainShare.Database;
using Windows.Networking.BackgroundTransfer;
using Windows.Storage.Streams;
using System.Net.Http;
using Windows.UI.Xaml.Media.Imaging;
using BrainShare.Core;
using Windows.Data.Pdf;
using BrainShare.Models;
using System.Collections.ObjectModel;

namespace BrainShare.Common
{
    class CommonTask
    {
        //Method that checks if the App is online
        public static bool IsInternetConnectionAvailable()
        {
            ConnectionProfile connection = NetworkInformation.GetInternetConnectionProfile();
            bool internet = connection != null && connection.GetNetworkConnectivityLevel() == NetworkConnectivityLevel.InternetAccess;
            return internet;
        }
        //Method to download Files
        public static async Task FileDownloader(string filepath, string fileName)
        {
            StorageFile storageFile = await Constants.appFolder.CreateFileAsync(fileName + Constants.PDF_extension, CreationCollisionOption.ReplaceExisting);
            string newpath = Constants.BaseUri + filepath;
            try
            {
                var downloader = new BackgroundDownloader();
                Uri uri = new Uri(newpath);
                DownloadOperation op = downloader.CreateDownload(uri, storageFile);
                await op.StartAsync();
            }
            catch(Exception ex)
            {
                ErrorLogTask Logfile = new ErrorLogTask();
                Logfile.Error_details = ex.ToString();
                Logfile.Error_title = "FileDownloader Method";
                Logfile.Location = "CommonTask";
                //await ErrorLogTask.LogFileSaveAsync(Logfile);
            }
        }
        #region Images download Methods
        public static async Task ImageDownloader(string filepath, string fileName)
        {
            filepath = ImageTask.httplink(filepath); //Format download link
            Uri uri = new Uri(filepath);
            string imageformat = ImageTask.imageFormat(filepath);

            // download pic
            var bitmapImage = new BitmapImage();
            var httpClient = new HttpClient();
            var httpResponse = await httpClient.GetAsync(uri);
            byte[] b = await httpResponse.Content.ReadAsByteArrayAsync();

            // create a new in memory stream and datawriter
            using (var stream = new InMemoryRandomAccessStream())
            {
                using (DataWriter dw = new DataWriter(stream))
                {
                    // write the raw bytes and store
                    dw.WriteBytes(b);
                    await dw.StoreAsync();

                    // set the image source
                    stream.Seek(0);
                    bitmapImage.SetSource(stream);

                    // write to local pictures
                    StorageFile storageFile = await Constants.appFolder.CreateFileAsync(fileName + imageformat, 
                        CreationCollisionOption.FailIfExists);
                    using (var storageStream = await storageFile.OpenAsync(FileAccessMode.ReadWrite))
                    {
                        await RandomAccessStream.CopyAndCloseAsync(stream.GetInputStreamAt(0), storageStream.GetOutputStreamAt(0));
                    }
                }
            }
        }
        public static async Task ForceImageDownloader(string filepath, string fileName, string extension)
        {
            filepath = ImageTask.httplink(filepath); //Format download link
            Uri uri = new Uri(filepath);

            // download pic
            var bitmapImage = new BitmapImage();
            var httpClient = new HttpClient();
            var httpResponse = await httpClient.GetAsync(uri);
            byte[] b = await httpResponse.Content.ReadAsByteArrayAsync();

            // create a new in memory stream and datawriter
            using (var stream = new InMemoryRandomAccessStream())
            {
                using (DataWriter dw = new DataWriter(stream))
                {
                    // write the raw bytes and store
                    dw.WriteBytes(b);
                    await dw.StoreAsync();

                    // set the image source
                    stream.Seek(0);
                    bitmapImage.SetSource(stream);

                    // write to local pictures
                    StorageFile storageFile;
                    try
                    {
                        storageFile = await Constants.appFolder.CreateFileAsync(fileName + extension,
                            CreationCollisionOption.ReplaceExisting);
                        using (var storageStream = await storageFile.OpenAsync(FileAccessMode.ReadWrite))
                        {
                            await RandomAccessStream.CopyAndCloseAsync(stream.GetInputStreamAt(0), storageStream.GetOutputStreamAt(0));
                        }
                    }
                    catch(Exception ex)
                    {
                        ErrorLogTask Logfile = new ErrorLogTask();
                        Logfile.Error_details = ex.ToString();
                        Logfile.Error_title = "ForceImagedownloader Method";
                        Logfile.Location = "CommonTask";
                        //await ErrorLogTask.LogFileSaveAsync(Logfile);
                    }                  
                }
            }
        }
        #endregion       
        //Method to Initialize the SQLite Database
        public static async Task InitializeDatabase()
        {
            try
            {
                DbConnection Dbconnect = new DbConnection();
                await Dbconnect.InitializeDatabase();
            }
            catch
            {

            }
        }
        //Method to format the youtube Link
        public static string newYouTubeLink(string link)
        {
            char[] delimiter1 = { '=' };
            char[] delimiter2 = { '/' };
            string[] linksplit = link.Split(delimiter1);
            List<string> linklist = linksplit.ToList();
            string linkfile = linklist.Last();
            string finallink = "https://www.youtube.com/embed/" + linkfile;
            return finallink;
        }
        #region PDF Reader Functions
        //Method to load PDF File
        public static async Task LoadPdfFileAsync(StorageFile File, ObservableDictionary DefaultViewModel, double ActualWidth)
        {
            try
            {
                StorageFile pdfFile = File;
                PdfDocument pdfDocument = await PdfDocument.LoadFromFileAsync(pdfFile); ;
                ObservableCollection<PdfDataItem> items = new ObservableCollection<PdfDataItem>();
                DefaultViewModel["Items"] = items;
                if (pdfDocument != null && pdfDocument.PageCount > 0)
                {
                    for (int pageIndex = 0; pageIndex < pdfDocument.PageCount; pageIndex++)
                    {
                        var pdfPage = pdfDocument.GetPage((uint)pageIndex);
                        if (pdfPage != null)
                        {
                            StorageFolder tempFolder = ApplicationData.Current.TemporaryFolder;
                            StorageFile pngFile = await tempFolder.CreateFileAsync(Guid.NewGuid().ToString() + ".png", CreationCollisionOption.ReplaceExisting);

                            if (pngFile != null)
                            {
                                IRandomAccessStream randomStream = await pngFile.OpenAsync(FileAccessMode.ReadWrite);
                                PdfPageRenderOptions pdfPageRenderOptions = new PdfPageRenderOptions();
                                pdfPageRenderOptions.DestinationWidth = (uint)(ActualWidth - 130);
                                try
                                {
                                    await pdfPage.RenderToStreamAsync(randomStream, pdfPageRenderOptions);
                                }
                                catch
                                {

                                }
                                try
                                {
                                    await randomStream.FlushAsync();
                                }
                                catch
                                {

                                }
                                randomStream.Dispose();
                                pdfPage.Dispose();
                                items.Add(new PdfDataItem(
                                    pageIndex.ToString(),
                                    pageIndex.ToString(),
                                    pngFile.Path));
                            }
                        }
                    }
                }
            }
            catch (Exception err)
            {
                string error = err.ToString();
            }
        }
        //Method to delete Temporary files used in PDF Reader
        public static async Task DeleteTemporaryFiles()
        {
            StorageFolder tempFolder = ApplicationData.Current.TemporaryFolder;
            IReadOnlyList<StorageFile> images = await tempFolder.GetFilesAsync();
            foreach (var image in images)
            {
                await image.DeleteAsync();
            }
        }
        #endregion
    }
}
