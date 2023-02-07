using ClosedXML.Excel;
using DocumentFormat.OpenXml.Drawing.Charts;
using PDFReader.Model;
using PDFReader.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.UI;

namespace PDFReader.Controllers
{
    public class AnnouncementController : Controller
    {
        #region announcement

        public async Task<ActionResult> Index()
        {
            return View();
        }

        [OutputCache(Duration = 20, Location = OutputCacheLocation.Client, VaryByParam = "none")]
        public async Task<ActionResult> GetAnnouncementView(string CompanyName, string DateRange, bool ShowAll = false, bool ShowRepeated = false, bool showFav = false)
        {
            var categoriesCount = DB.GetDashboardCategoriesCnt(null,CompanyName, ShowAll, DateRange, showFav, ShowRepeated);
            //var categoriesCount = AnnouncementBL.GetCategoryCounts(CompanyName, ShowAll, DateRange, showFav, ShowRepeated).Result;
            //categoriesCount.ShowRepeated = ShowRepeated;
            ViewBag.ShowRepeated = ShowRepeated;

            return PartialView("_AnnouncementView", categoriesCount);
        }

        public async Task<ActionResult> GetDashboardCategories(string CompanyName, string DateRange, bool ShowAll = false, bool ShowRepeated = false, bool showFav = false)
        {
            var categoriesCount = AnnouncementBL.GetCategoryCounts(CompanyName, ShowAll, DateRange, showFav, ShowRepeated).Result;
            categoriesCount.ShowRepeated = ShowRepeated;
            ViewBag.ShowRepeated = ShowRepeated;

            return PartialView("_AnnouncementCounts", categoriesCount.CategoryCounts);
        }

