using DocumentFormat.OpenXml.Drawing.Diagrams;
using PDFReader.Models;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;

namespace PDFReader
{
    internal class AnnouncementBL
    {
        public static async Task<AnnoucementViewModel> GetCategoryCounts(string CompanyName, bool ShowAll, string dtRange, List<AnnouncementCategoryModel> categories, bool showFav)
        {
            var announcements = GetWatchListCompanies(CompanyName, ShowAll, dtRange, showFav);

            List<AnnouncementCategoryCount> searchedCategories = new List<AnnouncementCategoryCount>();
            List<AnnouncementCategoryCount> announcementCategoryCounts = new List<AnnouncementCategoryCount>();
            List<AnnouncementCategoryCount> repeatedAnnList = new List<AnnouncementCategoryCount>();
            Dictionary<string, List<string>> annCategories = new Dictionary<string, List<string>>();
            PerformSearch(categories.ToList(), announcements.ToList(), searchedCategories, annCategories, repeatedAnnList);


            List<AnnouncementCategoryCount> subCategoriesList = searchedCategories.Where(x =>
                  x?.PARENT_ID != null
            ).ToList();

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

            return new AnnoucementViewModel
            {
                CategoryCounts = announcementCategoryCounts,
                TotalAnnouncement = announcements.Count(),
                TotalCategory = announcements.Count(),
                RepeatedAnnList = repeatedAnnList,
                D_RepeatedAnnList = annCategories
            };
        }

        public static async Task PerformSearch(List<AnnouncementCategoryModel> categories, List<AnnouncementResult> announcements,
            List<KeyValuePair<string, int>> RepeatedAnnList)
        {
            try
            {
               
                List<AnnouncementResult> ActualAnn = announcements;

                List<AnnouncementResult> ActualSearchedAnnList = new List<AnnouncementResult>();

                var previousHighPrority = categories.Max(x => x.PRIORITY);

                foreach (var category in categories
                    .Where(x => !x.CATEGORY.Equals("Others"))
                    .OrderByDescending(x => x.PRIORITY).ToList())
                {
        
                    List<AnnouncementResult> searchedAnn = new List<AnnouncementResult>();

                    if (previousHighPrority != category.PRIORITY)
                    {
                        ActualAnn = ActualAnn.Except(ActualSearchedAnnList).ToList();
                        previousHighPrority = category.PRIORITY;
                    }

                    category.SEARCH_VALUES.Split('|').ToList().ForEach(x =>
                    {
                        var annList = ActualAnn.Where(an => !category.IS_PARENT && an.NEWSSUB.ToLower().Contains(x.ToLower())).ToList();
                        searchedAnn.AddRange(annList);
                    });

                    searchedAnn = searchedAnn.Distinct().ToList();

                    searchedAnn.ForEach(x =>
                    {
                        RepeatedAnnList.Add(new KeyValuePair<string, int>(x.NEWSID, category.CATEGORY_ID));
                    });

                    ActualSearchedAnnList.AddRange(searchedAnn);
                }

                var otherCat = categories.FirstOrDefault(x => x.CATEGORY.ToLower().Equals("others")).CATEGORY_ID;

                var other = announcements.Select(x => x.NEWSID).Except(RepeatedAnnList.Select(x => x.Key).Distinct()).ToList();

                announcements.Where(x => other.Contains(x.NEWSID)).ToList().ForEach(x =>
                {
                    RepeatedAnnList.Add(new KeyValuePair<string, int>(x.NEWSID, otherCat));
                });



            }
            catch (Exception ex)
            {

            }
        }

        public static async Task PerformSearch(List<AnnouncementCategoryModel> categories, List<AnnouncementModel> announcements,
            List<AnnouncementCategoryCount> searchedCategories, Dictionary<string, List<string>> AnnCategories, List<AnnouncementCategoryCount> RepeatedAnnList)
        {
            try
            {

                var data = DB.GetAnnCates(announcements.Select(x => x.ANN_ID).ToList());

                categories.ForEach(cat =>
                {
                    var anns = data.Where(x => x.CATEGORY_ID == cat.CATEGORY_ID);
                    searchedCategories.Add(new AnnouncementCategoryCount
                    {
                        Count = anns.Count(),
                        CATEGORY = cat.CATEGORY,
                        CATEGORY_ID = cat.CATEGORY_ID,
                        PARENT_ID = cat.PARENT_ID,
                        Ann_Id = string.Join(",", anns.Select(x => x.ANN_ID))
                    });
                });

                data.GroupBy(x=> x.ANN_ID).ToList().ForEach(x =>
                {
                    AnnCategories.Add(x.Key, x.Select(y=> y.CATEGORY).ToList());
                });
            }
            catch (Exception ex)
            {

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



            if (ShowFav)
            {
                watchListAnn = watchListAnn.Where(x => x.IsFavorite).ToList();
            }

            return watchListAnn;
        }
    }
}