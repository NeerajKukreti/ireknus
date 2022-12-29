using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace PDFReader.Models
{
    public class AnnouncementModel
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public string ANN_ID { get; set; }
        public string NEWS_ID { get; set; }
        public string COMPANY_NAME { get; set; }
        public string COMPANY_ID { get; set; }
        public string NEWS_SUBJECT { get; set; }
        public string HEAD_LINE { get; set; }
        public string MORE { get; set; }
        public string ANNOUNCEMENT_TYPE { get; set; }
        public string QUARTER_ID { get; set; }
        public string ATTACHMENT { get; set; }
        public string CATEGORY_NAME_BY_BSE { get; set; }
        public string NSURL { get; set; }
        public string AGENDA_ID { get; set; }
        public DateTime NEWS_SUBMISSION_DATE { get; set; }
        public string NEWS_SUBMISSION_DATE_STR { get; set; }
        public DateTime DISSEMINATION_DATE { get; set; }
        public string DISSEMINATION_DATE_STR { get; set; }
        public string TIME_DIFFERENCE { get; set; }
        public string INSERT_DATE { get; set; }
        public string ACTION { get; set; }
        public string DATETIMESTAMP { get; set; }
        public string IS_DELETED { get; set; }
        public string Color { get; set; }
        public bool IsFavorite { get; set; }
    }

    public class AnnouncementCategoryModel
    {
        public int CATEGORY_ID { get; set; }
        public string CATEGORY { get; set; }
        public int? PARENT_ID { get; set; }
        public int PRIORITY { get; set; }
        public string PCATEGORY { get; set; }
        public string SEARCH_VALUES { get; set; }
        public string INSERT_DATE { get; set; }
        public string ACTION { get; set; }
        public string DATETIMESTAMP { get; set; }
        public string IS_DELETED { get; set; }
        public bool IS_PARENT { get; set; }
        public List<SelectListItem> ParentCategory { get; set; }
    }

    public class AnnouncementCategoryCount
    {
        public int CATEGORY_ID { get; set; }
        public int? PARENT_ID { get; set; }
        public bool IsChecked { get; set; } = true;
        public string CATEGORY { get; set; }
        public int Count { get; set; }
        public string Ann_Id { get; set; }
        public List<AnnouncementCategoryCount> SubCategories { get; set; }
    }

    public class AnnoucementViewModel
    {
        public int TotalAnnouncement { get; set; }
        public int TotalCategory { get; set; }
        public List<AnnouncementCategoryCount> CategoryCounts { get; set; }
        public List<AnnouncementCategoryCount> l1 { get; set; }
        public List<AnnouncementCategoryCount> l2 { get; set; }
        public List<AnnouncementCategoryCount> RepeatedAnnList { get; set; }
        public List<AnnouncementCategoryCount> AnnList { get; set; }
        public bool ShowRepeated { get; set; }
        public bool ShowFav { get; set; }
        public Dictionary<string, List<string>> D_RepeatedAnnList { get; set; }
    }

    public class CompanyViewModel
    {
        public string DtRange { get; set; }
        public List<AnnouncementModel> announcementModels { get; set; }
        public string CompanyName { get; set; }
        public string CompanyId { get; set; }
    }

    public class WATCHLIST
    {
        public int WATCHLIST_ID { get; set; }
        public string COMPANY_ID { get; set; }
    }
}