        [OutputCache(Duration = 20, Location = OutputCacheLocation.Client, VaryByParam = "none")]
        public ActionResult GetAnnouncements(string companyName, string catIds, bool showRepeated = false, string dtRange = "", bool showFav = false, bool showAll = false, int? start = null, int? length = null, int? draw = null)
        {
            var announcements = DB.GetDashboardDetails(catIds, companyName, showAll, dtRange, showFav, start, length, showRepeated);

            if (draw != null)
            announcements.draw = draw.Value;

            var jsonResult = Json(announcements, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;
        }
        [OutputCache(Duration = 20, Location = OutputCacheLocation.Client, VaryByParam = "none")]
        public ActionResult GetAnnouncements1(string catIds, bool showRepeated = false)
        {
            var ann = (AnnoucementViewModel)System.Web.HttpContext.Current.Application["CategoriesCount"];
            var newIds = string.Empty;
            var announcements = new List<AnnouncementModel>();

            List<AnnouncementCategoryCount> categoryCounts = showRepeated ? ann.RepeatedAnnList : ann.CategoryCounts;

            if (!showRepeated)
            {
                if (string.IsNullOrEmpty(catIds))
                {
                    newIds = string.Join(",", categoryCounts.Where(x => !string.IsNullOrEmpty(x.Ann_Id)).Select(x => x.Ann_Id));

                    foreach (var cat in categoryCounts)
                    {
                        if (cat.SubCategories != null && cat.SubCategories.Any())
                        {
                            newIds = newIds + "," + string.Join(",", cat.SubCategories.Where(x => !string.IsNullOrEmpty(x.Ann_Id)).Select(x => x.Ann_Id));
                        }
                    }
                }
                else
                {
                    newIds = string.Join(",",
                        categoryCounts
                        .Where(x => catIds.Split(',').Contains(x.CATEGORY_ID.ToString())).Where(x => !string.IsNullOrEmpty(x.Ann_Id)).Select(x => x.Ann_Id).Distinct()
                        );

                    categoryCounts
                        .ForEach(x =>
                        {
                            if (x.SubCategories != null && x.SubCategories.Any())
                            {
                                newIds = newIds + "," + string.Join(",", x.SubCategories
                                        .Where(y => catIds.Split(',').Contains(y.CATEGORY_ID.ToString()))
                                        .Where(y => !string.IsNullOrEmpty(y.Ann_Id)).Select(y => y.Ann_Id).Distinct());
                            }
                        });

                }

                announcements = DB.GetAnnouncements(string.Join(",", newIds.Split(',').Distinct())).ToList();
                //announcements = DB.GetAnnouncements(string.Join(",", newIds.Split(','))).ToList();

                announcements.ForEach(x =>
                {
                    var category = categoryCounts.FirstOrDefault(y => y.Ann_Id.Contains(x.ANN_ID));

                    if (category != null)
                    {
                        x.CategoryId = category.CATEGORY_ID;
                        x.CategoryName = category.CATEGORY;
                    }

                    if (!x.HEAD_LINE.ToLower().Equals(x.MORE.ToLower()))
                    {
                        x.HEAD_LINE = $"{x.HEAD_LINE} {x.MORE}";
                    }

                    x.DISSEMINATION_DATE_STR = x.NEWS_SUBMISSION_DATE.ToString("dd/MM/yyyy HH:mm:ss");
                    x.NEWS_SUBMISSION_DATE_STR = x.NEWS_SUBMISSION_DATE.ToString("dd/MM/yyyy HH:mm:ss");
                    x.ATTACHMENT = $"https://www.bseindia.com/xml-data/corpfiling/AttachLive/{x.ATTACHMENT}";
                });
            }
            else
            {

                var rptList = ann.D_RepeatedAnnList.ToList().Where(x => x.Value.Count() > 1).ToList();
                announcements = DB.GetAnnouncements(string.Join(",", rptList.Select(x => x.Key))).ToList();

                announcements.ForEach(x =>
                {
                    var category = rptList.FirstOrDefault(y => y.Key.Contains(x.ANN_ID));

                    //if (category.)
                    {
                        x.CategoryId = 0;
                        x.CategoryName = string.Join(",", category.Value);
                    }

                    if (!x.HEAD_LINE.ToLower().Equals(x.MORE.ToLower()))
                    {
                        x.HEAD_LINE = $"{x.HEAD_LINE} {x.MORE}";
                    }
                });
            }

            var jsonResult = Json(data: announcements, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;

            return jsonResult;

        }

        public ActionResult GetCompanies(string q, string DateRange, bool ShowAll = false, bool showFav = false)
        {
            var list = DB.SearchCompany(q);
            return Json(data:
                list.Select(x => new { value = x.COMPANY_ID, text = x.COMPANY_NAME }).Distinct(), JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region announcement settings
        public ActionResult Settings()
        {
            var categories = DB.GetCategories();
            var parentCat = categories.Where(x => x.PARENT_ID != null).Select(x => x.PARENT_ID).ToList();
            categories = categories.Where(x => !parentCat.Contains(x.CATEGORY_ID)).ToList();
            var KeywordSet = DB.GetKeywordSet();

            AlertModel alertModel = new AlertModel
            {
                ACTIVE = true,
                CategoryList = categories.Select(x =>
                              new SelectListItem
                              {
                                  Text = x.CATEGORY,
                                  Value = x.CATEGORY
                              }).ToList(),
                KeywordSetList = KeywordSet.Select(x =>
                                  new SelectListItem
                                  {
                                      Text = x.SM_NAME,
                                      Value = x.SM_NAME
                                  }).ToList(),
                WATCHLIST = false
            };

            return View("_AnnouncementSettings", alertModel);
        }


        public ActionResult GetSettings()
        {
            var jsonResult = Json(data: DB.GetSettings(), JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;

            return jsonResult;
        }

        public void SaveSettings(AlertModel alertModel)
        {
            DB.SaveSettings(alertModel);
        }
        #endregion

        #region announcement alert

        //public void ExecuteAlertJob(string alertList, DateTime Searcheddate) {
        //    var list = alertList.Split(',');

        //    var alerts = DB.GetSettings().Where(x=> list.Contains(x.CATEGORY)).ToList();

        //    alerts.ForEach(alert =>
        //    {
        //        var categories = DB.GetCategoriesByName(alert.CATEGORY);

        //        if (categories == null || !categories.Any()) return;

        //        var date = DateTime.Now.AddDays(-2).Date;

        //        var dtRange = $"{date}|{date}";
        //        var announcementData = AnnouncementBL.GetCategoryCounts(string.Empty, alert.WATCHLIST, dtRange, categories.ToList()).Result;
        //        var announcements = DB.GetAnnouncements(string.Join(",", announcementData.CategoryCounts.Where(x => !x.CATEGORY.Equals("Others")).Select(x => x.Ann_Id))).ToList();
        //        List<Reports> reports = new List<Reports>();

        //        announcements.ForEach(x =>
        //        {
        //            reports.Add(new Reports
        //            {
        //                CompanyName = x.COMPANY_NAME,
        //                FinancialYear = alert.ALERT_NAME,
        //                URL = $"https://www.bseindia.com/xml-data/corpfiling/AttachLive/{x.ATTACHMENT}",
        //                AnnId = x.ANN_ID
        //            });
        //        });

        //        DB.insertCompaniesData(reports, date);
        //        var annReports = DB.GetReports(alert.ALERT_NAME, date).ToList();
        //        var keywords = DB.GetKeywordsBySetName(alert.KEYWORD_SET);

        //        Parallel.ForEach(annReports, report =>
        //        {
        //            PDFSearch.Search(report.ID, report.URL, keywords.Select(x => x.KEYWORD).ToList());
        //        });

        //        var finalReport = DB.GetSearchedReport(alert.ALERT_NAME, DateTime.Now, DateTime.Now);

        //        if (finalReport.Count() > 0)
        //            if (GenerateExcel(finalReport.ToList(), alert.ALERT_NAME))
        //            {
        //                SendMail(alert.ALERT_NAME, finalReport.ToList());
        //            }

        //    });
        //}

        public void ExecuteAlertJob()
        {
            var alerts = DB.GetSettings().Where(x => x.ACTIVE).ToList();
            string xx = "";

            alerts.ForEach(alert =>
            {
                var categories = DB.GetCategoriesByName(alert.CATEGORY);

                if (categories == null || !categories.Any()) return;

                var date = DateTime.Now.AddDays(-1).Date;
                var dtRange = $"{date}|{date}";
                var announcementData = AnnouncementBL.GetCategoryCounts(string.Empty, alert.WATCHLIST, dtRange, false).Result;
                var announcements = DB.GetAnnouncements(string.Join(",", announcementData.CategoryCounts.Where(x => !x.CATEGORY.Equals("Others")).Select(x => x.Ann_Id))).ToList();
                List<Reports> reports = new List<Reports>();

                announcements.ForEach(x =>
                {
                    reports.Add(new Reports
                    {
                        CompanyName = x.COMPANY_NAME,
                        FinancialYear = alert.ALERT_NAME,
                        URL = $"https://www.bseindia.com/xml-data/corpfiling/AttachLive/{x.ATTACHMENT}",
                        AnnId = x.ANN_ID,
                        InsertDate = x.NEWS_SUBMISSION_DATE
                    });
                });

                DB.insertCompaniesData(reports);
                var annReports = DB.GetReportsByAnnId(alert.ALERT_NAME, string.Join(",", reports.Select(x => x.AnnId))).ToList();
                var keywords = DB.GetKeywordsBySetName(alert.KEYWORD_SET);

                Parallel.ForEach(annReports, report =>
                {
                    PDFSearch.Search(report.ID, report.URL, keywords.Select(x => x.KEYWORD).ToList());
                });

                var finalReport = DB.GetSearchedReportNyAnnId(alert.ALERT_NAME, string.Join(",", reports.Select(x => x.AnnId)));

                if (finalReport.Count() > 0)
                    if (GenerateExcel(finalReport.ToList(), alert.ALERT_NAME))
                    {
                        SendMail(alert.ALERT_NAME, finalReport.ToList());
                    }

            });
        }

        private bool GenerateExcel(List<KeywordResult> keywordResults, string FileName)
        {
            try
            {
                var dt = new System.Data.DataTable();
                //Setiing Table Name  
                dt.TableName = "SearchedKeyword";
                //Add Columns  
                dt.Columns.Add("", typeof(string));
                dt.Columns.Add("RT", typeof(string));
                dt.Columns.Add("FinancialYear", typeof(string));
                dt.Columns.Add("CompanyName", typeof(string));
                dt.Columns.Add("Subject", typeof(string));
                dt.Columns.Add("Keywords", typeof(string));
                dt.Columns.Add("PageNumber", typeof(int));
                dt.Columns.Add("TotalPages", typeof(int));


                keywordResults = keywordResults.OrderBy(x => x.CompanyName).ToList();

                foreach (var item in keywordResults)
                {
                    dt.Rows.Add("", item.NEWS_SUBMISSION_DATE, item.FinancialYear, item.CompanyName, item.news_subject.Replace($"{item.CompanyName} - {item.Company_Id} - ", ""), item.FoundKeywords, item.PDFPageNumber, item.TotalPages);
                }

                dt.AcceptChanges();

                using (XLWorkbook wb = new XLWorkbook())
                {
                    //Add DataTable in worksheet  
                    var ws = wb.Worksheets.Add(dt);

                    for (int rowNumber = 1; rowNumber <= keywordResults.Count(); rowNumber++)
                    {
                        //for (int columnNumber = 1; columnNumber <= 7; columnNumber++)
                        {
                            if (rowNumber > 1)
                                ws.Cell(rowNumber, 4).Hyperlink = new XLHyperlink($@"{keywordResults.ElementAt(rowNumber - 2).Url}#page={keywordResults.ElementAt(rowNumber - 2).PDFPageNumber}");
                        }
                    }

                    using (MemoryStream ms = new MemoryStream())
                    {
                        using (FileStream file = new FileStream(Server.MapPath($"~/App_Data/ExcelDownload/{FileName}.xlsx"), FileMode.Create, System.IO.FileAccess.ReadWrite))
                        {
                            wb.SaveAs(ms);

                            byte[] xlsInBytes = ms.ToArray();
                            file.Write(xlsInBytes, 0, xlsInBytes.Length);
                            ms.Close();
                        }
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        private void SendMail(string AttachmentName, List<KeywordResult> finalReport)
        {
            var to = ConfigurationManager.AppSettings["Mail-to"].ToString();
            var from = ConfigurationManager.AppSettings["Mail-from"].ToString();
            var cc = ConfigurationManager.AppSettings["Mail-cc"].ToString();
            var bcc = ConfigurationManager.AppSettings["Mail-bcc"].ToString();
            var passcode = ConfigurationManager.AppSettings["Mail-passcode"].ToString();


            MailModel objModelMail = new MailModel
            {
                To = to,
                Subject = $"{DateTime.Now.AddDays(-1).ToString("dd/MM/yyyy")} | {AttachmentName}",
                Body = SetBodyContent(finalReport.ToList())
            };

            using (MailMessage mail = new MailMessage(from, objModelMail.To))
            {
                mail.Subject = objModelMail.Subject;
                mail.IsBodyHtml = true;
                mail.Body = objModelMail.Body;


                var fileStream = new FileStream(Server.MapPath($"~/App_Data/ExcelDownload/{AttachmentName}.xlsx")
                    , FileMode.Open, FileAccess.ReadWrite);
                {
                    mail.Attachments.Add(new Attachment(fileStream, $"{AttachmentName}_{DateTime.Now.AddDays(-1):dd/MM/yyyy}.xlsx"));
                }

                SmtpClient smtp = new SmtpClient
                {
                    Host = "smtp.gmail.com",
                    EnableSsl = true,
                    UseDefaultCredentials = false
                };

                NetworkCredential networkCredential = new NetworkCredential(from, passcode);
                smtp.Credentials = networkCredential;
                smtp.Port = 587;
                smtp.Send(mail);
            }
        }

        private string SetBodyContent(List<KeywordResult> keywordResults)
        {
            StringBuilder sb = new StringBuilder();
            keywordResults.GroupBy(x => x.CompanyName).ToList().ForEach(x =>
            {

                sb.AppendLine($"<br><strong style='color: blue;'>{x.Key}</strong><br>");
                x.ToList().ForEach(item =>
                {
                    sb.AppendLine($"<a style='text-decoration: none;color: blue;' target='_blank' href='{item.Url}#page={item.PDFPageNumber}'>{item.news_subject.Replace($"{item.CompanyName} - {item.Company_Id} - ", "")}</a> - Keyword: {item.FoundKeywords} - {item.PDFPageNumber}<br>");
                });
            });

            string str = string.Format("<html><body>{0}</body></html>", sb.ToString());

            return str;
        }
        #endregion

        #region alert report
        public ActionResult AlertReports()
        {
            var alertlist = DB.GetAlertList();
            AlertModel ob = new AlertModel();

            ob.AlertList = alertlist.Select(x =>
            new SelectListItem
            {
                Text = x.ALERT_NAME.ToString(),
                Value = x.ALERT_ID.ToString()
            }).ToList();
            return View("_AlertReportList", ob);
        }


        public void AlertInsert(string alertName, DateTime alertDate)
        {
            var alerts = new List<AlertModel>();
            if (alertName == "All Alert")
            {
                alerts = DB.GetSettings().ToList();
            }
            else
            {
                alerts = DB.GetSettings().Where(x => x.ALERT_NAME == alertName).ToList();
            }

            string xx = "";
            try
            {
                alerts.ForEach(alert =>
                {
                    var categories = DB.GetCategoriesByName(alert.CATEGORY);

                    if (categories == null || !categories.Any()) return;

                    var date = alertDate;
                    var dtRange = $"{date}|{date}";
                    var announcementData = AnnouncementBL.GetCategoryCounts(string.Empty, alert.WATCHLIST, dtRange, false).Result;
                    var announcements = DB.GetAnnouncements(string.Join(",", announcementData.CategoryCounts.Where(x => !x.CATEGORY.Equals("Others")).Select(x => x.Ann_Id))).ToList();
                    List<Reports> reports = new List<Reports>();

                    announcements.ForEach(x =>
                    {
                        reports.Add(new Reports
                        {
                            CompanyName = x.COMPANY_NAME,
                            FinancialYear = alert.ALERT_NAME,
                            URL = $"https://www.bseindia.com/xml-data/corpfiling/AttachLive/{x.ATTACHMENT}",
                            AnnId = x.ANN_ID,
                            InsertDate = x.NEWS_SUBMISSION_DATE
                        });
                    });

                    DB.insertCompaniesData(reports);
                    var annReports = DB.GetReportsByAnnId(alert.ALERT_NAME, string.Join(",", reports.Select(x => x.AnnId))).ToList();
                    var keywords = DB.GetKeywordsBySetName(alert.KEYWORD_SET);



                    foreach (var report in annReports)
                    {
                        PDFSearch.Search(report.ID, report.URL, keywords.Select(x => x.KEYWORD).ToList());
                    }
                });
            }
            catch (Exception ee)
            {
                string ss = ee.Message;
                return;
            }

        }
        public ActionResult GetAlertReportList(string alertName, DateTime alertDate)
        {
            AlertInsert(alertName, alertDate);
            var jsonResult = Json(data: DB.GetSearchedReport(alertName, alertDate, alertDate), JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;

            return jsonResult;
        }
        #endregion

        #region set favourites

        [HttpGet]
        public ActionResult SaveFavorite(int annId, int IsFavorite)
        {
            try
            {
                DB.SaveFavorite(annId, IsFavorite);
            }
            catch (Exception ex)
            {

            }

            return Content("");
        }

        [HttpGet]
        public ActionResult SaveCatFavorite(string annIds, int IsFavorite)
        {
            try
            {
                DB.SaveCatFavorite(annIds, IsFavorite);
            }
            catch (Exception ex)
            {

            }

            return Content("");
        }
        #endregion
    }
}