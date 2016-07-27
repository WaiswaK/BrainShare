using System.Collections.Generic;
using BrainShare.Models;
using System.Linq;



namespace BrainShare.ViewModels
{
    class StudentViewModel{
        //School
        private SchoolModel _school;
        public SchoolModel School
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
        private List<SubjectModel> _courses;
        public List<SubjectModel> CourseList
        {
            get { return _courses; }
            set { _courses = value; }
        }
        private LibraryModel _library;
        public LibraryModel Library
        {
         get { return _library; }
         set { _library = value; }
        }

        private List<LibCategoryModel> _libCategory;
        public List<LibCategoryModel> CategoryList
        {
            get { return _libCategory; }
            set { _libCategory = value; }
        }
        private List<LibCategoryModel> LibraryCategories(UserModel user)
        {
            List<LibCategoryModel> categories = new List<LibCategoryModel>();
            List<BookModel> books = new List<BookModel>();
            foreach (var category in user.Library.categories)
            {
                category.category_image = category.category_books.First().thumb_url;
                categories.Add(category);
            }

            return categories;
        }

        public StudentViewModel(UserModel user)
        {
            School = user.School;
            SchoolName = School.SchoolName;
            SchoolBadge = School.ImagePath;
            CourseList = user.subjects;
            CategoryList = LibraryCategories(user);
        }
    }
}
