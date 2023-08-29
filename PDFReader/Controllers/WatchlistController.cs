using Dapper;
using ExcelDataReader;
using Newtonsoft.Json;
using PDFReader.Helpers;
using PDFReader.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Hosting;
using System.Web.Mvc;

namespace PDFReader.Controllers
{
    public class WatchlistController : Controller
    {
        // GET: Watchlist
        [Authorize]
        public ActionResult Index()
        {
            WatchlistModel ob = new WatchlistModel();
            ob.ACTION = "1";
            return View("Watchlist", ob);
        }

        [HttpPost]
        public async Task<int> AddEditDeleteWatchlist(WatchlistModel WatchlistModel)
        {
            using (IDbConnection db = new SqlConnection(Connection.MyConnection()))
            {
                DynamicParameters dp = new DynamicParameters();
                dp.Add("COMPANY_ID", WatchlistModel.COMPANY_ID);
                dp.Add("COMPANY_NAME", WatchlistModel.COMPANY_NAME);
                dp.Add("ACTION", WatchlistModel.ACTION);

                try
                {
                    return await db.ExecuteScalarAsync<int>("SP_WATCHLIST", dp, commandType: CommandType.StoredProcedure);
                }
                catch (Exception ee)
                {
                    return 0;
                }
            }
        }

