using System.IO;
using Windows.Storage; //To be removed after test

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
          
        //Function to make image path
        public static string imagePath(string imagename)
        {
            string path = RemoveFowardSlashes(Path.Combine(ApplicationData.Current.LocalFolder.Path, imagename));
            return path;
        }
        private static string RemoveFowardSlashes(string word)
        {
            string current =  "\\" ;
            string ChangeTo = "\\";
            //string path = ChangeTo.ToString();
            //string path = word.Replace(current, ChangeTo);
            string path = word.Replace(current, ChangeTo);
            //NotesImageDownloader("");
            return path;        
        }
    }
}



