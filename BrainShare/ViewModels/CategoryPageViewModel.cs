using System.Collections.Generic;
using BrainShare.Models;

namespace BrainShare.ViewModels
{
    class CategoryPageViewModel
    {
        private string _categoryName;
        public string CategoryName
        {
            get { return _categoryName; }
            set { _categoryName = value; }
        }
        private List<AttachmentObservable> _bookList;
        public List<AttachmentObservable> FileList
        {
            get { return _bookList; }
            set { _bookList = value; }
        }
        private List<VideoObservable> _videosList;
        public List<VideoObservable> VideosList
        {
            get { return _videosList; }
            set { _videosList = value; }
        }
        private List<AssignmentObservable> _assignment;
        public List<AssignmentObservable> AssignmentList
        {
            get { return _assignment; }
            set { _assignment = value; }
        }
        public CategoryPageViewModel(CategoryObservable category)
        {
            FileList = category.files;
            VideosList = category.videos;
            AssignmentList = category.assignments;
            CategoryName = category.categoryName;
        }
    }
}
