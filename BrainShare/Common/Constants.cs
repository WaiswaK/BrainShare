using System.IO;
using Windows.Storage;

namespace BrainShare.Common
{
    class Constants
    {
        public static string dbName = "BrainShareDB.sqlite";
        public static StorageFolder appFolder = ApplicationData.Current.LocalFolder;
        public static string dbPath = Path.Combine(appFolder.Path, dbName);
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


       

    }
}



