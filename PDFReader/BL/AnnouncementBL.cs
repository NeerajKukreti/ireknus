using PDFReader.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace PDFReader
{
    internal class AnnouncementBL
    {
        public static async Task<AnnoucementViewModel> GetCategoryCounts(string CompanyName, bool ShowAll, string dtRange, List<AnnouncementCategoryModel> categories, bool showFav)
        {
            var announcements = GetWatchListCompanies(CompanyName, ShowAll, dtRange, showFav);

            var highPriorityCategories = categories.Where(x => x.PRIORITY == 1).ToList();
            var lowPriorityCategories = categories.Where(x => x.PRIORITY == 0).ToList();
            var Parentcategories = categories.Where(x => x.PARENT_ID != null).Select(x => x.PARENT_ID).Distinct().ToList();

            List<AnnouncementCategoryCount> searchedCategories = new List<AnnouncementCategoryCount>();
            List<AnnouncementCategoryCount> announcementCategoryCounts = new List<AnnouncementCategoryCount>();
            List<AnnouncementCategoryCount> repeatedAnnList = new List<AnnouncementCategoryCount>();
            Dictionary<string, List<string>> RepeatedAnnList = new Dictionary<string, List<string>>();
            PerformSearch(categories.ToList(), announcements.ToList(), searchedCategories, repeatedAnnList, RepeatedAnnList);

            var annLeftForLowCategories = announcements.Select(x => x.ANN_ID).Except(string.Join(",", searchedCategories.Select(x => x.Ann_Id)).Split(','));
            //PerformSearch(lowPriorityCategories.ToList(), announcements.Where(x => annLeftForLowCategories.Contains(x.ANN_ID)).ToList(), searchedCategories, repeatedAnnList, RepeatedAnnList);

            List<AnnouncementCategoryCount> subCategoriesList = searchedCategories.Where(x =>
            {
                var xx = searchedCategories;
                if (x == null)
                {
                }
                return x != null && x.PARENT_ID != null;
            }).ToList();

            List<AnnouncementCategoryCount> ActualsearchedCategories = searchedCategories.Except(subCategoriesList).ToList();

            ActualsearchedCategories.ForEach(x => //assinging subs to cats
            {
                var subCategories = subCategoriesList.Where(cat => cat.PARENT_ID == x.CATEGORY_ID).ToList();

                if (subCategories.Any())
                {
                    x.SubCategories = new List<AnnouncementCategoryCount>(subCategories);
                }
                announcementCategoryCounts.Add(x);
            });

            announcementCategoryCounts = announcementCategoryCounts.OrderBy(x => x.CATEGORY).ToList();

            var ann1 = string.Join(",", announcementCategoryCounts.Where(x=> !string.IsNullOrEmpty(x.Ann_Id)).Select(x => x.Ann_Id)).Trim(',') + "," +
                string.Join(",", subCategoriesList.Where(x => !string.IsNullOrEmpty(x.Ann_Id)).Select(x => x.Ann_Id)).Trim(',');

            var others = announcements.Select(x => x.ANN_ID).Except(ann1.Split(',')).ToList(); //ann which are not being searched

            if (others.Count() > 0)
                announcementCategoryCounts.Add(new AnnouncementCategoryCount
                {
                    CATEGORY = "Others",
                    CATEGORY_ID = 0,
                    PARENT_ID = null,
                    Count = others.Count(),
                    Ann_Id = string.Join(",", others)
                });

            return new AnnoucementViewModel
            {
                CategoryCounts = announcementCategoryCounts,
                TotalAnnouncement = announcements.Count(),//string.Join(",", announcementCategoryCounts.Where(x => !string.IsNullOrEmpty(x.Newsids)).Select(x => x.Newsids)).Split(',').Where(x => !string.IsNullOrEmpty(x)).Count(),
                TotalCategory = announcements.Count(),//string.Join(",", announcementCategoryCounts.Where(x => !string.IsNullOrEmpty(x.Newsids)).Select(x => x.Newsids)).Split(',').Where(x => !string.IsNullOrEmpty(x)).Count(),
                RepeatedAnnList = repeatedAnnList,
                D_RepeatedAnnList = RepeatedAnnList
            };
        }

        public static async Task PerformSearch(List<AnnouncementCategoryModel> categories, List<AnnouncementModel> announcements,
            List<AnnouncementCategoryCount> searchedCategories, List<AnnouncementCategoryCount> RepeatedCategoryList, 
            Dictionary<string, List<string>> RepeatedAnnList)
        {
            try
            {
                List<AnnouncementModel> ActualAnn = announcements;

                List<AnnouncementModel> ActualSearchedAnnList = new List<AnnouncementModel>();

                var previousHighPrority = categories.Max(x => x.PRIORITY);

                foreach (var category in categories.OrderByDescending(x=> x.PRIORITY).ToList())
                {
                    List<AnnouncementModel> searchedAnn = new List<AnnouncementModel>();

                    if (previousHighPrority != category.PRIORITY)
                    {
                        ActualAnn = ActualAnn.Except(ActualSearchedAnnList).ToList();
                        previousHighPrority = category.PRIORITY;
                    }

                    category.SEARCH_VALUES.Split('|').ToList().ForEach(x =>
                    {
                        var annList = ActualAnn.Where(an => !category.IS_PARENT && an.NEWS_SUBJECT.ToLower().Contains(x.ToLower())).ToList();
                        searchedAnn.AddRange(annList);
                    });

                    searchedAnn = searchedAnn.Distinct().ToList();

                    searchedAnn.ForEach(x =>
                    {
                        if (RepeatedAnnList.ContainsKey(x.ANN_ID))
                        {
                            RepeatedAnnList[x.ANN_ID].Add(category.CATEGORY);
                        }
                        else 
                            RepeatedAnnList.Add(x.ANN_ID,new List<string> { category.CATEGORY });
                    });

                    //var RepeatedAnn = ActualSearchedAnnList.Intersect(searchedAnn).ToList();
                    var RepeatedAnn = new List<AnnouncementModel>();
                    var ActualSearchedAnn = searchedAnn.Except(RepeatedAnn).ToList();

                    ActualSearchedAnnList.AddRange(ActualSearchedAnn);

                    

                    //RepeatedAnnList.AddRange(RepeatedAnn);

                    if (ActualSearchedAnn.Distinct().Any())
                    {
                        searchedCategories.Add(new AnnouncementCategoryCount
                        {
                            Count = ActualSearchedAnn.Count(),
                            CATEGORY = category.CATEGORY,
                            CATEGORY_ID = category.CATEGORY_ID,
                            PARENT_ID = category.PARENT_ID,
                            Ann_Id = string.Join(",", ActualSearchedAnn.Select(x => x.ANN_ID))
                        });
                    }
                    else
                    {
                        searchedCategories.Add(new AnnouncementCategoryCount
                        {
                            Count = 0,
                            CATEGORY = category.CATEGORY,
                            CATEGORY_ID = category.CATEGORY_ID,
                            PARENT_ID = category.PARENT_ID,
                            Ann_Id = string.Empty
                        });
                    }

                    if (RepeatedAnn.Any())
                    {
                        RepeatedCategoryList.Add(new AnnouncementCategoryCount
                        {
                            Count = RepeatedAnn.Count(),
                            CATEGORY = category.CATEGORY,
                            CATEGORY_ID = category.CATEGORY_ID,
                            PARENT_ID = category.PARENT_ID,
                            Ann_Id = string.Join(",", RepeatedAnn.Select(x => x.ANN_ID))
                        });
                    }
                }
            }
            catch (Exception ex) { 
            
            }
        }

        public static IEnumerable<AnnouncementModel> GetWatchListCompanies(string CompanyName, bool ShowAll, string dtRange, bool ShowFav = false)
        {
            var watchList = new List<string>();

            if (!ShowAll)
                watchList = DB.GetWatchList().Select(y => y.COMPANY_ID).ToList();

            var ann = new List<AnnouncementModel>();

            if (!string.IsNullOrEmpty(dtRange)) // note: date range will never be empty
            {
                var sDt = DateTime.Parse(dtRange.Split('|')[0]);
                var eDt = DateTime.Parse(dtRange.Split('|')[1]);
                ann = DB.GetCompaniesByDtRange(sDt, eDt).ToList(); 
            }

            var watchListAnn = new List<AnnouncementModel>();

            if (!string.IsNullOrEmpty(CompanyName))
            {
                if (ShowAll)
                {
                    watchListAnn = ann.Where(x =>
                        x.COMPANY_NAME.ToLower().Equals(CompanyName.ToLower())).ToList();
                }
                else
                    watchListAnn = ann.Where(x =>
                        watchList.Contains(x.COMPANY_ID) &&
                        x.COMPANY_NAME.ToLower().Equals(CompanyName.ToLower())).ToList();
            }
            else
            {
                if (ShowAll)
                    watchListAnn = ann.ToList();
                else
                    watchListAnn = ann.Where(x => watchList.Contains(x.COMPANY_ID)).ToList();
            }

            

            if (ShowFav) {
                watchListAnn = watchListAnn.Where(x => x.IsFavorite).ToList();
            }

            return watchListAnn;
        }
    }
}