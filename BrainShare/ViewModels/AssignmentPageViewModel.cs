using System.Collections.Generic;
using BrainShare.Models;


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
