using System;
using System.Collections.Generic;

namespace PDFReader.Model
{
    public class ReportModel
    {
        public int ID { get; set; }
        public string CompanyName { get; set; }
        public string URL { get; set; }
        public string FinancialYear { get; set; }
        public DateTime InsertDate { get; set; }
        public string ProcessedDate { get; set; }
        public List<FetchedKeywords> FetchedKeywords { get; set; }

        public int annid { get; set; }
    }

    public class FetchedKeywords
    {
        public int ID { get; set; }
        public int ReportID { get; set; }
        public int PDFPageNumber { get; set; }
        public string FoundKeywords { get; set; }
        public string PageText { get; set; }
    }
    public class FetchedPhrase
    {
        public int ReportID { get; set; }
        public int PDFPageNumber { get; set; }
        public string FoundKeywords { get; set; }
        public List<string> PhraseText { get; set; }
    }

    public class ReportResult
    {
        public List<ReportModel> Processed { get; set; }
        public List<ReportModel> UnProcessed { get; set; }
    }

    public class KeywordResult
    {
        public int ReportId { get; set; }
        public string Url { get; set; }
        public string news_subject { get; set; }
        public string NEWS_SUBMISSION_DATE { get; set; }
        public string NEWS_SUBMISSION_DATE_STR { get; set; }
        public int Company_Id { get; set; }
        public string FinancialYear { get; set; }
        public string FoundKeywords { get; set; }
        public string PDFPageNumber { get; set; }
        public string CompanyName { get; set; }
        public string TotalPages { get; set; }
        public string PageText { get; set; }
    }

    public class SkippedCount
    {
        public int ImgSkipped { get; set; }
        public int NonImgSkipped { get; set; }
    }
}