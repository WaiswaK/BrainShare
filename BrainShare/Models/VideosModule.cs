using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrainShare.Models
{
    class VideosModule
    {
        public string FilePath { get; set; }
        public string FileName { get; set; }
        public int VideoID { get; set; }
        public string description { get; set; }
        public string teacher { get; set; }
        public VideosModule(int _videoID, string _filePath, string _fileName, string _description, string _teacher)
        {
            FilePath = _filePath;
            VideoID = _videoID;
            FileName = _fileName;
            description = _description;
            teacher = _teacher;
        }
        public VideosModule() { }
    }
}
