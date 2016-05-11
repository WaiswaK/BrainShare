using BrainShare.Common;
using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;
using System.Runtime.Serialization;

namespace BrainShare.Core
{
    class ErrorLogTask
    {
        public string Error_title { get; set; }
        public string Error_details { get; set; }
        public string Location { get; set; }

        /*
        public async Task UploadAsync()
        {
            StorageFile file = await ApplicationData.Current.LocalFolder.GetFileAsync("UserDetails");
            if (file == null) return;
            IRandomAccessStream inStream = await file.OpenReadAsync();
            // Deserialize the Session State.
            DataContractSerializer serializer = new DataContractSerializer(typeof(UserDetail));
            var StatsDetails = (UserDetail)serializer.ReadObject(inStream.AsStreamForRead());
            inStream.Dispose();
            email_tb.Text = StatsDetails.email;
            password_tb.Password = StatsDetails.password;
        }
        */
        public static async Task LogFileSaveAsync(ErrorLogTask Errorlog)
        {
            StorageFile logfile = await Constants.appFolder.CreateFileAsync("Log.txt", CreationCollisionOption.GenerateUniqueName);
            IRandomAccessStream raStream = await logfile.OpenAsync(FileAccessMode.ReadWrite);
            using (IOutputStream outStream = raStream.GetOutputStreamAt(0))
            {
                // Serialize the Session State.
                DataContractSerializer serializer = new DataContractSerializer(typeof(ErrorLogTask));
                serializer.WriteObject(outStream.AsStreamForWrite(), Errorlog);
                await outStream.FlushAsync();
            }
        }
    }
}
