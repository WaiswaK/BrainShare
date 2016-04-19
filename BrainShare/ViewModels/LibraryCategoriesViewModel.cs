using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BrainShare.Models;

namespace BrainShare.ViewModels
{
    class LibraryCategoriesViewModel
    {
        private List<Library_CategoryObservable> _categories;
        public List<Library_CategoryObservable> CategoryList
        {
            get { return _categories; }
            set { _categories = value; }
        }
        public LibraryCategoriesViewModel(LibraryObservable library)
        {
            CategoryList = library.categories;
        }
    }
}
