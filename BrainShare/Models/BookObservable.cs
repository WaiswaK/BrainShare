﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrainShare.Models
{
    class BookObservable
    {
        public int book_id { get; set; }
        public string book_title { get; set; }
        public string book_author { get; set; }
        public string book_description { get; set; }
        public string updated_at { get; set; }
        public string thumb_url { get; set; }
        public string file_url { get; set; }
        public int file_size { get; set; }
        public int Library_id { get; set; }
        public int Category_id { get; set; }
        public string Category_name { get; set; }

        public BookObservable(int id, string title, string author, string description, string update, string thumb, string filepath, int fileSize, int library_id,
                              int category_id, string category_name)
        {
            book_id = id;
            book_title = title;
            book_author = author;
            book_description = description;
            updated_at = update;
            thumb_url = thumb;
            file_url = filepath;
            file_size = fileSize;
            Library_id = library_id;
            Category_id = category_id;
            Category_name = category_name;
        }
        public BookObservable()
        {

        }
    }
}