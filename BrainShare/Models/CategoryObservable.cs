using System.Collections.Generic;

namespace BrainShare.Models
{
    class CategoryObservable
    {
        public string categoryName { get; set; }
        public string categoryImage { get; set; }
        public int categorycount { get; set; }
        public List<AttachmentObservable> files { get; set; }
        public List<VideoObservable> videos { get; set; }
        public List<AssignmentObservable> assignments { get; set; }
        public CategoryObservable() { }
        public CategoryObservable(string _categoryname, string _categoryImage, int _categoryCount, List<AttachmentObservable> _files) 
        {
            categoryName = _categoryname;
            categoryImage = _categoryImage;
            categorycount = _categoryCount;
            files = _files;
        }
        public CategoryObservable(string _categoryname, string _categoryImage, int _categoryCount, List<AssignmentObservable> _assignments)
        {
            categoryName = _categoryname;
            categoryImage = _categoryImage;
            categorycount = _categoryCount;
            assignments = _assignments;
        }
        public CategoryObservable(string _categoryname, string _categoryImage, int _categoryCount, List<VideoObservable> _videos)
        {
            categoryName = _categoryname;
            categoryImage = _categoryImage;
            categorycount = _categoryCount;
            videos = _videos;
        }
    }
}
