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
        #region Attachments
        private List<AttachmentModel> _attachments;
        public List<AttachmentModel> AttachmentList
        {
            get { return _attachments; }
            set { _attachments = value; }
        }
        public List<AttachmentModel> AllAttachments(FolderModel MainTopic)
        {
            List<AttachmentModel> attachments = new List<AttachmentModel>();
            foreach (var topic in MainTopic.topics)
            {
                foreach (var file in topic.Files)
                {
                    attachments.Add(file);
                }
            }
            return attachments;
        }
        #endregion
        public TopicsViewModel(FolderModel MainTopic)
        {
            TopicTitle = MainTopic.folder_name;
            TopicList = MainTopic.topics;
            FolderName = MainTopic.folder_name;
            AttachmentList = AllAttachments(MainTopic);
        }
    }
}
