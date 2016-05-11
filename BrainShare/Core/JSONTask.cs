using BrainShare.Common;
using BrainShare.Models;
using System.Collections.Generic;
using System.Linq;
using Windows.Data.Json;

namespace BrainShare.Core
{
    class JSONTask
    {
        private static string Teacher(JsonObject obj)
        {
            string teacher_names = string.Empty;
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
        #region Login JSON Methods
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
            string user = string.Empty;
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
        #endregion
        #region Subjects JSON Methods
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
        #endregion
        #region Topics JSON Methods
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
        #endregion
        #region Library JSON Methods
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
        public static LibraryObservable GetLibrary(JsonArray LibraryArray, int id)
        {
            LibraryObservable library = new LibraryObservable();
            var CategoryList = (from i in LibraryArray select i.GetObject()).ToList();
            library.library_id = id;
            library.categories = GetLibraryCategories(CategoryList, id);
            return library;
        }
        #endregion
    }
}
