﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrainShare.Models
{
    public class SchoolObservable
    {
        public string SchoolName;
        public string ImagePath;
        public int SchoolId;

        public SchoolObservable(string _schoolName, string _imagePath, int _id)
        {
            SchoolName = _schoolName;
            ImagePath = _imagePath;
            SchoolId = _id;
        }

        public SchoolObservable() { }
    }
}