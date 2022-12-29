using PDFReader.Model;
using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace PDFReader.Models
{
    public class Reports
    {
        public int ID { get; set; }
        public string CompanyName { get; set; }
        public string URL { get; set; }
        public string AnnId { get; set; }
        public string FinancialYear { get; set; }
        public DateTime InsertDate { get; set; }
        public string Keywords { get; set; }
        public string ProcessedDate { get; set; }

        public List<FetchedKeywords> FetchedKeywords { get; set; }

        public List<SelectListItem> ReportList { get; set; }
    }

    public class Keywords
    {
        public int KID { get; set; }
        public string Keyword { get; set; }
        public bool Isactive { get; set; }
        public DateTime InsertDate { get; set; }
    }

    public class ReportViewModel { 
        public List<string> ReportList { get;set; }
        public List<SelectListItem> KeywordSet { get; set; }
    }
}