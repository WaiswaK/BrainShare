﻿using BrainShare.Common;
using BrainShare.Database;
using BrainShare.Models;
using System.Collections.Generic;
using System.Linq;

namespace BrainShare.Core
{
    class DBRetrievalTask
    {
        //Subjects in database
        public static List<Subject> SelectAllSubjects()
        {
            List<Subject> subjects = new List<Subject>();
            List<Subject> nullSubject = null;
            int count = 0;
            using (var db = new SQLite.SQLiteConnection(Constant.dbPath))
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
        //Users in Database
        public static List<User> SelectAllUsers()
        {
            List<User> users = new List<User>();
            List<User> nullUser = null;
            int count = 0;
            using (var db = new SQLite.SQLiteConnection(Constant.dbPath))
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
        //Method to get Subject Ids of a particular User
        public static List<int> SubjectIdsForUser(string username)
        {
            char[] delimiter = { '.' };
            List<int> subjectids = new List<int>();
            using (var db = new SQLite.SQLiteConnection(Constant.dbPath))
            {
                var query = (db.Table<User>().Where(c => c.e_mail == username)).Single();
                string[] SplitSubjectId = query.subjects.Split(delimiter);
                List<string> SubjectIdList = SplitSubjectId.ToList();
                subjectids = ModelTask.SubjectIdsConvert(SubjectIdList);
            }
            return subjectids;
        }
        //Method to get Subject details
        public static SubjectModel GetSubject(int id)
        {
            SubjectModel sub = new SubjectModel();
            using (var db = new SQLite.SQLiteConnection(Constant.dbPath))
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
        public static SchoolModel GetSchool(int school_id)
        {
            SchoolModel school = new SchoolModel();
            using (var db = new SQLite.SQLiteConnection(Constant.dbPath))
            {
                var query = (db.Table<School>().Where(c => c.School_id == school_id)).Single();
                school = new SchoolModel(query.SchoolName, query.SchoolBadge, query.School_id);
            }
            return school;
        }
        private static List<TopicModel> GetTopics(int subId)
        {
            List<TopicModel> topics = new List<TopicModel>();
            TopicModel topic = null;
            using (var db = new SQLite.SQLiteConnection(Constant.dbPath))
            {
                var query = (db.Table<Topic>().Where(c => c.SubjectId == subId));
                foreach (var _topic in query)
                {
                    topic = new TopicModel(_topic.TopicID, _topic.Notes, _topic.Updated_Notes, _topic.TopicTitle, GetFiles(_topic.TopicID, 0, 0), _topic.teacher_full_names, _topic.Updated_at, _topic.Folder_Id, _topic.Folder_Name);
                    topics.Add(topic);
                }
            }
            return topics;
        }
        public static List<AttachmentModel> GetFiles(int id1, int id2, int id3)
        {
            List<AttachmentModel> attachments = new List<AttachmentModel>();
            AttachmentModel attachment = null;
            using (var db = new SQLite.SQLiteConnection(Constant.dbPath))
            {
                if (id1 > 0)
                {
                    var query = (db.Table<Attachment>().Where(c => c.TopicID == id1));
                    foreach (var _file in query)
                    {
                        attachment = new AttachmentModel(_file.AttachmentID, _file.FilePath, _file.FileName);
                        attachments.Add(attachment);
                    }
                }
                if (id2 > 0)
                {
                    var query = (db.Table<Attachment>().Where(c => c.SubjectId == id2));
                    foreach (var _file in query)
                    {
                        attachment = new AttachmentModel(_file.AttachmentID, _file.FilePath, _file.FileName);
                        attachments.Add(attachment);
                    }
                }
                if (id3 > 0)
                {
                    var query = (db.Table<Attachment>().Where(c => c.AssignmentID == id3));
                    foreach (var _file in query)
                    {
                        attachment = new AttachmentModel(_file.AttachmentID, _file.FilePath, _file.FileName);
                        attachments.Add(attachment);
                    }
                }
            }
            return attachments;
        }
        private static List<VideoModel> GetVideos(int Subjectid)
        {
            List<VideoModel> videos = new List<VideoModel>();
            VideoModel video = null;
            using (var db = new SQLite.SQLiteConnection(Constant.dbPath))
            {
                var query = (db.Table<Video>().Where(c => c.SubjectId == Subjectid));
                foreach (var _video in query)
                {
                    video = new VideoModel(_video.VideoID, _video.FilePath, _video.FileName, _video.description, _video.teacher_full_names);
                    videos.Add(video);
                }
            }
            return videos;
        }
        private static List<AssignmentModel> GetAssignments(int subId)
        {
            List<AssignmentModel> assignments = new List<AssignmentModel>();
            AssignmentModel assignment = null;
            using (var db = new SQLite.SQLiteConnection(Constant.dbPath))
            {
                var query = (db.Table<Assignment>().Where(c => c.SubjectId == subId));
                foreach (var _assignment in query)
                {
                    assignment = new AssignmentModel(_assignment.AssignmentID, _assignment.title, _assignment.description, _assignment.teacher_full_names, GetFiles(0, 0, _assignment.AssignmentID));
                    assignments.Add(assignment);
                }
            }
            return assignments;
        }
        public static List<AttachmentModel> OldGetFiles(int topicID, int assignmentID)
        {
            List<AttachmentModel> attachments = new List<AttachmentModel>();
            AttachmentModel attachment = null;

            if (topicID == 0 && assignmentID > 0)
            {
                using (var db = new SQLite.SQLiteConnection(Constant.dbPath))
                {
                    var query = (db.Table<Attachment>().Where(c => c.AssignmentID == assignmentID));
                    foreach (var _title in query)
                    {
                        attachment = new AttachmentModel(_title.AttachmentID, _title.FilePath, _title.FileName);
                        attachments.Add(attachment);
                    }
                }
            }

            if (topicID > 0 && assignmentID == 0)
            {
                using (var db = new SQLite.SQLiteConnection(Constant.dbPath))
                {
                    var query = (db.Table<Attachment>().Where(c => c.TopicID == topicID));
                    foreach (var _title in query)
                    {
                        attachment = new AttachmentModel(_title.AttachmentID, _title.FilePath, _title.FileName);
                        attachments.Add(attachment);
                    }
                }
            }
            return attachments;
        }
        public static LibraryModel GetLibrary(int school_id)
        {
            LibraryModel library = new LibraryModel();
            List<Book> books = new List<Book>();
            List<LibCategoryModel> categories = new List<LibCategoryModel>();
            int count;
            using (var db = new SQLite.SQLiteConnection(Constant.dbPath))
            {
                var query = db.Table<Book>().Where(c => c.Library_id == school_id);
                books = query.ToList();
                count = books.Count;
            }
            library.library_id = school_id;
            library.categories = ModelTask.categories(books);
            return library;
        }
    }
}
