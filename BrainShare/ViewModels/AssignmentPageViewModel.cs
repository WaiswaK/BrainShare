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
    class AssignmentPageViewModel 
    {
        private string _topicTitle;
        public string AssignmentTitle
        {
            get { return _topicTitle; }
            set { _topicTitle = value; }
        }
        private List<AttachmentObservable> _attachments;
        public List<AttachmentObservable> AssignmentFiles
        {
            get { return _attachments; }
            set { _attachments = value; }
        }
        private string _notes;
        public string AssignmentNotes
        {
            get { return _notes; }
            set { _notes = value; }
        }

        public AssignmentPageViewModel(AssignmentObservable assignment)
        {
            AssignmentTitle = assignment.title;
            AssignmentFiles = assignment.Files;
            AssignmentNotes = assignment.description;
        }

    }
}
