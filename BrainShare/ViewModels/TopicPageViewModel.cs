using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BrainShare.Models;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using Windows.UI.Xaml.Data;


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
            TopicNotes = Topic.body;
        }

    }
}
