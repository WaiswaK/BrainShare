using BrainShare.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;

namespace BrainShare.Core
{
    class ImageTask
    {
        //Getting image name from a string
        public static string imageName(string filepath)
        {
            string imagename = string.Empty;
            char[] delimiter = { '/' };
            string[] linksplit = filepath.Split(delimiter);
            List<string> linklist = linksplit.ToList();
            imagename = linklist.Last();
            return imagename;
        }
        //Getting image format from a string 
        public static string imageFormat(string filepath)
        {
            int f = 0;
            string imageformat = string.Empty;
            filepath = filepath.ToLower(); //Changing to lower-case
            int l = filepath.Length;
            int JPG_extension = Constants.JPG_extension.Length;
            int PNG_extension = Constants.PNG_extension.Length;
            int GIF_extension = Constants.GIF_extension.Length;
            int TIFF_extension = Constants.TIFF_extension.Length;
            int BMP_extension = Constants.BMP_extension.Length;

            //Search for jpg 
            for (int i = 0; i < l; i++)
            {
                if (filepath[i] == Constants.JPG_extension[0])
                {
                    for (int K = i + 1, j = 1; j < JPG_extension; j++, K++)
                    {
                        if (filepath[K] == Constants.JPG_extension[j])
                        {
                            f++;
                        }
                    }
                }
            }
            if (f == JPG_extension - 1)
            {
                imageformat = Constants.JPG_extension;
            }

            if (imageformat == string.Empty)
            {
                //Search for png 
                f = 0;
                for (int i = 0; i < l; i++)
                {
                    if (filepath[i] == Constants.PNG_extension[0])
                    {
                        for (int K = i + 1, j = 1; j < PNG_extension; j++, K++)
                        {
                            if (filepath[K] == Constants.PNG_extension[j])
                            {
                                f++;
                            }
                        }
                    }
                }
                if (f == PNG_extension - 1)
                {
                    imageformat = Constants.PNG_extension;
                }
                if (imageformat == string.Empty)
                {
                    //Search for gif 
                    f = 0;
                    for (int i = 0; i < l; i++)
                    {
                        if (filepath[i] == Constants.GIF_extension[0])
                        {
                            for (int K = i + 1, j = 1; j < GIF_extension; j++, K++)
                            {
                                if (filepath[K] == Constants.GIF_extension[j])
                                {
                                    f++;
                                }
                            }
                        }
                    }
                    if (f == GIF_extension - 1)
                    {
                        imageformat = Constants.GIF_extension;
                    }
                    if (imageformat == string.Empty)
                    {
                        //Search for bmp 
                        f = 0;
                        for (int i = 0; i < l; i++)
                        {
                            if (filepath[i] == Constants.BMP_extension[0])
                            {
                                for (int K = i + 1, j = 1; j < BMP_extension; j++, K++)
                                {
                                    if (filepath[K] == Constants.BMP_extension[j])
                                    {
                                        f++;
                                    }
                                }
                            }
                        }
                        if (f == BMP_extension - 1)
                        {
                            imageformat = Constants.BMP_extension;
                        }
                        if (imageformat == string.Empty)
                        {
                            //Search for tiff
                            f = 0;
                            for (int i = 0; i < l; i++)
                            {
                                if (filepath[i] == Constants.TIFF_extension[0])
                                {
                                    for (int K = i + 1, j = 1; j < TIFF_extension; j++, K++)
                                    {
                                        if (filepath[K] == Constants.TIFF_extension[j])
                                        {
                                            f++;
                                        }
                                    }
                                }
                            }
                            if (f == TIFF_extension - 1)
                            {
                                imageformat = Constants.TIFF_extension;
                            }
                        }
                    }
                }
            }
            return imageformat;
        }
        //Method to Format a weblink for download of Images
        public static string httplink(string filepath)
        {
            int f = 0;
            string weblink = string.Empty;
            int l = filepath.Length;
            string link = "http";
            int http = link.Length;

            //Search for http in link 
            for (int i = 0; i < l; i++)
            {
                if (filepath[i] == link[0])
                {
                    for (int K = i + 1, j = 1; j < http; j++, K++)
                    {
                        if (filepath[K] == link[j])
                        {
                            f++;
                        }
                    }
                }
            }
            if (f == http - 1)
            {
                weblink = filepath;
            }
            else
            {
                weblink = Constants.BaseUri + filepath;
            }
            return weblink;
        }
        //Method to get the 10 Digit numbers after a link
        public static string imageNumbers(string fileName)
        {
            string imagename = string.Empty;
            char[] delimiter = { '?' };
            string[] linksplit = fileName.Split(delimiter);
            List<string> linklist = linksplit.ToList();
            imagename = linklist.Last();
            return imagename;
        }
        //Methods to convert Image to Base64 String on while online the other offline
        public static async Task<string> Base64(string image_path)
        {
            StorageFolder appFolder = Constants.appFolder;
            string image = imageName(image_path);
            StorageFile file = await appFolder.GetFileAsync(image);
            string base64 = string.Empty;
            using (var stream = await file.OpenAsync(FileAccessMode.Read))
            {
                var reader = new DataReader(stream.GetInputStreamAt(0));
                var bytes = new byte[stream.Size];
                await reader.LoadAsync((uint)stream.Size);
                reader.ReadBytes(bytes);
                base64 = Convert.ToBase64String(bytes);
            }
            return base64;
        }
        public static async Task<string> LocalBase64(string image_path, string fileformat)
        {
            StorageFolder appFolder = Constants.appFolder;
            string image = imageName(image_path + fileformat);
            string base64 = string.Empty;
            try
            {
                StorageFile file = await appFolder.GetFileAsync(image);
                using (var stream = await file.OpenAsync(FileAccessMode.Read))
                {
                    var reader = new DataReader(stream.GetInputStreamAt(0));
                    var bytes = new byte[stream.Size];
                    await reader.LoadAsync((uint)stream.Size);
                    reader.ReadBytes(bytes);
                    base64 = Convert.ToBase64String(bytes);
                }
            }
            catch
            {

            }
            return base64;
        }
        //Function to make image path
        public static string imagePath(string imagename)
        {
            string path = Path.Combine(Constants.appFolder.Path, imagename);
            return path;
        }
    }
}
