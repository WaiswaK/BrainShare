using System.Collections.Generic;
using BrainShare.Models;

namespace BrainShare.ViewModels
{
    class TopicsViewModel
    {
        private string _topicTitle;
        private string _folderName;
        public string TopicTitle
        {
            get { return _topicTitle; }
            set { _topicTitle = value; }

        }
        public string FolderName
        {
            get { return _folderName; }
            set { _folderName = value; }

        }
        private List<TopicModel> _topics;
        public List<TopicModel> TopicList
        {
            get { return _topics; }
            set { _topics = value; }
        }
        public TopicsViewModel(FolderModel MainTopic)
        {
            TopicTitle = MainTopic.folder_name;
            TopicList = MainTopic.topics;
            FolderName = MainTopic.folder_name;
        }
    }
}
