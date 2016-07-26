using System.Collections.Generic;
using BrainShare.Models;


namespace BrainShare.ViewModels
{
    class TopicViewModel 
    {
        private string _topicTitle;
        public string TopicName
        {
            get { return _topicTitle; }
            set { _topicTitle = value; }
        }
        private string _notes;
        public string TopicNotes
        {
            get { return _notes; }
            set { _notes = value; }
        }
   
        public TopicViewModel(TopicModel Topic)
        {
            TopicName = Topic.TopicTitle;
            TopicNotes = Topic.notes;
        }

    }
}
