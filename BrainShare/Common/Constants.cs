using System.IO;
using Windows.Storage; //To be removed after test
using System;//To be removed after
using System.Net;
using Windows.UI.Xaml.Media.Imaging;
using System.Net.Http;
using Windows.Storage.Streams;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace BrainShare.Common
{
    class Constants
    {
        public static string dbName = "BrainShareDB.sqlite";
        public static string dbPath = Path.Combine(ApplicationData.Current.LocalFolder.Path, dbName);
        public static StorageFolder appFolder = ApplicationData.Current.LocalFolder;
        public static string BaseUri = "http://brainshare.ug";
        public static string ImageFolder = "ImageFolder";
        public static string PDF_extension = ".pdf";
        public static string JPG_extension = ".jpg";
        public static string PNG_extension = ".png";
        public static string GIF_extension = ".gif";
        public static string TIFF_extension = ".tiff";
        public static string BMP_extension = ".bmp";
        public static int updating = 1;
        public static int finished_update = 0;
        public static bool NotesModule;
        public static bool VideosModule;
        public static bool LibraryModule;


        /*
        public static string NotesUpdater(string notes, string subject, string topic)
        {
            string start_string = "/assets/content_images/";
            int notes_image = 0;
            string expression = start_string + @"\S*\d{10}"; //Ending with 10 digits and starting with /assets/content_images/
            List<string> downloadLinks = Links(notes, expression); //First expression
            string start_string_two = "http://";
            string expression_png = start_string_two + @"\S*" + Constants.PNG_extension;
            string expression_jpg = start_string_two + @"\S*" + Constants.JPG_extension;
            string expression_gif = start_string_two + @"\S*" + Constants.GIF_extension;
            string expression_bmp = start_string_two + @"\S*" + Constants.BMP_extension;
            string expression_tiff = start_string_two + @"\S*" + Constants.TIFF_extension;


            List<string> jpg_links = Links(notes, expression_jpg); //Links with png
            List<string> png_links = Links(notes, expression_png); //Links with jpg
            List<string> gif_links = Links(notes, expression_png); //Links with gif
            List<string> bmp_links = Links(notes, expression_bmp); //Links with bmp
            List<string> tiff_links = Links(notes, expression_tiff); //Links with tiff
                                                                     //Search for links with first Regular Expression
            foreach (string _string in downloadLinks)
            {
                //notes_image++;
                string imageName = imagePath(imageName(school.ImagePath));
                //await ImageDownloader(imageName, _string); 

                try
                {
                    await NotesImageDownloader(_string);
                }
                catch
                {

                }
            }
            //Search for links with Second Regular Expression with jpg 
            foreach (string _string in jpg_links)
            {
                notes_image++;
                string imageName = subject + "-" + topic + "_" + "notes_image" + notes_image.ToString();
                //await ImageDownloader(imageName, _string); 
                try
                {
                    await NotesImageDownloader(_string);
                }
                catch
                {

                }
            }
            //Search for links with Second Regular Expression with png 
            foreach (string _string in png_links)
            {
                notes_image++;
                string imageName = subject + "-" + topic + "_" + "notes_image" + notes_image.ToString();
                //await ImageDownloader(imageName, _string);
                try
                {
                    await NotesImageDownloader(_string);
                }
                catch
                {

                }
            }
            //Search for links with Second Regular Expression with bmp
            foreach (string _string in bmp_links)
            {
                notes_image++;
                string imageName = subject + "-" + topic + "_" + "notes_image" + notes_image.ToString();
                //await ImageDownloader(imageName, _string);
                try
                {
                    await NotesImageDownloader(_string);
                }
                catch
                {

                }
            }
            //Search for links with Second Regular Expression with tiff 
            foreach (string _string in tiff_links)
            {
                notes_image++;
                string imageName = subject + "-" + topic + "_" + "notes_image" + notes_image.ToString();
                //await ImageDownloader(imageName, _string);
                try
                {
                    await NotesImageDownloader(_string);
                }
                catch
                {

                }
            }
        }


            public static string imageName(string filepath)
        {
            string imagename = string.Empty;
            char[] delimiter = { '/' };
            string[] linksplit = filepath.Split(delimiter);
            List<string> linklist = linksplit.ToList();
            imagename = linklist.Last();
            return imagename;
        }

        public static string imagePath(string imagename)
        {
            string path = Path.Combine(ApplicationData.Current.LocalFolder.Path, imagename);
            return path;
        }
        */
    }
}



