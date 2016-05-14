using BrainShare.Common;
using BrainShare.Database;
using BrainShare.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BrainShare.Core
{
    class DatabaseInputTask
    {
        #region Insert Methods
        //Method to insert Subjects into the SQLite Database
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
                      db.Insert(new Subject() { SubjectId = subject.Id, name = subject.name, thumb = subject.thumb });
                        topics = subject.topics;
                        if (topics.Count > 0)
                        {
                            foreach (var topic in topics)
                            {
                                string Updated_notes = NotesTask.NotesChanger(topic.body); //Update the notes
                                db.Insert(new Topic() { TopicID = topic.TopicID, Notes = topic.body, Updated_Notes = Updated_notes, SubjectId = subject.Id, teacher_full_names = topic.teacher, TopicTitle = topic.TopicTitle, Updated_at = topic.Updated_at, Folder_Id = topic.folder_id, Folder_Name = topic.folder_name });
                                files = topic.Files;
                                if (files.Count > 0)
                                {
                                    foreach (var file in files)
                                    {
                                      db.Insert(new Attachment() { AttachmentID = file.AttachmentID, FileName = file.FileName, FilePath = file.FilePath, TopicID = topic.TopicID, SubjectId = 0, AssignmentID = 0 });
                                    }
                                }
                            }
                        }

                        videos = subject.videos;
                        if (videos.Count > 0)
                        {
                            foreach (var video in videos)
                            {
                              db.Insert(new Video() { VideoID = video.VideoID, description = video.description, FileName = video.FileName, FilePath = video.FilePath, teacher_full_names = video.teacher, SubjectId = subject.Id });
                            }
                        }

                        files = subject.files;
                        if (files.Count > 0)
                        {
                            foreach (var file in files)
                            {
                              db.Insert(new Attachment() { AttachmentID = file.AttachmentID, FileName = file.FileName, FilePath = file.FilePath, TopicID = 0, SubjectId = subject.Id, AssignmentID = 0 });
                            }
                        }

                        assignments = subject.assignments;
                        if (assignments.Count > 0)
                        {
                            foreach (var assignment in assignments)
                            {
                                db.Insert(new Assignment() { AssignmentID = assignment.id, description = assignment.description, teacher_full_names = assignment.teacher, title = assignment.title, SubjectId = subject.Id });
                                files = assignment.Files;
                                if (files.Count > 0)
                                {
                                    foreach (var file in files)
                                    {
                                     db.Insert(new Attachment() { AttachmentID = file.AttachmentID, FileName = file.FileName, FilePath = file.FilePath, TopicID = 0, SubjectId = 0, AssignmentID = assignment.id });
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
        //Method to Update Subjects in the SQLite Database
        public static void InsertSubjectsUpdateAsync(List<SubjectObservable> subjects)
        {
            List<TopicObservable> topics = new List<TopicObservable>();
            List<AttachmentObservable> files = new List<AttachmentObservable>();   
                var db = new SQLite.SQLiteConnection(Constants.dbPath);
                foreach (var subject in subjects)
                {
                    topics = subject.topics;
                    if (topics.Count > 0)
                    {
                        foreach (var topic in topics)
                        {
                            db.Insert(new Topic() { TopicID = topic.TopicID, Notes = topic.body, Updated_Notes = NotesTask.NotesChanger(topic.body), SubjectId = subject.Id, teacher_full_names = topic.teacher, TopicTitle = topic.TopicTitle, Updated_at = topic.Updated_at, Folder_Id = topic.folder_id, Folder_Name = topic.folder_name });
                            files = topic.Files;
                            if (files.Count > 0)
                            {
                                foreach (var file in files)
                                {
                                 db.Insert(new Attachment() { AttachmentID = file.AttachmentID, FileName = file.FileName, FilePath = file.FilePath, TopicID = topic.TopicID, SubjectId = 0, AssignmentID = 0 });
                                }
                            }
                        }
                    }
                }           
        }
        //Task to Insert a User into the database
        public static async Task InsertUserAsync(UserObservable user)
        {
            var db = new SQLite.SQLiteConnection(Constants.dbPath);
            List<string> subjectsnames = ModelTask.SubjectNames(user.subjects);
            string ConcatSubjects = ModelTask.JoinedSubjects(subjectsnames);
            SchoolObservable school = user.School;
            db.Insert(new User() { e_mail = user.email, password = user.password, School_id = school.SchoolId, subjects = ConcatSubjects, profileName = user.full_names });
            try
            {
                await CommonTask.ImageDownloader(school.ImagePath, school.SchoolName);
            }
            catch
            {

            }
            string image_extension = ImageTask.imageFormat(school.ImagePath);
            string newPath = ImageTask.imagePath(school.SchoolName + image_extension);
            db.Insert(new School() { SchoolName = school.SchoolName, SchoolBadge = newPath, School_id = school.SchoolId });
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
                            await CommonTask.ImageDownloader(book.thumb_url, book.book_title);
                            string image_extension = ImageTask.imageFormat(book.thumb_url);
                            string newPath = ImageTask.imagePath(book.book_title + image_extension);
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
        #endregion
        #region Update Methods
        //Task to Update User
        public static async Task UpdateUserAsync(UserObservable user)
        {
            var db = new SQLite.SQLiteConnection(Constants.dbPath);
            List<string> subjectsnames = ModelTask.SubjectNames(user.subjects);
            string ConcatSubjects = ModelTask.JoinedSubjects(subjectsnames);
            ConcatSubjects = ModelTask.UserSubjects(ConcatSubjects);
            SchoolObservable school = user.School;
            string image_extension = ImageTask.imageFormat(school.ImagePath);
            string newPath = ImageTask.imagePath(school.SchoolName + image_extension);
            if (school.ImagePath.Equals(newPath)) { } //Checking if image was downloaded
            else
            {
                await CommonTask.ImageDownloader(school.ImagePath, school.SchoolName);
                School sch = new School(school.SchoolId, school.SchoolName, newPath);
                db.Update(sch);
            }

            User userInfo = new User(user.email, user.password, user.full_names, ConcatSubjects, school.SchoolId);
            db.Update(userInfo);
        }
        //Task to Update Subjects
        public static void UpdateSubjectsAsync(List<SubjectObservable> subjects)
        {
            List<TopicObservable> topics = new List<TopicObservable>();
            List<AttachmentObservable> files = new List<AttachmentObservable>();
            List<VideoObservable> videos = new List<VideoObservable>();
            List<AssignmentObservable> assignments = new List<AssignmentObservable>();
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
                            Topic newTopic = new Topic(topic.TopicID, subject.Id, topic.TopicTitle, topic.body, NotesTask.NotesChanger(topic.body), topic.Updated_at, topic.teacher, topic.folder_id, topic.folder_name);
                            db.Update(newTopic);
                            List<AttachmentObservable> topicfiles = topic.Files;
                            List<AttachmentObservable> oldfiles = DatabaseOutputTask.OldGetFiles(topic.TopicID, 0);
                            List<AttachmentObservable> newfiles = ModelTask.GetNewFiles(topicfiles, oldfiles);
                            if (newfiles == null) { }
                            else
                            {
                                foreach (var file in newfiles)
                                {
                                  db.Insert(new Attachment() { AttachmentID = file.AttachmentID, FileName = file.FileName, FilePath = file.FilePath, TopicID = topic.TopicID, SubjectId = 0, AssignmentID = 0 }); 
                                }
                            }
                        }
                    }
                    if (videos != null)
                    {
                        foreach (var video in videos)
                        {
                         db.Insert(new Video() { VideoID = video.VideoID, description = video.description, FileName = video.FileName, FilePath = video.FilePath, teacher_full_names = video.teacher, SubjectId = subject.Id });
                        }
                    }
                    if (files != null)
                    {
                        foreach (var file in files)
                        {
                          db.Insert(new Attachment() { AttachmentID = file.AttachmentID, FileName = file.FileName, FilePath = file.FilePath, TopicID = 0, SubjectId = subject.Id, AssignmentID = 0 });
                        }
                    }
                    if (assignments != null)
                    {
                        foreach (var assignment in assignments)
                        {
                            Assignment work = new Assignment(assignment.id, subject.Id, assignment.title, assignment.description, assignment.teacher);
                            db.Update(work);
                            List<AttachmentObservable> assignmentfiles = assignment.Files;
                            List<AttachmentObservable> oldfiles = DatabaseOutputTask.OldGetFiles(0, assignment.id);
                            List<AttachmentObservable> newfiles = ModelTask.GetNewFiles(assignmentfiles, oldfiles);
                            if (newfiles == null) { }
                            else
                            {
                                foreach (var file in newfiles)
                                {
                                  db.Insert(new Attachment() { AttachmentID = file.AttachmentID, FileName = file.FileName, FilePath = file.FilePath, TopicID = 0, SubjectId = 0, AssignmentID = assignment.id });
                                }
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
                        await CommonTask.ImageDownloader(book.thumb_url, book.book_title);
                        string image_extension = ImageTask.imageFormat(book.thumb_url);
                        string newPath = ImageTask.imagePath(book.book_title + image_extension);
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
        #endregion
    }
}
