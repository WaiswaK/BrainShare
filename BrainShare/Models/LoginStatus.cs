﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrainShare.Models
{
    class LoginStatus
    {
        public string statusCode { get; set; }
        public string statusDescription { get; set; }
        public LoginStatus(string status_code, string status_description)
        {
            statusCode = status_code;
            statusDescription = status_description;
        }
        public LoginStatus() { }
    }
}