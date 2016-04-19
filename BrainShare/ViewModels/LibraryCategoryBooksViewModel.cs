using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BrainShare.Models;

namespace BrainShare.ViewModels
{
    class LibraryCategoryBooksViewModel
    {
        private List<BookObservable> _books;
        public List<BookObservable> BookList
        {
            get { return _books; }           
            set { _books = value; }
        }

        private string _categoryname;
        public string CategoryName
        {
            get
            {
                return _categoryname;
            }
            set
            {
                _categoryname = value;
            }
        }

        public LibraryCategoryBooksViewModel(Library_CategoryObservable category)
        {
            BookList = category.category_books;
            CategoryName = category.category_name;
        }
    }
}
