using System.Collections.Generic;

namespace BrainShare.Models
{
    class NotesModule
    {
        public int Id { get; set; }
        public string name { get; set; }
        public string thumb { get; set; }
        public List<TopicObservable> topics { get; set; }
        public List<AssignmentObservable> assignments { get; set; }
        public List<VideoObservable> videos { get; set; }
        public List<AttachmentObservable> files { get; set; }
        public NotesModule(int _id, string _name, string _thumb, List<TopicObservable> _topics, List<AssignmentObservable> _assignments, List<VideoObservable> _videos, List<AttachmentObservable> _files)
        {
            Id = _id;
            name = _name;
            thumb = _thumb;
            topics = _topics;
            files = _files;
            assignments = _assignments;
            videos = _videos;
        }
        public NotesModule()
        {

        }
    }
}
