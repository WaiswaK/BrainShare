using System.IO;
using System.Threading.Tasks;
using Windows.Storage; //To be removed after test
using Windows.Storage.Streams; //To be removed 
using System;//To be removed after

namespace BrainShare.Common
{
    class Constants
    {    
        public static string dbName  = "BrainShareDB.sqlite";
        public static string dbPath = Path.Combine(ApplicationData.Current.LocalFolder.Path, dbName);
        public static StorageFolder appFolder = ApplicationData.Current.LocalFolder;
        public static string BaseUri = "http://brainshare.ug";
        public static string ImageFolder = "ImageFolder";
        public static string PDF_extension = ".pdf";
        public static string JPG_extension = ".jpg";
        public static string PNG_extension = ".png";
        public static int updating = 1;
        public static int finished_update = 0;
    }
}



