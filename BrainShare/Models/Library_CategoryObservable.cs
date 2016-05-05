using System.Collections.Generic;

namespace BrainShare.Models
{
    class Library_CategoryObservable
    {
        public int category_id { get; set; }
        public string category_name { get; set; }
        public int book_count { get; set; }
        public List<BookObservable> category_books { get; set; }
        public Library_CategoryObservable()
        {
        }
        public Library_CategoryObservable(int id, string name, int count, List<BookObservable> books)
        {
            category_id = id;
            category_name = name;
            book_count = count;
            category_books = books;

        }
    }
}
