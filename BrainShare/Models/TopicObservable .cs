using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrainShare.Models
{
    class TopicObservable 
    {
        public string TopicTitle { get; set; }
        public int TopicID { get; set; }
        public string body { get; set; }
        public string teacher { get; set; }
        public List<AttachmentObservable> Files { get; set; }
        public string Updated_at { get; set; }
        public string folder_name { get; set; }
        public int folder_id { get; set; }

        public TopicObservable(int _topicId, string _notes, string _title, List<AttachmentObservable> _files, string full_names, string _updated, int _folder_id, string _folder_name)
        {
            TopicTitle = _title;
            TopicID = _topicId;
            Files = _files;
            body = _notes;
            teacher = full_names;
            Updated_at = _updated;
            folder_id = _folder_id;
            folder_name = _folder_name;
        }

        public TopicObservable() { }
    }
}