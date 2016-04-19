using System.Collections.Generic;
using BrainShare.Models;



namespace BrainShare.ViewModels
{
    class StudentPageViewModel{
        //School
        private SchoolObservable _school;
        public SchoolObservable School
        {
            get { return _school; }
            set { _school = value; }
        }
        private string _schoolname;
        public string SchoolName
        {
            get { return _schoolname; }
            set { _schoolname = value; }
        }
        private string _schoolbadge;
        public string SchoolBadge
        {
            get { return _schoolbadge; }
            set { _schoolbadge = value; }
        }
        //Subjects
        private List<SubjectObservable> _courses;
        public List<SubjectObservable> CourseList
        {
            get { return _courses; }
            set { _courses = value; }
        }
        private LibraryObservable _library;
        public LibraryObservable Library
        {
         get { return _library; }
         set { _library = value; }
        }

        private List<Library_CategoryObservable> _libCategory;
        public List<Library_CategoryObservable> CategoryList
        {
            get { return _libCategory; }
            set { _libCategory = value; }
        }


        public StudentPageViewModel(UserObservable user)
        {
            School = user.School;
            SchoolName = School.SchoolName;
            SchoolBadge = School.ImagePath;
            CourseList = user.subjects;
            CategoryList = user.Library.categories;

        }
    }
}
