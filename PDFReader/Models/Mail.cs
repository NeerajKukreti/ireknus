using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace PDFReader.Models
{
    public class MailModel
    {
        public string To { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
    }
}