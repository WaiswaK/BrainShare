using BrainShare.Database;
using BrainShare.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BrainShare.Core
{
    class ObservableStructure
    {
        #region Subjects Methods
        public static List<string> oldSubjects()
        {
            List<Subject> subjects = DatabaseRetrieveTasks.SelectAllSubjects();
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
        public static List<string> SubjectNames(List<SubjectObservable> subjects)
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
        public static string JoinedSubjects(List<string> subjects)
        {
            string joined = string.Empty;
            foreach (var subject in subjects)
            {
                if (joined.Equals(string.Empty))
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
        public static string UserSubjects(string string_input)
        {
            char[] delimiter = { '.' };
            string final = string.Empty;
            List<int> subjectids = new List<int>();
            string[] SplitSubjectId = string_input.Split(delimiter);
            List<string> SubjectIdList = SplitSubjectId.ToList();
            subjectids = SubjectIdsConvert(SubjectIdList);
            List<int> finalids = RemoveRepitions(subjectids);
            foreach (var id in finalids)
            {
                if (final.Equals(string.Empty))
                {
                    final = string.Empty + id;
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
        #endregion
        #region Topics Methods
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
                    Topic = new TopicObservable(oldTopic.TopicID, null, null, oldTopic.TopicTitle, Files, oldTopic.teacher, newTopic.Updated_at, oldTopic.folder_id, oldTopic.folder_name);
                }
            }
            else
            {
                Files = GetNewFiles(newFiles, oldFiles);
                if (Files == null)
                {
                    Topic = new TopicObservable(oldTopic.TopicID, newTopic.body, NotesTasks.NotesChanger(newTopic.body), oldTopic.TopicTitle,
                        null, oldTopic.teacher, newTopic.Updated_at, oldTopic.folder_id, oldTopic.folder_name);
                }
                else
                {
                    Topic = new TopicObservable(oldTopic.TopicID, newTopic.body, NotesTasks.NotesChanger(newTopic.body),
                        oldTopic.TopicTitle, Files, oldTopic.teacher, newTopic.Updated_at, oldTopic.folder_id, oldTopic.folder_name);
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
        #endregion        
        #region Library Methods
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
        #endregion      
    }
}
