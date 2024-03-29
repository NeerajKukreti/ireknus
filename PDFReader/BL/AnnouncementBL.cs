﻿using DocumentFormat.OpenXml.Drawing.Diagrams;
using PDFReader.Models;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace PDFReader
{
    internal class AnnouncementBL
    {
        public static async Task<AnnoucementViewModel> GetCategoryCounts(string CompanyName, bool ShowAll, string dtRange,bool showFav
            , bool showrepeated = false, int timeSlot = 0)
        {
           
            var AnnoucementView = DB.GetDashboardCategories(String.Empty, CompanyName, ShowAll, dtRange, showFav, showrepeated, timeSlot);
            
            return AnnoucementView;
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
                var rptAnn = announcements.Where(x => x.rn > 1).Select(x => x.ANN_ID);
                var data1 = data.Where(x => rptAnn.Contains(x.ANN_ID));

                categories.ForEach(cat =>
                {
                    var anns = data.Where(x => x.CATEGORY_ID == cat.CATEGORY_ID);
                    searchedCategories.Add(new AnnouncementCategoryCount
                    {
                        Count = anns.Count(),
                        CATEGORY = cat.CATEGORY,
                        CATEGORY_ID = cat.CATEGORY_ID,
                        PARENT_ID = cat.PARENT_ID,
                        //Ann_Id = string.Join(",", anns.Select(x => x.ANN_ID))
                    });


                    anns = data1.Where(x => x.CATEGORY_ID == cat.CATEGORY_ID);
                    RepeatedAnnList.Add(new AnnouncementCategoryCount
                    {
                        Count = anns.Count(),
                        CATEGORY = cat.CATEGORY,
                        CATEGORY_ID = cat.CATEGORY_ID,
                        PARENT_ID = cat.PARENT_ID,
                        //Ann_Id = string.Join(",", anns.Select(x => x.ANN_ID))
                    });

                });

                data.GroupBy(x => x.ANN_ID).ToList().ForEach(x =>
                 {
                     AnnCategories.Add(x.Key, x.Select(y => y.CATEGORY).ToList());
                 });


            }
            catch (Exception ex)
            {

            }
        }

        public static AnnoucementGridData GetWatchListCompanies(string CompanyName, bool ShowAll, string dtRange, bool ShowFav = false)
        {
            var ann = DB.GetDashboardDetails(string.Empty, CompanyName, ShowAll, dtRange, ShowFav);

            return ann;
        }
    }
}