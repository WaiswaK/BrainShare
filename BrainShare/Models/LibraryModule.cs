using System.Collections.Generic;

namespace BrainShare.Models
{
    class LibraryModule
    {
        public int library_id { get; set; }
        public List<Library_CategoryObservable> categories { get; set; }
        public LibraryModule()
        {

        }
        public LibraryModule(int id, List<Library_CategoryObservable> category)
        {
            library_id = id;
            categories = category;
        }
    }
}
