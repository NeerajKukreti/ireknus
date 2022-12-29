using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PDFReader.Models
{
    public class Table
    {
        public List<AnnouncementResult> TableData { get; set; }
    }

    public class Table1
    {
        public int ROWCNT { get; set; }
    }

    public class Root
    {
        public List<AnnouncementResult> Table { get; set; }
        public List<Table1> Table1 { get; set; }
    }
    public class AnnouncementResult
    {
        public string NEWSID { get; set; }
        public string SCRIP_CD { get; set; }
        public string XML_NAME { get; set; }
        public string NEWSSUB { get; set; }
        public DateTime DT_TM { get; set; }
        public DateTime NEWS_DT { get; set; }
        public int CRITICALNEWS { get; set; }
        public string ANNOUNCEMENT_TYPE { get; set; }
        public string QUARTER_ID { get; set; }
        public string FILESTATUS { get; set; }
        public string ATTACHMENTNAME { get; set; }
        public string MORE { get; set; }
        public string HEADLINE { get; set; }
        public string CATEGORYNAME { get; set; }
        public int OLD { get; set; }
        public int RN { get; set; }
        public int PDFFLAG { get; set; }
        public string NSURL { get; set; }
        public string SLONGNAME { get; set; }
        public int AGENDA_ID { get; set; }
        public int TotalPageCnt { get; set; }
        public DateTime? News_submission_dt { get; set; }
        public DateTime? DissemDT { get; set; }
        public string TimeDiff { get; set; }
        public int? Fld_attachsize { get; set; }
    }
}