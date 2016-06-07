using BrainShare.Common;
using BrainShare.Database;
using System.Collections.Generic;
using System.Linq;

namespace BrainShare.Core
{
    class ErrorLogTask
    {
        public string Error_title { get; set; }
        public string Error_details { get; set; }
        public string Location { get; set; }    
        public static void UploadLogFile()
        {
            try
            {
                using (var db = new SQLite.SQLiteConnection(Constants.dbPath))
                {
                    var query = (db.Table<log>().ToList());
                    List<ErrorLogTask> logs = new List<ErrorLogTask>();
                    foreach (var error in query)
                    {
                        ErrorLogTask log = new ErrorLogTask()
                        {
                            Error_details = error.Error_details,
                            Error_title = error.Error_title,
                            Location = error.Location
                        };
                        logs.Add(log);
                    }
                }
            }
            catch
            {

            }
            //Trigger upload of the errors
            //Delete errors after send
        }     
        public static void LogFileSaveAsync(ErrorLogTask Errorlog)
        {
            try
            {
                var db = new SQLite.SQLiteConnection(Constants.dbPath);
                db.Insert(new log() { Error_title = Errorlog.Error_title, Error_details = Errorlog.Error_details, Location = Errorlog.Location });
            }
            catch
            {

            }
        }
    }
}
