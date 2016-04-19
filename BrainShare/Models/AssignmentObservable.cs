using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrainShare.Models
{
    class AssignmentObservable
    {
      public int id { get; set; }
       public string title { get; set; }
       public string description { get; set; }
       public string teacher { get; set; }
       public List<AttachmentObservable> Files { get; set; }
       public AssignmentObservable() { }
       public AssignmentObservable(int Assignment_id, string _title, string _description, string full_names, List<AttachmentObservable> _files)
       {
        id = Assignment_id;
        title = _title;
        description = _description;
        teacher = full_names;
        Files = _files;
       }
    }
}
