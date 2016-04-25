using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Networking.Connectivity;
using Windows.Storage;
using Windows.Data.Json;
using BrainShare.Database;
using Windows.Networking.BackgroundTransfer;
using BrainShare.Models;
using System.Text.RegularExpressions;
using System.IO;

namespace BrainShare.Common
{
    class CommonTask
    {
        public static bool IsInternetConnectionAvailable()
        {
            ConnectionProfile connection = NetworkInformation.GetInternetConnectionProfile();
            bool internet = connection != null && connection.GetNetworkConnectivityLevel() == NetworkConnectivityLevel.InternetAccess;
            return internet;
        }
        public static void InsertSubjectsAsync(List<SubjectObservable> subjects)
        {
            List<TopicObservable> topics = new List<TopicObservable>();
            List<AttachmentObservable> files = new List<AttachmentObservable>();
            List<VideoObservable> videos = new List<VideoObservable>();
            List<AssignmentObservable> assignments = new List<AssignmentObservable>();
            bool proceed = true;
            try
            {
                var db = new SQLite.SQLiteConnection(Constants.dbPath);

                foreach (var subject in subjects)
                {
                    try
                    {
                        var query = (db.Table<Subject>().Where(c => c.SubjectId == subject.Id)).Single();
                        proceed = false;
                    }
                    catch
                    {
                        proceed = true;
                    }
                    if (proceed == true)
                    {
                        try
                        {
                            db.Insert(new Subject() { SubjectId = subject.Id, name = subject.name, thumb = subject.thumb });
                        }
                        catch
                        {

                        }
                        topics = subject.topics;
                        if (topics.Count > 0)
                        {
                            foreach (var topic in topics)
                            {
                                try
                                {
                                        db.Insert(new Topic() { TopicID = topic.TopicID, Notes = topic.body, SubjectId = subject.Id, teacher_full_names = topic.teacher, TopicTitle = topic.TopicTitle, Updated_at = topic.Updated_at, Folder_Id = topic.folder_id, Folder_Name = topic.folder_name });
                                }
                                catch
                                {

                                }
                                files = topic.Files;
                                if (files.Count > 0)
                                {
                                    foreach (var file in files)
                                    {
                                        try
                                        {
                                            db.Insert(new Attachment() { AttachmentID = file.AttachmentID, FileName = file.FileName, FilePath = file.FilePath, TopicID = topic.TopicID, SubjectId = 0, AssignmentID = 0 });
                                        }
                                        catch
                                        {

                                        }
                                    }
                                }
                            }
                        }

                        videos = subject.videos;
                        if (videos.Count > 0)
                        {
                            foreach (var video in videos)
                            {
                                try
                                {
                                    db.Insert(new Video() { VideoID = video.VideoID, description = video.description, FileName = video.FileName, FilePath = video.FilePath, teacher_full_names = video.teacher, SubjectId = subject.Id });
                                }
                                catch
                                {

                                }
                            }
                        }

                        files = subject.files;
                        if (files.Count > 0)
                        {
                            foreach (var file in files)
                            {
                                try
                                {
                                    db.Insert(new Attachment() { AttachmentID = file.AttachmentID, FileName = file.FileName, FilePath = file.FilePath, TopicID = 0, SubjectId = subject.Id, AssignmentID = 0 });
                                }
                                catch
                                {

                                }
                            }
                        }

                        assignments = subject.assignments;
                        if (assignments.Count > 0)
                        {
                            foreach (var assignment in assignments)
                            {
                                try
                                {
                                    db.Insert(new Assignment() { AssignmentID = assignment.id, description = assignment.description, teacher_full_names = assignment.teacher, title = assignment.title, SubjectId = subject.Id });
                                }
                                catch
                                {

                                }
                                files = assignment.Files;
                                if (files.Count > 0)
                                {
                                    foreach (var file in files)
                                    {
                                        try
                                        {
                                            db.Insert(new Attachment() { AttachmentID = file.AttachmentID, FileName = file.FileName, FilePath = file.FilePath, TopicID = 0, SubjectId = 0, AssignmentID = assignment.id });
                                        }
                                        catch
                                        {

                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch
            {
            }
            }
        public static void InsertSubjectsUpdateAsync(List<SubjectObservable> subjects)
        {
            List<TopicObservable> topics = new List<TopicObservable>();
            List<AttachmentObservable> files = new List<AttachmentObservable>();
            try { 
            var db = new SQLite.SQLiteConnection(Constants.dbPath);
                foreach (var subject in subjects)
                {
                    topics = subject.topics;
                    if (topics.Count > 0)
                    {
                        foreach (var topic in topics)
                        {
                            try
                            {
                                db.Insert(new Topic() { TopicID = topic.TopicID, Notes = topic.body, SubjectId = subject.Id, teacher_full_names = topic.teacher, TopicTitle = topic.TopicTitle, Updated_at = topic.Updated_at, Folder_Id = topic.folder_id, Folder_Name = topic.folder_name });
                            }
                            catch
                            {

                            }
                            files = topic.Files;
                            if (files.Count > 0)
                            {
                                foreach (var file in files)
                                {
                                    try
                                    {
                                        db.Insert(new Attachment() { AttachmentID = file.AttachmentID, FileName = file.FileName, FilePath = file.FilePath, TopicID = topic.TopicID, SubjectId = 0, AssignmentID = 0 });
                                    }
                                    catch
                                    {

                                    }
                                }
                            }
                        }
                    }
                }

            }
            catch
            {

            }
        }
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
            catch
            {
                //var message = new MessageDialog("An error occured while downloading", "Download Error").ShowAsync();
            }
        }
        public static async Task ImageDownloader(string fileName, string filepath)
        {
            StorageFile storageFile = await Constants.appFolder.CreateFileAsync(fileName + Constants.PNG_extension, CreationCollisionOption.ReplaceExisting);
            string newpath = Constants.BaseUri + filepath;
            try
            {
                var downloader = new BackgroundDownloader();
                Uri uri = new Uri(newpath);
                DownloadOperation op = downloader.CreateDownload(uri, storageFile);
                await op.StartAsync();
            }
            catch
            {

            }
        }     
        //Notes image download methods
        public static async void NotesImageDownloader(string notes, string subject, string topic)
        {
            string start_string = "/assets/content_images/";
            int notes_image = 0;
            string expression = start_string + @"\S*\d{10}"; //Ending with 10 digits and starting with /assets/content_images/
            List<string> downloadLinks = Links(notes, expression); //First expression
            string start_string_two = "http://imgur.com/";
            string expression_png = start_string_two + @"\S*" + Constants.PNG_extension;
            string expression_jpg = start_string_two + @"\S*" + Constants.JPG_extension;
 
            List<string> imgur_jpg_links = Links(notes, expression_jpg); //Links with png
            List<string> imgur_png_links = Links(notes, expression_png); //Links with jpg
          
            //Search for links with first Regular Expression
            foreach (string _string in downloadLinks)
            {
                notes_image++;
                string imageName = subject + "-" + topic + "_" + "notes_image" + notes_image.ToString();
                await ImageDownloader(imageName, _string); 
            }

            //Search for links with Seconf Regular Expression with jpg 
            foreach (string _string in imgur_jpg_links)
            {
                notes_image++;
                string imageName = subject + "-" + topic + "_" + "notes_image" + notes_image.ToString();
                await ImageDownloader(imageName, _string); 
            }

            //Search for links with Seconf Regular Expression with png 
            foreach (string _string in imgur_png_links)
            {
                notes_image++;
                string imageName = subject + "-" + topic + "_" + "notes_image" + notes_image.ToString();
                await ImageDownloader(imageName, _string);
            }
        }
        public static void GetNotesImagesSubjectsAsync(List<SubjectObservable> subjects)
        {
            List<TopicObservable> topics = new List<TopicObservable>();
            try
            {
                var db = new SQLite.SQLiteConnection(Constants.dbPath);
                foreach (var subject in subjects)
                {
                    topics = subject.topics;
                    if (topics != null)
                    {
                        foreach (var topic in topics)
                        {
                            try
                            {
                                NotesImageDownloader(topic.body, subject.name, topic.folder_name);
                                string new_notes = NotesUpdater(topic.body, subject.name, topic.folder_name);
                                Topic newTopic = new Topic(topic.TopicID, subject.Id, topic.TopicTitle, new_notes, topic.Updated_at, topic.teacher, topic.folder_id, topic.folder_name);

                                try
                                {
                                    db.Update(newTopic);
                                }
                                catch
                                {

                                }
                            }
                            catch
                            {

                            }
                        }
                    }

                }
            }
            catch
            {

            }
        }
        //Method to updateNotes
        public static string NotesUpdater(string notes, string subject, string topic)
        {
            string start_string = "/assets/content_images/";
            string expression = start_string + @"\S*\d{10}"; //Ending with 10 digits and starting with /assets/content_images/
            List<string> downloadLinks = Links(notes, expression);
            int notes_image = 0;
            foreach (string _string in downloadLinks)
            {
                notes_image++;
                string path = ApplicationData.Current.LocalFolder.Path; //Path of Image
                string imageName = path + subject + "-" + topic + "_" + "notes_image" + notes_image.ToString();
                notes.Replace(_string,imageName);
            }
            return notes;
        }
        //Function to make image path
        public static string imagePath(string imagename)
        {
            string path = Path.Combine(ApplicationData.Current.LocalFolder.Path, imagename);
            return path;
        }
        //Method to get all image link locations in notes
        private static List<string> Links(string text, string expr)
        {
            List<string> collection = new List<string>();
            List<string> links = new List<string>();
            MatchCollection mc = Regex.Matches(text, expr);
            foreach (Match m in mc)
            {
                collection.Add(m.ToString());
            }
            return collection;
        }
        public static async Task InsertUserAsync(UserObservable user)
        {
            var db = new SQLite.SQLiteConnection(Constants.dbPath);
            List<string> subjectsnames = SubjectNames(user.subjects);
            string ConcatSubjects = JoinedSubjects(subjectsnames);
            SchoolObservable school = user.School;
            try
            {
                db.Insert(new User() { e_mail = user.email, password = user.password, School_id = school.SchoolId, subjects = ConcatSubjects, profileName = user.full_names });
            }
            catch
            {
            }
            try
            {
                await ImageDownloader(school.SchoolName, school.ImagePath);
            }
            catch
            {

            }
            string newPath = imagePath(school.SchoolName + Constants.PNG_extension);
            try
            {
                db.Insert(new School() { SchoolName = school.SchoolName, SchoolBadge = newPath, School_id = school.SchoolId });
            }
            catch
            {

            }
        }
        private static List<string> SubjectNames(List<SubjectObservable> subjects)
        {
            List<string> subjectnames = new List<string>();
            string sub;
            foreach (var subject in subjects)
            {
                sub = subject.Id.ToString();
                subjectnames.Add(sub);
            }
            return subjectnames;
        }
        private static string JoinedSubjects(List<string> subjects)
        {
            string joined = "";
            foreach (var subject in subjects)
            {
                if (joined.Equals(""))
                {
                    joined = subject;
                }
                else
                {
                    joined = joined + "." + subject;
                }
            }
            return joined;
        }
        public static SchoolObservable GetSchool(int school_id)
        {
            SchoolObservable school = new SchoolObservable();
            using (var db = new SQLite.SQLiteConnection(Constants.dbPath))
            {
                var query = (db.Table<School>().Where(c => c.School_id == school_id)).Single();
                school = new SchoolObservable(query.SchoolName, query.SchoolBadge, query.School_id);
            }
            return school;
        }
        public static SubjectObservable GetSubject(int id)
        {
            SubjectObservable sub = new SubjectObservable();
            using (var db = new SQLite.SQLiteConnection(Constants.dbPath))
            {
                var query = (db.Table<Subject>().Where(c => c.SubjectId == id)).Single();
                sub.Id = id;
                sub.name = query.name;
                sub.thumb = query.thumb;
                sub.topics = GetTopics(query.SubjectId);
                sub.videos = GetVideos(query.SubjectId);
                sub.files = GetFiles(0, query.SubjectId, 0);
                sub.assignments = GetAssignments(query.SubjectId);
            }
            return sub;
        }
        private static List<TopicObservable> GetTopics(int subId)
        {
            List<TopicObservable> topics = new List<TopicObservable>();
            TopicObservable topic = null;
            using (var db = new SQLite.SQLiteConnection(Constants.dbPath))
            {
                var query = (db.Table<Topic>().Where(c => c.SubjectId == subId));
                foreach (var _topic in query)
                {
                    topic = new TopicObservable(_topic.TopicID, _topic.Notes, _topic.TopicTitle, GetFiles(_topic.TopicID, 0, 0), _topic.teacher_full_names, _topic.Updated_at, _topic.Folder_Id, _topic.Folder_Name);
                    topics.Add(topic);
                }
            }
            return topics;
        }
        public static List<AttachmentObservable> GetFiles(int id1, int id2, int id3)
        {
            List<AttachmentObservable> attachments = new List<AttachmentObservable>();
            AttachmentObservable attachment = null;
            using (var db = new SQLite.SQLiteConnection(Constants.dbPath))
            {
                if (id1 > 0)
                {
                    var query = (db.Table<Attachment>().Where(c => c.TopicID == id1));
                    foreach (var _file in query)
                    {
                        attachment = new AttachmentObservable(_file.AttachmentID, _file.FilePath, _file.FileName);
                        attachments.Add(attachment);
                    }
                }
                if (id2 > 0)
                {
                    var query = (db.Table<Attachment>().Where(c => c.SubjectId == id2));
                    foreach (var _file in query)
                    {
                        attachment = new AttachmentObservable(_file.AttachmentID, _file.FilePath, _file.FileName);
                        attachments.Add(attachment);
                    }
                }
                if (id3 > 0)
                {
                    var query = (db.Table<Attachment>().Where(c => c.AssignmentID == id3));
                    foreach (var _file in query)
                    {
                        attachment = new AttachmentObservable(_file.AttachmentID, _file.FilePath, _file.FileName);
                        attachments.Add(attachment);
                    }
                }
            }
            return attachments;
        }
        private static List<VideoObservable> GetVideos(int Subjectid)
        {
            List<VideoObservable> videos = new List<VideoObservable>();
            VideoObservable video = null;
            using (var db = new SQLite.SQLiteConnection(Constants.dbPath))
            {
                var query = (db.Table<Video>().Where(c => c.SubjectId == Subjectid));
                foreach (var _video in query)
                {
                    video = new VideoObservable(_video.VideoID, _video.FilePath, _video.FileName, _video.description, _video.teacher_full_names);
                    videos.Add(video);
                }
            }
            return videos;
        }
        private static List<AssignmentObservable> GetAssignments(int subId)
        {
            List<AssignmentObservable> assignments = new List<AssignmentObservable>();
            AssignmentObservable assignment = null;
            using (var db = new SQLite.SQLiteConnection(Constants.dbPath))
            {
                var query = (db.Table<Assignment>().Where(c => c.SubjectId == subId));
                foreach (var _assignment in query)
                {
                    assignment = new AssignmentObservable(_assignment.AssignmentID, _assignment.title, _assignment.description, _assignment.teacher_full_names, GetFiles(0, 0, _assignment.AssignmentID));
                    assignments.Add(assignment);
                }
            }
            return assignments;
        }
        public static async Task UpdateUserAsync(UserObservable user)
        {
            var db = new SQLite.SQLiteConnection(Constants.dbPath);
            List<string> subjectsnames = SubjectNames(user.subjects);
            string ConcatSubjects = JoinedSubjects(subjectsnames);
            ConcatSubjects = UserSubjects(ConcatSubjects);
            SchoolObservable school = user.School;
            string newPath = imagePath(school.SchoolName + Constants.PNG_extension);              
            if (school.ImagePath.Equals(newPath)) { } //Checking if image was downloaded
            else
            {
                await ImageDownloader(school.SchoolName, school.ImagePath);
                School sch = new School(school.SchoolId, school.SchoolName, newPath);
                try
                {
                    db.Update(sch);
                }
                catch
                {

                }
            }

            User userInfo = new User(user.email, user.password, user.full_names, ConcatSubjects, school.SchoolId);
            try
            {
                db.Update(userInfo);
            }
            catch
            {

            }
        }
        public static void UpdateSubjectsAsync(List<SubjectObservable> subjects)
        {
            List<TopicObservable> topics = new List<TopicObservable>();
            List<AttachmentObservable> files = new List<AttachmentObservable>();
            List<VideoObservable> videos = new List<VideoObservable>();
            List<AssignmentObservable> assignments = new List<AssignmentObservable>();
            try
            {
                var db = new SQLite.SQLiteConnection(Constants.dbPath);
                foreach (var subject in subjects)
                {
                    topics = subject.topics;
                    files = subject.files;
                    assignments = subject.assignments;
                    videos = subject.videos;
                    if (topics != null)
                    {
                        foreach (var topic in topics)
                        {
                            Topic newTopic = new Topic(topic.TopicID, subject.Id, topic.TopicTitle, topic.body, topic.Updated_at, topic.teacher, topic.folder_id, topic.folder_name);
                            try
                            {
                                db.Update(newTopic);
                            }
                            catch
                            {

                            }
                            List<AttachmentObservable> topicfiles = topic.Files;
                            List<AttachmentObservable> oldfiles = OldGetFiles(topic.TopicID, 0);
                            List<AttachmentObservable> newfiles = GetNewFiles(topicfiles, oldfiles);
                            if (newfiles == null) { }
                            else
                            {
                                foreach (var file in newfiles)
                                {
                                    try
                                    {
                                        db.Insert(new Attachment() { AttachmentID = file.AttachmentID, FileName = file.FileName, FilePath = file.FilePath, TopicID = topic.TopicID, SubjectId = 0, AssignmentID = 0 });
                                    }
                                    catch
                                    {

                                    }
                                }
                            }
                        }
                    }
                    if (videos != null)
                    {
                        foreach (var video in videos)
                        {
                            try
                            {
                                db.Insert(new Video() { VideoID = video.VideoID, description = video.description, FileName = video.FileName, FilePath = video.FilePath, teacher_full_names = video.teacher, SubjectId = subject.Id });
                            }
                            catch
                            {

                            }
                        }
                    }
                    if (files != null)
                    {
                        foreach (var file in files)
                        {
                            try
                            {
                                db.Insert(new Attachment() { AttachmentID = file.AttachmentID, FileName = file.FileName, FilePath = file.FilePath, TopicID = 0, SubjectId = subject.Id, AssignmentID = 0 });
                            }
                            catch
                            {

                            }
                        }
                    }
                    if (assignments != null)
                    {
                        foreach (var assignment in assignments)
                        {
                            Assignment work = new Assignment(assignment.id, subject.Id, assignment.title, assignment.description, assignment.teacher);
                            db.Update(work);
                            List<AttachmentObservable> assignmentfiles = assignment.Files;
                            List<AttachmentObservable> oldfiles = OldGetFiles(0, assignment.id);
                            List<AttachmentObservable> newfiles = GetNewFiles(assignmentfiles, oldfiles);
                            if (newfiles == null) { }
                            else
                            {
                                foreach (var file in newfiles)
                                {
                                    try
                                    {
                                        db.Insert(new Attachment() { AttachmentID = file.AttachmentID, FileName = file.FileName, FilePath = file.FilePath, TopicID = 0, SubjectId = 0, AssignmentID = assignment.id });
                                    }
                                    catch
                                    {

                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch
            {

            }
        }
        private static List<AttachmentObservable> OldGetFiles(int topicID, int assignmentID)
        {
            List<AttachmentObservable> attachments = new List<AttachmentObservable>();
            AttachmentObservable attachment = null;

            if (topicID == 0 && assignmentID > 0)
            {
                using (var db = new SQLite.SQLiteConnection(Constants.dbPath))
                {
                    var query = (db.Table<Attachment>().Where(c => c.AssignmentID == assignmentID));
                    foreach (var _title in query)
                    {
                        attachment = new AttachmentObservable(_title.AttachmentID, _title.FilePath, _title.FileName);
                        attachments.Add(attachment);
                    }
                }
            }

            if (topicID > 0 && assignmentID == 0)
            {
                using (var db = new SQLite.SQLiteConnection(Constants.dbPath))
                {
                    var query = (db.Table<Attachment>().Where(c => c.TopicID == topicID));
                    foreach (var _title in query)
                    {
                        attachment = new AttachmentObservable(_title.AttachmentID, _title.FilePath, _title.FileName);
                        attachments.Add(attachment);
                    }
                }
            }
            return attachments;
        }
        public static List<AttachmentObservable> GetNewFiles(List<AttachmentObservable> newFiles, List<AttachmentObservable> oldFiles)
        {
            List<AttachmentObservable> files = new List<AttachmentObservable>();
            AttachmentObservable temp = new AttachmentObservable();
            bool found = false;
            bool something = false;

            foreach (var nfile in newFiles)
            {
                foreach (var ofile in oldFiles)
                {

                    if (nfile.AttachmentID == ofile.AttachmentID)
                    {
                        found = true;
                    }
                    else
                    {
                        temp = nfile;
                    }
                }

                if (found == false)
                {
                    files.Add(temp);
                    something = true;
                }
                found = false;
            }
            if (something == false)
            {
                files = null;
            }
            return files;
        }
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
        public static List<Subject> SelectAllSubjects()
        {
            List<Subject> subjects = new List<Subject>();
            List<Subject> nullSubject = null;
            int count = 0;
            using (var db = new SQLite.SQLiteConnection(Constants.dbPath))
            {
                var query = (db.Table<Subject>().ToList());
                subjects = query;
                count = query.Count;
            }
            if (count > 0)
                return subjects;
            else
                return nullSubject;
        }
        public static LoginStatus Notification(JsonObject loginObject)
        {
            LoginStatus user = new LoginStatus();
            foreach (var log in loginObject.Keys)
            {
                IJsonValue val;
                if (!loginObject.TryGetValue(log, out val))
                    continue;
                switch (log)
                {
                    case "statusCode":
                        user.statusCode = val.GetString();
                        break;
                    case "statusDescription":
                        user.statusDescription = val.GetString();
                        break;
                }
            }
            return user;
        }
        public static SchoolObservable GetSchool(JsonObject loginObject)
        {
            SchoolObservable user = new SchoolObservable();
            foreach (var log in loginObject.Keys)
            {
                IJsonValue val;
                if (!loginObject.TryGetValue(log, out val))
                    continue;
                switch (log)
                {
                    case "school_id":
                        user.SchoolId = (int)val.GetNumber();
                        break;
                    case "school":
                        user.SchoolName = val.GetString();
                        break;
                    case "school_logo":
                        user.ImagePath = Constants.BaseUri + val.GetString();
                        break;
                }
            }
            return user;
        }
        public static string GetUsername(JsonObject loginObject)
        {
            string user = "";
            foreach (var log in loginObject.Keys)
            {
                IJsonValue val;
                if (!loginObject.TryGetValue(log, out val))
                    continue;
                switch (log)
                {
                    case "name":
                        user = val.GetString();
                        break;
                }
            }
            return user;
        }
        public static SubjectObservable GetSubject(JsonArray SubjectsArray, int Sub_id, JsonArray NotesArray, JsonArray VideosArray, JsonArray AssignmentArray, JsonArray FilesArray)
        {
            SubjectObservable subject = new SubjectObservable();
            int temp = 0;
            foreach (var item in SubjectsArray)
            {
                var obj = item.GetObject();
                foreach (var key in obj.Keys)
                {
                    IJsonValue val;
                    if (!obj.TryGetValue(key, out val))
                        continue;
                    switch (key)
                    {
                        case "id":
                            temp = (int)val.GetNumber();
                            break;
                        case "name":
                            if (temp == Sub_id)
                            {
                                subject.Id = Sub_id;
                                subject.name = val.GetString();
                                subject.thumb = "ms-appx:///Assets/Course/course.jpg";
                                var noteslist = (from i in NotesArray select i.GetObject()).ToList();
                                subject.topics = GetTopics(noteslist);
                                var videoslist = (from i in VideosArray select i.GetObject()).ToList();
                                subject.videos = GetVideos(videoslist);
                                var fileslist = (from i in FilesArray select i.GetObject()).ToList();
                                subject.files = GetFiles(fileslist);
                                var assignmentlist = (from i in AssignmentArray select i.GetObject()).ToList();
                                subject.assignments = GetAssignments(assignmentlist);
                            }
                            break;
                    }
                }
            }
            return subject;
        }
        public static List<int> SubjectIds(JsonArray SubjectsArray)
        {
            int id;
            List<int> ids = new List<int>();
            foreach (var item in SubjectsArray)
            {
                var obj = item.GetObject();
                foreach (var key in obj.Keys)
                {
                    IJsonValue val;
                    if (!obj.TryGetValue(key, out val))
                        continue;
                    switch (key)
                    {
                        case "id":
                            id = (int)val.GetNumber();
                            ids.Add(id);
                            break;
                    }
                }
            }
            return ids;
        }
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
        public static List<User> SelectAllUsers()
        {
            List<User> users = new List<User>();
            List<User> nullUser = null;
            int count = 0;
            using (var db = new SQLite.SQLiteConnection(Constants.dbPath))
            {
                var query = (db.Table<User>().ToList());
                users = query;
                count = query.Count;
            }
            if (count > 0)
                return users;
            else
                return nullUser;
        }
        public static List<string> oldSubjects()
        {
            List<Subject> subjects = SelectAllSubjects();
            List<string> subs = new List<string>();
            List<string> temp = null;
            if (subjects == null)
            {
                return temp;
            }
            else
            {
                foreach (Subject subject in subjects)
                {
                    string sub = subject.name;
                    subs.Add(sub);
                }
                return subs;
            }
        }
        public static List<SubjectObservable> new_subjects(List<SubjectObservable> Gotsubjects)
        {
            List<string> oldsubs = oldSubjects();
            List<string> subjects = new List<string>();
            List<string> newsubs = new List<string>();
            int x = 0;
            int y = 0;
            List<SubjectObservable> final = new List<SubjectObservable>();
            foreach (var subject in Gotsubjects)
            {
                string sub = subject.name;
                subjects.Add(sub);
            }
            foreach (string newsubject in subjects)
            {
                x++;
                foreach (string oldsubject in oldsubs)
                {
                    string sub;
                    if (oldsubject.Equals(newsubject))
                    {
                        y++;
                    }
                    else
                    {
                        sub = newsubject;
                        newsubs.Add(sub);
                    }
                }
            }
            if (x == y)
            {
                return null;
            }
            else
            {
                foreach (var sub in Gotsubjects)
                {
                    foreach (var substring in newsubs)
                    {
                        if (sub.name.Equals(substring))
                        {
                            final.Add(sub);
                        }
                    }
                }
                return final;
            }
        }
        public static List<SubjectObservable> UpdateableSubjects(List<SubjectObservable> oldSubjects, List<SubjectObservable> newSubjects)
        {
            List<SubjectObservable> final = new List<SubjectObservable>();
            List<TopicObservable> TopicsTemp = new List<TopicObservable>();
            List<AssignmentObservable> AssignmentsTemp = new List<AssignmentObservable>();
            List<VideoObservable> VideosTemp = new List<VideoObservable>();
            List<AttachmentObservable> FileTemp = new List<AttachmentObservable>();
            SubjectObservable Temp = new SubjectObservable();
            bool got = false;
            foreach (var oldsubject in oldSubjects)
            {
                foreach (var newsubject in newSubjects)
                {
                    if (oldsubject.Id == newsubject.Id)
                    {
                        TopicsTemp = UpdateableTopics(oldsubject.topics, newsubject.topics);
                        AssignmentsTemp = UpdateableAssignments(oldsubject.assignments, newsubject.assignments);
                        VideosTemp = GetNewVideos(newsubject.videos, oldsubject.videos);
                        FileTemp = GetNewFiles(newsubject.files, oldsubject.files);

                        if (TopicsTemp == null && AssignmentsTemp == null && VideosTemp == null && FileTemp == null)
                        {
                        }
                        else
                        {
                            Temp = new SubjectObservable(oldsubject.Id, newsubject.name, oldsubject.thumb, TopicsTemp, AssignmentsTemp, VideosTemp, FileTemp);
                            final.Add(Temp);
                            got = true;
                        }
                    }
                }
            }

            if (got == false)
            {
                return null;
            }
            else
            {
                return final;
            }
        }
        public static List<SubjectObservable> UpdateableSubjectsTopics(List<SubjectObservable> oldSubjects, List<SubjectObservable> newSubjects)
        {
            List<SubjectObservable> final = new List<SubjectObservable>();
            SubjectObservable Temp = new SubjectObservable();
            bool got = false;
            foreach (var oldsubject in oldSubjects)
            {
                foreach (var newsubject in newSubjects)
                {
                    if (oldsubject.Id == newsubject.Id)
                    {
                        var newtopics = GetNewTopics(newsubject.topics, oldsubject.topics);
                        if (newtopics != null)
                        {
                            Temp = new SubjectObservable(oldsubject.Id, newsubject.name, oldsubject.thumb, newtopics, null, null, null);
                            final.Add(Temp);
                            got = true;
                        }
                    }
                }
            }

            if (got == false)
            {
                return null;
            }
            else
            {
                return final;
            }
        }
        public static List<int> newIds(List<int> oldIds, List<int> GotIDs)
        {
            List<int> final = new List<int>();
            int temp = new int();
            bool found = false;
            bool something = false;
            foreach (var nId in GotIDs)
            {
                foreach (var oId in oldIds)
                {
                    if (nId == oId)
                    {
                        found = true;
                    }
                    else
                    {
                        temp = nId;
                    }
                }
                if (found == false)
                {
                    final.Add(temp);
                    something = true;
                }
                found = false;
            }
            if (something == false)
            {
                final = null;
            }
            return final;
        }
        public static List<int> oldIds(List<int> oldIds, List<int> GotIDs)
        {
            List<int> final = new List<int>();
            int temp = new int();
            bool found = false;
            bool something = false;
            foreach (var nId in GotIDs)
            {
                foreach (var oId in oldIds)
                {
                    if (nId == oId)
                    {
                        temp = nId;
                        found = true;
                    }
                    else
                    {
                        ;
                    }
                }
                if (found == true)
                {
                    final.Add(temp);
                    something = true;
                }
                found = false;
            }
            if (something == false)
            {
                final = null;
            }
            return final;
        }
        public static List<int> SubjectIdsConvert(List<string> subjectIDString)
        {
            List<int> numbers = new List<int>();
            foreach (var id in subjectIDString)
            {
                int number = Int32.Parse(id);
                numbers.Add(number);
            }
            return numbers;
        }
        private static List<AttachmentObservable> AllFiles(List<JsonObject> AllObjects)
        {
            List<AttachmentObservable> Files = new List<AttachmentObservable>();
            foreach (var SingleObject in AllObjects)
            {
                AttachmentObservable file = SingleFile(SingleObject);
                Files.Add(file);
            }
            return Files;
        }
        private static AttachmentObservable SingleFile(JsonObject obj)
        {
            AttachmentObservable attachment = new AttachmentObservable();
            foreach (var key in obj.Keys)
            {
                IJsonValue val;
                if (!obj.TryGetValue(key, out val))
                    continue;
                switch (key)
                {
                    case "id":
                        attachment.AttachmentID = (int)val.GetNumber();
                        break;
                    case "name":
                        attachment.FileName = val.GetString();
                        break;
                    case "absolute_uri":
                        attachment.FilePath = val.GetString();
                        break;
                }
            }
            return attachment;
        }
        private static string Teacher(JsonObject obj)
        {
            string teacher_names = "";
            foreach (var teacher in obj.Keys)
            {
                IJsonValue teacherVal;
                if (!obj.TryGetValue(teacher, out teacherVal))
                    continue;
                switch (teacher)
                {
                    case "full_name":
                        teacher_names = teacherVal.GetString();
                        break;
                }
            }
            return teacher_names;
        }
        private static List<TopicObservable> GetTopics(List<JsonObject> AllObjects)
        {
            List<TopicObservable> Topics = new List<TopicObservable>();
            foreach (var SingleObject in AllObjects)
            {
                TopicObservable topic = SingleTopic(SingleObject);
                Topics.Add(topic);
            }
            return Topics;
        }
        private static TopicObservable SingleTopic(JsonObject obj)
        {
            TopicObservable topic = new TopicObservable();
            foreach (var key in obj.Keys)
            {
                IJsonValue val;
                if (!obj.TryGetValue(key, out val))
                    continue;
                switch (key)
                {
                    case "id":
                        topic.TopicID = (int)val.GetNumber();
                        break;
                    case "title":
                        topic.TopicTitle = val.GetString();
                        break;
                    case "updated_at":
                        topic.Updated_at = val.GetString();
                        break;
                    case "body":
                        topic.body = val.GetString();
                        break;
                    case "folder_name":
                        topic.folder_name = val.GetString();
                        break;
                    case "folder_id":
                        topic.folder_id = (int)val.GetNumber();
                        break;
                    case "teacher":
                        var teacherObject = val.GetObject();
                        topic.teacher = Teacher(teacherObject);
                        break;
                    case "attachments":
                        var attachmentArray = val.GetArray();
                        var list = (from i in attachmentArray select i.GetObject()).ToList();
                        topic.Files = AllFiles(list);
                        break;
                }
            }
            return topic;
        }
        private static List<VideoObservable> GetVideos(List<JsonObject> AllObjects)
        {
            List<VideoObservable> videos = new List<VideoObservable>();
            foreach (var SingleObject in AllObjects)
            {
                VideoObservable video = SingleVideo(SingleObject);
                videos.Add(video);
            }
            return videos;
        }
        private static VideoObservable SingleVideo(JsonObject obj)
        {
            VideoObservable video = new VideoObservable();
            foreach (var key in obj.Keys)
            {
                IJsonValue val;
                if (!obj.TryGetValue(key, out val))
                    continue;
                switch (key)
                {
                    case "id":
                        video.VideoID = (int)val.GetNumber();
                        break;
                    case "title":
                        video.FileName = val.GetString();
                        break;
                    case "link":
                        video.FilePath = val.GetString();
                        break;
                    case "description":
                        video.description = val.GetString();
                        break;
                    case "teacher":
                        var teacherObject = val.GetObject();
                        video.teacher = Teacher(teacherObject);
                        break;
                }
            }
            return video;
        }
        private static List<AssignmentObservable> GetAssignments(List<JsonObject> AllObjects)
        {
            List<AssignmentObservable> Assignments = new List<AssignmentObservable>();
            foreach (var SingleObject in AllObjects)
            {
                AssignmentObservable Assignment = SingleAssignment(SingleObject);
                Assignments.Add(Assignment);
            }
            return Assignments;
        }
        private static AssignmentObservable SingleAssignment(JsonObject obj)
        {
            AssignmentObservable assignment = new AssignmentObservable();
            foreach (var key in obj.Keys)
            {
                IJsonValue val;
                if (!obj.TryGetValue(key, out val))
                    continue;
                switch (key)
                {
                    case "id":
                        assignment.id = (int)val.GetNumber();
                        break;
                    case "title":
                        assignment.title = val.GetString();
                        break;
                    case "description":
                        assignment.description = val.GetString();
                        break;
                    case "teacher":
                        var teacherObject = val.GetObject();
                        assignment.teacher = Teacher(teacherObject);
                        break;
                    case "attachments":
                        var attachmentArray = val.GetArray();
                        var list = (from i in attachmentArray select i.GetObject()).ToList();
                        assignment.Files = AllFiles(list);
                        break;
                }
            }
            return assignment;
        }
        private static List<AttachmentObservable> GetFiles(List<JsonObject> AllObjects)
        {
            List<AttachmentObservable> files = new List<AttachmentObservable>();
            foreach (var SingleObject in AllObjects)
            {
                AttachmentObservable file = AFile(SingleObject);
                files.Add(file);
            }
            return files;
        }
        private static AttachmentObservable AFile(JsonObject obj)
        {
            AttachmentObservable attachment = new AttachmentObservable();
            foreach (var key in obj.Keys)
            {
                IJsonValue val;
                if (!obj.TryGetValue(key, out val))
                    continue;
                switch (key)
                {
                    case "id":
                        attachment.AttachmentID = (int)val.GetNumber();
                        break;
                    case "name":
                        attachment.FileName = val.GetString();
                        break;
                    case "url":
                        attachment.FilePath = val.GetString();
                        break;
                }
            }
            return attachment;
        }
        private static TopicObservable TopicChange(TopicObservable newTopic, TopicObservable oldTopic)
        {
            string newNotes = newTopic.body;
            string oldNotes = oldTopic.body;
            List<AttachmentObservable> newFiles = newTopic.Files;
            List<AttachmentObservable> oldFiles = oldTopic.Files;
            List<AttachmentObservable> Files = new List<AttachmentObservable>();
            TopicObservable Topic = new TopicObservable();
            if (newNotes.Equals(oldNotes))
            {
                Files = GetNewFiles(newFiles, oldFiles);
                if (Files == null)
                {
                    Topic = null;
                }
                else
                {
                    Topic = new TopicObservable(oldTopic.TopicID, null, oldTopic.TopicTitle, Files, oldTopic.teacher, newTopic.Updated_at, oldTopic.folder_id, oldTopic.folder_name);
                }
            }
            else
            {
                Files = GetNewFiles(newFiles, oldFiles);
                if (Files == null)
                {
                    Topic = new TopicObservable(oldTopic.TopicID, newTopic.body, oldTopic.TopicTitle, null, oldTopic.teacher, newTopic.Updated_at, oldTopic.folder_id, oldTopic.folder_name);
                }
                else
                {
                    Topic = new TopicObservable(oldTopic.TopicID, newTopic.body, oldTopic.TopicTitle, Files, oldTopic.teacher, newTopic.Updated_at, oldTopic.folder_id, oldTopic.folder_name);
                }
            }
            return Topic;
        }
        private static List<TopicObservable> UpdateableTopics(List<TopicObservable> oldTopics, List<TopicObservable> newTopics)
        {
            List<TopicObservable> final = new List<TopicObservable>();
            List<TopicObservable> ntopics = new List<TopicObservable>();
            List<TopicObservable> otopics = new List<TopicObservable>();
            TopicObservable temp = new TopicObservable();
            bool found = false;
            foreach (var oldtopic in oldTopics)
            {
                foreach (var newtopic in newTopics)
                {
                    if (oldtopic.TopicID == newtopic.TopicID)
                    {
                        temp = TopicChange(newtopic, oldtopic);
                        if (temp == null) { }
                        else
                        {
                            final.Add(newtopic);
                            found = true;
                        }
                    }
                }
            }
            if (found == true)
            {
                return final;
            }
            else
            {
                return null;
            }
        }
        private static List<AssignmentObservable> UpdateableAssignments(List<AssignmentObservable> oldAssignments, List<AssignmentObservable> newAssignments)
        {
            List<AssignmentObservable> final = new List<AssignmentObservable>();
            List<AssignmentObservable> nassignments = new List<AssignmentObservable>();
            List<AssignmentObservable> oassignments = new List<AssignmentObservable>();
            AssignmentObservable temp = new AssignmentObservable();
            bool found = false;
            foreach (var oldassignment in oldAssignments)
            {
                foreach (var newassignment in newAssignments)
                {
                    if (oldassignment.id == newassignment.id)
                    {
                        temp = AssignmentChange(newassignment, oldassignment);
                        if (temp == null) { }
                        else
                        {
                            final.Add(newassignment);
                            found = true;
                        }
                    }
                }
            }
            if (found == true)
            {
                return final;
            }
            else
            {
                return null;
            }
        }
        private static AssignmentObservable AssignmentChange(AssignmentObservable newAssignment, AssignmentObservable oldAssignment)
        {
            string newNotes = newAssignment.description;
            string oldNotes = oldAssignment.description;
            List<AttachmentObservable> newFiles = newAssignment.Files;
            List<AttachmentObservable> oldFiles = oldAssignment.Files;
            List<AttachmentObservable> Files = new List<AttachmentObservable>();
            AssignmentObservable Assignment = new AssignmentObservable();
            if (newNotes.Equals(oldNotes))
            {
                Files = GetNewFiles(newFiles, oldFiles);
                if (Files == null)
                {
                    Assignment = null;
                }
                else
                {
                    Assignment = new AssignmentObservable(oldAssignment.id, oldAssignment.title, null, oldAssignment.teacher, Files);
                }
            }
            else
            {
                Files = GetNewFiles(newFiles, oldFiles);
                if (Files == null)
                {
                    Assignment = new AssignmentObservable(oldAssignment.id, oldAssignment.title, newAssignment.description, oldAssignment.teacher, null);
                }
                else
                {
                    Assignment = new AssignmentObservable(oldAssignment.id, oldAssignment.title, newAssignment.description, oldAssignment.teacher, Files);
                }
            }
            return Assignment;
        }
        private static List<VideoObservable> GetNewVideos(List<VideoObservable> newVideos, List<VideoObservable> oldVideos)
        {
            List<VideoObservable> files = new List<VideoObservable>();
            VideoObservable temp = new VideoObservable();
            bool found = false;
            bool something = false;

            foreach (var nvideo in newVideos)
            {
                foreach (var ovideo in oldVideos)
                {

                    if (ovideo.VideoID == nvideo.VideoID)
                    {
                        found = true;
                    }
                    else
                    {
                        temp = nvideo;
                    }
                }

                if (found == false)
                {
                    files.Add(temp);
                    something = true;
                }
                found = false;
            }
            if (something == false)
            {
                files = null;
            }
            return files;
        }
        public static async Task DeleteTemporaryFiles()
        {
            StorageFolder tempFolder = ApplicationData.Current.TemporaryFolder;
            IReadOnlyList<StorageFile> images = await tempFolder.GetFilesAsync();
            foreach (var image in images)
            {
                await image.DeleteAsync();
            }
        }
        private static List<TopicObservable> GetNewTopics(List<TopicObservable> newTopics, List<TopicObservable> oldTopics)
        {
            List<TopicObservable> files = new List<TopicObservable>();
            TopicObservable temp = new TopicObservable();
            bool found = false;
            bool something = false;

            foreach (var ntopic in newTopics)
            {
                foreach (var otopic in oldTopics)
                {

                    if (otopic.TopicID == ntopic.TopicID)
                    {
                        found = true;
                    }
                    else
                    {
                        temp = ntopic;
                    }
                }

                if (found == false)
                {
                    files.Add(temp);
                    something = true;
                }
                found = false;
            }
            if (something == false)
            {
                files = null;
            }
            return files;
        }
        public static string UserSubjects(string string_input)
        {
            char[] delimiter = { '.' };
            string final = "";
            List<int> subjectids = new List<int>();
            string[] SplitSubjectId = string_input.Split(delimiter);
            List<string> SubjectIdList = SplitSubjectId.ToList();
            subjectids = SubjectIdsConvert(SubjectIdList);
            List<int> finalids = RemoveRepitions(subjectids);
            foreach (var id in finalids)
            {
                if (final.Equals(""))
                {
                    final = "" + id;
                }
                else
                {
                    final = final + "." + id;
                }
            };
            return final;
        }
        private static List<int> RemoveRepitions(List<int> numbers)
        {
            List<int> final = new List<int>();
            List<int> compare = numbers;
            
            bool done = false;
            foreach (var number in numbers)
            {              
                foreach (var second in compare)
                {
                    if (number == second)
                    {                       
                        if (final.Count > 0)
                        {
                            foreach (var test in final)
                            {
                                if (test == second)
                                {
                                    done = true;
                                }
                            }
                            if (done == false)
                            {
                                final.Add(second);
                            }
                        }
                        else
                        {
                            final.Add(number);
                        }
                        done = false;
                    }
                }
            }
            return final;
        }
        public static List<int> SubjectIdsForUser(string username)
        {
            char[] delimiter = { '.' };
            List<int> subjectids = new List<int>();
            using (var db = new SQLite.SQLiteConnection(Constants.dbPath))
            {
                var query = (db.Table<User>().Where(c => c.e_mail == username)).Single();
                string[] SplitSubjectId = query.subjects.Split(delimiter);
                List<string> SubjectIdList = SplitSubjectId.ToList();
                subjectids = SubjectIdsConvert(SubjectIdList);
            }
            return subjectids;
        }
        public static LibraryObservable GetLibrary(JsonArray LibraryArray, int id)
        {
            LibraryObservable library = new LibraryObservable();
            var CategoryList = (from i in LibraryArray select i.GetObject()).ToList();
            library.library_id = id;
            library.categories = GetLibraryCategories(CategoryList, id);
            return library;
        }
        private static List<Library_CategoryObservable> GetLibraryCategories(List<JsonObject> AllObjects, int library_id)
        {
            List<Library_CategoryObservable> categories = new List<Library_CategoryObservable>();
            foreach (var SingleObject in AllObjects)
            {
                Library_CategoryObservable category = LibraryCategory(SingleObject, library_id);
                categories.Add(category);
            }
            return categories;
        }
        private static Library_CategoryObservable LibraryCategory(JsonObject obj, int Library_id)
        {
            List<BookObservable> tempbooks = new List<BookObservable>();
            Library_CategoryObservable category = new Library_CategoryObservable();
            foreach (var key in obj.Keys)
            {
                IJsonValue val;
                if (!obj.TryGetValue(key, out val))
                    continue;
                switch (key)
                {
                    case "id":
                        category.category_id = (int)val.GetNumber();
                        int cat_id = (int)val.GetNumber();
                        break;
                    case "name":
                        category.category_name = val.GetString();
                        break;
                    case "book_count":
                        category.book_count = (int)val.GetNumber();
                        break;
                    case "books":
                        var BooksArray = val.GetArray();
                        var BookList = (from i in BooksArray select i.GetObject()).ToList();
                        category.category_books = GetBooks(BookList, Library_id, category.category_id, category.category_name);
                        break;
                }
            }
            return category;
        }
        public static async void InsertLibAsync(LibraryObservable lib)
        {
            List<Library_CategoryObservable> categories = lib.categories;
            var db = new SQLite.SQLiteConnection(Constants.dbPath);
            if (categories == null)
            {

            }
            else
            {
                foreach (var category in categories)
                {
                    List<BookObservable> books = category.category_books;
                    foreach (var book in books)
                    {
                        try
                        {
                            await ImageDownloader(book.book_title, book.thumb_url);
                            string newPath = imagePath(book.book_title + Constants.PNG_extension);

                            //Insert here book if success here
                            db.Insert(new Book()
                            {
                                Book_id = book.book_id,
                                Book_author = book.book_author,
                                Book_description = book.book_description,
                                Book_title = book.book_title,
                                Category_id = category.category_id,
                                Category_name = category.category_name,
                                file_size = book.file_size,
                                file_url = book.file_url,
                                Library_id = lib.library_id,
                                thumb_url = newPath,
                                updated_at = book.updated_at
                            });

                        }
                        catch
                        {
                            db.Insert(new Book()
                            {
                                Book_id = book.book_id,
                                Book_author = book.book_author,
                                Book_description = book.book_description,
                                Book_title = book.book_title,
                                Category_id = category.category_id,
                                Category_name = category.category_name,
                                file_size = book.file_size,
                                file_url = book.file_url,
                                Library_id = lib.library_id,
                                thumb_url = book.thumb_url,
                                updated_at = book.updated_at
                            });
                        }
                    }
                }
            }
        }
        public static async void UpdateLibAsync(LibraryObservable lib)
        {
            List<Library_CategoryObservable> categories = lib.categories;
            var db = new SQLite.SQLiteConnection(Constants.dbPath);
            if (categories == null)
            {

            }
            else
            {
                foreach (var category in categories)
                {
                    List<BookObservable> books = category.category_books;
                    foreach (var book in books)
                    {
                        await ImageDownloader(book.book_title, book.thumb_url);
                        string newPath = imagePath(book.book_title + Constants.PNG_extension);
                        db.Insert(new Book()
                        {
                            Book_id = book.book_id,
                            Book_author = book.book_author,
                            Book_description = book.book_description,
                            Book_title = book.book_title,
                            Category_id = category.category_id
                            ,
                            Category_name = category.category_name,
                            file_size = book.file_size,
                            file_url = book.file_url,
                            Library_id = lib.library_id,
                            thumb_url = newPath,
                            updated_at = book.updated_at
                        });
                    }
                }
            }
        }
        private static List<BookObservable> GetBooks(List<JsonObject> AllObjects, int lib_id, int category_id, string category_name)
        {
            List<BookObservable> books = new List<BookObservable>();
            foreach (var SingleObject in AllObjects)
            {
                BookObservable Book = SingleBook(SingleObject, lib_id, category_id, category_name);
                books.Add(Book);
            }
            return books;
        }
        private static BookObservable SingleBook(JsonObject obj, int lib_id, int category_id, string category_name)
        {
            BookObservable book = new BookObservable();
            book.Category_id = category_id;
            book.Library_id = lib_id;
            book.Category_name = category_name;
            foreach (var key in obj.Keys)
            {
                IJsonValue val;
                if (!obj.TryGetValue(key, out val))
                    continue;
                switch (key)
                {
                    case "id":
                        book.book_id = (int)val.GetNumber();
                        break;
                    case "title":
                        book.book_title = val.GetString();
                        break;
                    case "author":
                        book.book_author = val.GetString();
                        break;
                    case "description":
                        book.book_description = val.GetString();
                        break;
                    case "thumb_url":
                        book.thumb_url = val.GetString();
                        break;
                    case "file_url":
                        book.file_url = val.GetString();
                        break;
                    case "file_size":
                        book.file_size = (int)val.GetNumber();
                        break;
                }
            }
            return book;
        }
        public static LibraryObservable GetLibrary(int school_id)
        {
            LibraryObservable library = new LibraryObservable();
            List<Book> books = new List<Book>();
            List<Library_CategoryObservable> categories = new List<Library_CategoryObservable>();
            int count;
            using (var db = new SQLite.SQLiteConnection(Constants.dbPath))
            {
                var query = db.Table<Book>().Where(c => c.Library_id == school_id);
                books = query.ToList();
                count = books.Count;
            }
            foreach (var book in books)
            {
                if (count > 0)
                { //If Starts here
                    Library_CategoryObservable lib_category = new Library_CategoryObservable();
                    List<BookObservable> Categorybooks = new List<BookObservable>();
                    bool finished = false;
                    lib_category.category_id = book.Category_id;
                    lib_category.category_name = book.Category_name;
                    foreach (var comparisonBook in books)
                    {
                        finished = false;
                        if (lib_category.category_id == comparisonBook.Category_id)
                        {
                            if (book.Book_id == comparisonBook.Book_id)
                            {
                                if (Categorybooks.Count > 0)
                                {
                                    finished = true;
                                }
                                else
                                {
                                    BookObservable newbook = new BookObservable(book.Book_id, book.Book_title, book.Book_author, book.Book_description,
                                        book.updated_at, book.thumb_url, book.file_url, book.file_size, book.Library_id, book.Category_id, book.Category_name);
                                    Categorybooks.Add(newbook);
                                    count = count - 1; //Reduce
                                }
                            }
                            else
                            {
                                if (Categorybooks.Count > 0)
                                {
                                    foreach (var CheckBook in Categorybooks)
                                    {
                                        if (comparisonBook.Book_id == CheckBook.book_id)
                                        {
                                            finished = true;
                                        }
                                    }
                                }
                                if (finished == false)
                                {
                                    BookObservable newbook = new BookObservable(comparisonBook.Book_id, comparisonBook.Book_title, comparisonBook.Book_author,
                                        comparisonBook.Book_description, comparisonBook.updated_at, comparisonBook.thumb_url, comparisonBook.file_url, comparisonBook.file_size,
                                        comparisonBook.Library_id, comparisonBook.Category_id, comparisonBook.Category_name);
                                    Categorybooks.Add(newbook);
                                    count = count - 1; //reduce
                                }
                            }
                        }
                        lib_category.category_books = Categorybooks;
                        lib_category.book_count = Categorybooks.Count;
                    }
                    categories.Add(lib_category);
                } //if ends here
            }
            library.library_id = school_id;
            library.categories = categories;
            return library;
        }
        //New methods to run update of library
        public static Library_CategoryObservable Category_Update(List<BookObservable> oldbooks, List<BookObservable> newbooks)
        {
            Library_CategoryObservable category = new Library_CategoryObservable();
            List<BookObservable> books = new List<BookObservable>();
            bool found = false;
            bool something = false;
            BookObservable temp = new BookObservable();
            int x = 0;
            foreach (var newbook in newbooks)
            {
                foreach (var oldbook in oldbooks)
                {
                    if (oldbook.book_id == newbook.book_id)
                    {
                        found = true;
                    }
                    else
                    {
                        temp = newbook;
                    }
                }
                if (found == false)
                {
                    books.Add(temp);
                    something = true;
                    category.category_id = temp.Category_id;
                    category.category_name = temp.Category_name;
                    x++;
                }
                found = false;
            }
            if (something == false)
            {
                category = null;
            }
            else
            {
                category.category_books = books;
                category.book_count = x;
            }
            return category;
        }
        public static List<Library_CategoryObservable> Categories_Update(List<Library_CategoryObservable> oldcategories, List<Library_CategoryObservable> newcategories)
        {
            List<Library_CategoryObservable> final = new List<Library_CategoryObservable>();
            foreach (var newcategory in newcategories)
            {
                foreach (var oldcategory in oldcategories)
                {
                    if (newcategory.category_id == oldcategory.category_id)
                    {
                        Library_CategoryObservable category = Category_Update(oldcategory.category_books, newcategory.category_books);
                        if (category != null)
                        {
                            final.Add(category);
                        }
                    }
                }
            }
            return final;
        }
        public static LibraryObservable CompareLibraries(LibraryObservable oldlib, LibraryObservable newlib)
        {
            bool found = false;
            bool something = false;
            Library_CategoryObservable temp = new Library_CategoryObservable();
            List<Library_CategoryObservable> final_categories = new List<Library_CategoryObservable>();
            LibraryObservable lib = new LibraryObservable();
            lib.library_id = oldlib.library_id;
            List<Library_CategoryObservable> new_libCategories = new List<Library_CategoryObservable>();
            new_libCategories = newlib.categories;

            List<Library_CategoryObservable> old_libCategories = new List<Library_CategoryObservable>();
            old_libCategories = oldlib.categories;

            foreach (var new_libCategory in new_libCategories)
            {
                foreach (var old_libCategory in old_libCategories)
                {
                    if (old_libCategory.category_id == new_libCategory.category_id)
                    {
                        found = true;
                    }
                    else
                    {
                        temp = new_libCategory;
                    }
                }
                if (found == false)
                {
                    final_categories.Add(temp);
                }
                found = false;
            }
            if (something == false)
            {
                lib = null;
            }
            else
            {
                lib.categories = final_categories;
            }
            return lib;
        }
        public static Library_CategoryObservable Category_Update_Removal(List<BookObservable> oldbooks, List<BookObservable> newbooks)
        {
            Library_CategoryObservable category = new Library_CategoryObservable();
            List<BookObservable> books = new List<BookObservable>();
            bool found = false;
            bool something = false;
            BookObservable temp = new BookObservable();
            int x = 0;
            foreach (var oldbook in oldbooks)
            {
                foreach (var newbook in newbooks)
                {
                    if (oldbook.book_id == newbook.book_id)
                    {
                        found = true;
                    }
                    else
                    {
                        temp = oldbook;
                    }
                }
                if (found == false)
                {
                    books.Add(temp);
                    something = true;
                    category.category_id = temp.Category_id;
                    category.category_name = temp.Category_name;
                    x++;
                }
                found = false;
            }
            if (something == false)
            {
                category = null;
            }
            else
            {
                category.category_books = books;
                category.book_count = x;
            }
            return category;
        }
        public static List<Library_CategoryObservable> Categories_Update_Removal(List<Library_CategoryObservable> oldcategories, List<Library_CategoryObservable> newcategories)
        {
            List<Library_CategoryObservable> final = new List<Library_CategoryObservable>();
            foreach (var newcategory in newcategories)
            {
                foreach (var oldcategory in oldcategories)
                {
                    if (newcategory.category_id == oldcategory.category_id)
                    {
                        Library_CategoryObservable category = Category_Update_Removal(oldcategory.category_books, newcategory.category_books);
                        if (category != null)
                        {
                            final.Add(category);
                        }
                    }
                }
            }
            return final;
        }
        public static LibraryObservable CompareLibraries_Removal(LibraryObservable oldlib, LibraryObservable newlib)
        {
            bool found = false;
            bool something = false;
            Library_CategoryObservable temp = new Library_CategoryObservable();
            List<Library_CategoryObservable> final_categories = new List<Library_CategoryObservable>();
            LibraryObservable lib = new LibraryObservable();
            lib.library_id = oldlib.library_id;
            List<Library_CategoryObservable> new_libCategories = new List<Library_CategoryObservable>();
            new_libCategories = newlib.categories;

            List<Library_CategoryObservable> old_libCategories = new List<Library_CategoryObservable>();
            old_libCategories = oldlib.categories;

            foreach (var new_libCategory in new_libCategories)
            {
                foreach (var old_libCategory in old_libCategories)
                {
                    if (old_libCategory.category_id == new_libCategory.category_id)
                    {
                        found = true;
                    }
                    else
                    {
                        temp = new_libCategory;
                    }
                }
                if (found == false)
                {
                    final_categories.Add(temp);
                }
                found = false;
            }
            if (something == false)
            {
                lib = null;
            }
            else
            {
                lib.categories = final_categories;
            }
            return lib;
        }
    }
}
