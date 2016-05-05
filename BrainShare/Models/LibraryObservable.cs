using System.Collections.Generic;

namespace BrainShare.Models
{
    class LibraryObservable
    {
        public int library_id { get; set; }
        public List<Library_CategoryObservable> categories { get; set; }

        public LibraryObservable()
        {

        }

        public LibraryObservable(int id, List<Library_CategoryObservable> category)
        {
            library_id = id;
            categories = category;
        }


    }
}
