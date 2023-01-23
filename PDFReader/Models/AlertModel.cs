using System.Collections.Generic;
using System.Web.Mvc;

namespace PDFReader.Models
{
    public class AlertModel
    {
        public int ALERT_ID { get; set; }
        public string ALERT_NAME { get; set; }
        public string CATEGORY { get; set; }
        public string KEYWORD_SET { get; set; }
        public bool WATCHLIST { get; set; }
        public bool ACTIVE { get; set; }
        public List<SelectListItem> KeywordSetList { get; set; }
        public List<SelectListItem> CategoryList { get; set; }
        public List<SelectListItem> AlertList { get; set; }
    }
}