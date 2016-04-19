using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
