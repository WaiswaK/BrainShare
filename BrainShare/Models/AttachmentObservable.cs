namespace BrainShare.Models
{
    class AttachmentObservable
    {
        public string FilePath { get; set; }
        public string FileName { get; set; }
        public int AttachmentID { get; set; }
        public AttachmentObservable(int _attachmentID, string _filePath, string _fileName)
        {
            FilePath = _filePath;
            AttachmentID = _attachmentID;
            FileName = _fileName;
        }
        public AttachmentObservable() { }
    }
}