        public async Task<ActionResult> LoadWatchlist()
        {
            using (IDbConnection db = new SqlConnection(Connection.MyConnection()))
            {
                DynamicParameters dp = new DynamicParameters();

                dp.Add("COMPANY_ID", "");
                dp.Add("COMPANY_NAME", "");
                dp.Add("ACTION", 4);

                try
                {
                    return Json(new { data = await db.QueryAsync<WatchlistModel>("SP_WATCHLIST", dp, commandType: CommandType.StoredProcedure) }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception ee)
                {
                    return View("Watchlist", new WatchlistModel());
                }

            }
        }

        [HttpPost]
        public string UploadFile()
        {
            string filepath = FileHandler.SaveFile(Request, "Watchlist");
            return (InsertFileData(filepath));
        }

        private string InsertFileData(string filepath)
        {
            #region Watchlist Company Code Insertion

            var filename = filepath;

            //return (filename);

            List<WatchlistModel> list = new List<WatchlistModel>();

            #region EDR

            using (var stream = System.IO.File.Open(filename, FileMode.Open, FileAccess.Read))
            {
                // Auto-detect format, supports:
                //  - Binary Excel files (2.0-2003 format; *.xls)
                //  - OpenXml Excel files (2007 format; *.xlsx, *.xlsb)
                using (var reader = ExcelReaderFactory.CreateReader(stream))
                {
                    // Choose one of either 1 or 2:
                    // 1. Use the reader methods
                    //do
                    //{
                    while (reader.Read())
                    {
                        var companycodes = reader.GetValue(0) ?? "";
                        var companynames = reader.GetValue(1) ?? "";

                        if (!string.IsNullOrEmpty(companycodes.ToString().Trim()) || !string.IsNullOrEmpty(companynames.ToString().Trim()))
                            list.Add(new WatchlistModel { COMPANY_ID = companycodes.ToString(), COMPANY_NAME = companynames.ToString() });
                        // reader.GetDouble(0);
                    }
                    //} //while (reader.NextResult());

                    // 2. Use the AsDataSet extension method
                    //var result = reader.AsDataSet();

                    // The result of each spreadsheet is in result.Tables
                }
            }

            #endregion EDR

            list.RemoveAt(0);
            DB.insertWatchlistData(list);

            #endregion Watchlist Company Code Insertion

            return "All Watchlist Company Code Added Successfully";
        }

        public async Task JobLoop()
        {
            DateTime st = Convert.ToDateTime("01/01/2023"); //DD/MM/YYYY
            DateTime end = Convert.ToDateTime("24/02/2023");

            for (var dt = st; dt <= end; dt = dt.AddDays(1))
            {
                await announcement1(dt);
            }
        }
        public async Task announcement1(DateTime dt1)
        {
            var frm = dt1;
            var to = dt1;

            //var frm = DateTime.Now.AddDays(-1);
            //var to = DateTime.Now.AddDays(0);

            var dtFrom = frm.ToString("yyyyMMdd");
            var dtTo = to.ToString("yyyyMMdd");

            string dtFromForCount = frm.ToString("dd-MM-yyyy");
            string dtToForCount = to.ToString("dd-MM-yyyy");

            int totalInsertedAnnouncementCount = DB.totalAnnouncmentCount(dtFromForCount, dtToForCount);
            var dt = DB.GetLastAnnDateTime(dtFromForCount, dtToForCount);

            using (var client = new WebClient()) //WebClient  
            {
                client.Headers.Add("Content-Type:application/json"); //Content-Type  
                client.Headers.Add("Accept:application/json");
                var jsonresult = client.DownloadString
                   ($"https://api.bseindia.com/BseIndiaAPI/api/AnnGetData/w?strCat=-1&strPrevDate={dtFrom}&strScrip=&strSearch=P&strToDate={dtTo}&strType=C"); //URI  

                try
                {
                    var result = JsonConvert.DeserializeObject<Root>(jsonresult);
                    var rowcount = result.Table1[0].ROWCNT;
                    //totalInsertedAnnouncementCount = 0; 
                    int rowsTobeInserted = Convert.ToInt32(rowcount) - totalInsertedAnnouncementCount;
                    int noOfApiCalls = rowsTobeInserted / 50 + (rowsTobeInserted > 50 && (rowsTobeInserted % 50) > 0 ? 1 : 0);

                    var allList = new List<AnnouncementResult>();
                    allList.AddRange(result.Table);

                    if (noOfApiCalls > 0)
                    {
                        //for (int i = noOfApiCalls; i >= 2; i--)
                        for (int i = 2; i <= noOfApiCalls; i++)
                        {
                            var jsonresultToAppend = client.DownloadString
                        ($"https://api.bseindia.com/BseIndiaAPI/api/AnnGetData/w?pageno={i}&strCat=-1&strPrevDate={dtFrom}&strScrip=&strSearch=P&strToDate={dtTo}&strType=C"); //URI 

                            var resultToAppend = JsonConvert.DeserializeObject<Root>(jsonresultToAppend);
                            allList.AddRange(resultToAppend.Table);
                            //var annlist = resultToAppend.Table.Where(x => x.DissemDT > dt).ToList();
                            //List<KeyValuePair<string, int>> annCateList = new List<KeyValuePair<string, int>>();
                            //AnnouncementBL.PerformSearch(DB.GetCategories().ToList(), annlist, annCateList);
                            //await insertAnnouncement(annlist, annCateList);
                        }
                    }

                    if (allList.Any())
                    {
                        var newList = allList.Where(x => x.DT_TM > dt).ToList(); //uncomment
                        //var newList = allList;
                        List<KeyValuePair<string, int>> RepeatedAnnList = new List<KeyValuePair<string, int>>();
                        //var annList = result.Table.Where(x => x.DissemDT > dt).ToList();

                        if (newList.Any())
                        {
                            AnnouncementBL.PerformSearch(DB.GetCategories().ToList(), newList, RepeatedAnnList);
                            await insertAnnouncement(newList, RepeatedAnnList);
                        }
                    }
                }
                catch (Exception ex)
                {

                }
            }
        }

        public async Task announcement()
        {
            //var frm = dt1;
            //var to = dt1;

            var frm = DateTime.Now.AddDays(-1);
            var to = DateTime.Now.AddDays(0);

            var dtFrom = frm.ToString("yyyyMMdd");
            var dtTo = to.ToString("yyyyMMdd");

            string dtFromForCount = frm.ToString("dd-MM-yyyy");
            string dtToForCount = to.ToString("dd-MM-yyyy");

            int totalInsertedAnnouncementCount = DB.totalAnnouncmentCount(dtFromForCount, dtToForCount);
            var dt = DB.GetLastAnnDateTime(dtFromForCount, dtToForCount);

            using (var client = new WebClient()) //WebClient  
            {
                client.Headers.Add("Content-Type:application/json"); //Content-Type  
                client.Headers.Add("Accept:application/json");
                var jsonresult = client.DownloadString
                   //($"https://api.bseindia.com/BseIndiaAPI/api/AnnGetData/w?strCat=-1&strPrevDate={dtFrom}&strScrip=&strSearch=P&strToDate={dtTo}&strType=C"); //URI  
                   ($"https://api.bseindia.com/BseIndiaAPI/api/AnnSubCategoryGetData/w?strCat=-1&strPrevDate={dtFrom}&strScrip=&strSearch=P&strToDate={dtTo}&strType=C"); //URI  

                try
                {
                    var result = JsonConvert.DeserializeObject<Root>(jsonresult);
                    var rowcount = result.Table1[0].ROWCNT;
                    //totalInsertedAnnouncementCount = 0; 
                    int rowsTobeInserted = Convert.ToInt32(rowcount) - totalInsertedAnnouncementCount;
                    int noOfApiCalls = rowsTobeInserted / 50 + (rowsTobeInserted > 50 && (rowsTobeInserted % 50) > 0 ? 1 : 0);

                    var allList = new List<AnnouncementResult>();
                    allList.AddRange(result.Table);

                    if (noOfApiCalls > 0)
                    {
                        //for (int i = noOfApiCalls; i >= 2; i--)
                        for (int i = 2; i <= noOfApiCalls; i++)
                        {
                            var jsonresultToAppend = client.DownloadString
                        ($"https://api.bseindia.com/BseIndiaAPI/api/AnnSubCategoryGetData/w?pageno={i}&strCat=-1&strPrevDate={dtFrom}&strScrip=&strSearch=P&strToDate={dtTo}&strType=C"); //URI 

                            var resultToAppend = JsonConvert.DeserializeObject<Root>(jsonresultToAppend);
                            allList.AddRange(resultToAppend.Table);
                            //var annlist = resultToAppend.Table.Where(x => x.DissemDT > dt).ToList();
                            //List<KeyValuePair<string, int>> annCateList = new List<KeyValuePair<string, int>>();
                            //AnnouncementBL.PerformSearch(DB.GetCategories().ToList(), annlist, annCateList);
                            //await insertAnnouncement(annlist, annCateList);
                        }
                    }

                    if (allList.Any())
                    {
                        var newList = allList.Where(x => x.DT_TM > dt).ToList(); //uncomment
                        //var newList = allList;
                        List<KeyValuePair<string, int>> RepeatedAnnList = new List<KeyValuePair<string, int>>();
                        //var annList = result.Table.Where(x => x.DissemDT > dt).ToList();

                        if (newList.Any())
                        {
                            AnnouncementBL.PerformSearch(DB.GetCategories().ToList(), newList, RepeatedAnnList);
                            await insertAnnouncement(newList, RepeatedAnnList);
                        }
                    }
                }
                catch (Exception ex)
                {

                }
            }
        }

        public async Task<ActionResult> UploadAnnouncement()
        {
            return View();
        }

        [HttpPost]
        public async Task<int> LoadAnnFromHARFile()
        {
            FileHandler.UploadHARFile(Request);
            string path = HostingEnvironment.MapPath(ConfigurationManager.AppSettings["UploadPath"] + "/annoucements.har");
            var jsonData = HarHandler.GetJsonFromHARFile(path);
            var dt = DB.GetLastAnnDateTime();

            try
            {
                var result = JsonConvert.DeserializeObject<Root>(jsonData);
                var newList = result?.Table.Where(x => x.DT_TM > dt).Distinct().ToList();
                newList = newList.GroupBy(x => x.NEWSID).Select(x => x.FirstOrDefault()).ToList();

                if (newList.Any())
                {
                    for (int i = 0; i < newList.Count; i += 100)
                    {
                        var items = newList.Skip(i).Take(100).ToList();
                        List<KeyValuePair<string, int>> RepeatedAnnList = new List<KeyValuePair<string, int>>();
                        AnnouncementBL.PerformSearch(DB.GetCategories().ToList(), newList, RepeatedAnnList);
                        await insertAnnouncement(items, RepeatedAnnList);
                    }
                }
                return (newList!=null?newList.Count:0);
            }
            catch (Exception ex)
            {
                var err = ex;
                return 0;
            }
        }
        public async Task insertAnnouncement(List<AnnouncementResult> list, List<KeyValuePair<string, int>> annList)
        {
            try
            {
                list.ForEach(x =>
                {
                    if (x.News_submission_dt == null)
                    {
                        x.News_submission_dt = x.DissemDT;
                    }
                });

                var list1 = list.OrderBy(x => x.News_submission_dt).ToList();

                var anns = annList.Where(x => list1.Select(y => y.NEWSID).Contains(x.Key));

                using (var connection = new SqlConnection(Connection.MyConnection()))
                {
                    connection.Open();

                    var dt = new DataTable();

                    dt.Columns.Add("NEWS_ID", typeof(string));
                    dt.Columns.Add("COMPANY_NAME", typeof(string));
                    dt.Columns.Add("COMPANY_ID", typeof(string));
                    dt.Columns.Add("NEWS_SUBJECT", typeof(string));
                    dt.Columns.Add("DT_TM", typeof(DateTime));
                    dt.Columns.Add("NEWS_DT", typeof(DateTime));
                    dt.Columns.Add("HEAD_LINE", typeof(string));
                    dt.Columns.Add("MORE", typeof(string));
                    dt.Columns.Add("ANNOUNCEMENT_TYPE", typeof(string));
                    dt.Columns.Add("QUARTER_ID", typeof(string));
                    dt.Columns.Add("ATTACHMENT", typeof(string));
                    dt.Columns.Add("CATEGORY_NAME_BY_BSE", typeof(string));
                    dt.Columns.Add("NSURL", typeof(string));
                    dt.Columns.Add("AGENDA_ID", typeof(int));
                    dt.Columns.Add("NEWS_SUBMISSION_DATE", typeof(DateTime));
                    dt.Columns.Add("DISSEMINATION_DATE", typeof(DateTime));
                    dt.Columns.Add("TIME_DIFFERENCE", typeof(string));

                    foreach (var item in list1)
                    {
                        dt.Rows.Add(item.NEWSID, item.SLONGNAME, item.SCRIP_CD, item.NEWSSUB, item.DT_TM, item.NEWS_DT, item.HEADLINE, item.MORE, item.ANNOUNCEMENT_TYPE, item.QUARTER_ID,
                                    item.ATTACHMENTNAME, item.CATEGORYNAME, item.NSURL, 0, item.News_submission_dt, item.DissemDT, item.TimeDiff);
                    }

                    var annCates = new DataTable();

                    annCates.Columns.Add("NEWS_ID", typeof(string));
                    annCates.Columns.Add("CATEGORY_ID", typeof(int));

                    foreach (var item in anns)
                    {
                        annCates.Rows.Add(item.Key, item.Value);
                    }

                    var xx = await connection
                        .QueryAsync("sp_InsertAnnouncement",
                        new
                        {
                            @announcementType = dt.AsTableValuedParameter("AnnouncementType"),
                            @annCatesType = annCates.AsTableValuedParameter("AnnCatesType")
                        }, commandType: CommandType.StoredProcedure);

                }

            }
            catch (Exception ee)
            {

            }

        }
    }
}