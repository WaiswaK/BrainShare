using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BrainShare.Models;

namespace BrainShare.ViewModels
{
    class TopicsPageViewModel
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

        private List<TopicObservable> _topics;
        public List<TopicObservable> TopicList
        {
            get { return _topics; }
            set { _topics = value; }
        }
        public TopicsPageViewModel(FolderObservable MainTopic)
        {
            TopicTitle = MainTopic.folder_name;
            TopicList = MainTopic.topics;
            FolderName = MainTopic.folder_name;
        }
    }
}
