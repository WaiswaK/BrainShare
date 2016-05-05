using System.Collections.Generic;
using BrainShare.Models;


namespace BrainShare.ViewModels
{
    class TopicPageViewModel 
    {
        private string _topicTitle;
        public string TopicName
        {
            get { return _topicTitle; }
            set { _topicTitle = value; }
        }
        private List<AttachmentObservable> _attachments;
        public List<AttachmentObservable> TopicFiles
        {
            get { return _attachments; }
            set { _attachments = value; }
        }
        private string _notes;
        public string TopicNotes
        {
            get { return _notes; }
            set { _notes = value; }
        }
   
        public TopicPageViewModel(TopicObservable Topic)
        {
            TopicName = Topic.TopicTitle;
            TopicFiles = Topic.Files;
            TopicNotes = Topic.notes;
        }

    }
}
