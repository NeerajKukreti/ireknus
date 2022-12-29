using Dapper;
using ExcelDataReader;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PDFReader.Helpers;
using PDFReader.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
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
                dp.Add("WATCHLIST_ID", WatchlistModel.WATCHLIST_ID);
                dp.Add("COMPANY_ID", WatchlistModel.COMPANY_ID);
                dp.Add("COMPANY_NAME",WatchlistModel.COMPANY_NAME);
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

                dp.Add("WATCHLIST_ID", 0);
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
                            list.Add(new WatchlistModel { COMPANY_ID = companycodes.ToString(), COMPANY_NAME=companynames.ToString() });
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

        public async Task announcement()
        {
            //var dtFrom = "20210401";
            //var dtFrom = DateTime.Now.ToString("yyyyMMdd");
            //var dtTo = "20210405";


            var dtFrom = DateTime.Now.AddDays(-1).ToString("yyyyMMdd");
            //var dtFrom = DateTime.Now.ToString("yyyyMMdd");
            var dtTo = DateTime.Now.ToString("yyyyMMdd");

            string dtFromForCount = DateTime.Now.AddDays(-1).ToString("dd-MM-yyyy");
            //var dtFrom = DateTime.Now.ToString("yyyyMMdd");
            string dtToForCount = DateTime.Now.ToString("dd-MM-yyyy");

            //var dtFrom = DateTime.Parse("03/04/2021").ToString("yyyyMMdd"); ;
            //var dtTo = DateTime.Parse("04/05/2021").ToString("yyyyMMdd"); ;

            int totalInsertedAnnouncementCount = DB.totalAnnouncmentCount(dtFromForCount, dtToForCount);

            using (var client = new WebClient()) //WebClient  
            {
                client.Headers.Add("Content-Type:application/json"); //Content-Type  
                client.Headers.Add("Accept:application/json");
                 var jsonresult = client.DownloadString
                    ($"https://api.bseindia.com/BseIndiaAPI/api/AnnGetData/w?strCat=-1&strPrevDate={dtFrom}&strScrip=&strSearch=P&strToDate={dtTo}&strType=C"); //URI  
                //Console.WriteLine(Environment.NewLine + result);

                //DataTable dt = (DataTable)JsonConvert.DeserializeObject(result, (typeof(DataTable)));
                var x = JObject.Parse(jsonresult);
                try
                {
                    var result = JsonConvert.DeserializeObject<Root>(jsonresult);
                    var rowcount = result.Table1[0].ROWCNT;

                    int rowsTobeInserted = Convert.ToInt32(rowcount) - totalInsertedAnnouncementCount;
                    int noOfApiCalls = rowsTobeInserted / 50;
                    int page = 0;

                    for (int i = 1; i <= noOfApiCalls; i++)
                    {
                        page = i + 1;
                        var jsonresultToAppend = client.DownloadString
                    ($"https://api.bseindia.com/BseIndiaAPI/api/AnnGetData/w?pageno={page}&strCat=-1&strPrevDate={dtFrom}&strScrip=&strSearch=P&strToDate={dtTo}&strType=C"); //URI 

                        var resultToAppend = JsonConvert.DeserializeObject<Root>(jsonresultToAppend);

                        result.Table.AddRange(resultToAppend.Table);
                    }

                    await insertAnnouncement(result.Table);

                }
                catch (Exception ex) {
                
                }
            }
        }

        public async Task insertAnnouncement(List<AnnouncementResult> list)
        {
            try
            {
               
                //var list2 = list1.Take(20).ToList();

                list.ForEach(x =>
                {
                    if (x.News_submission_dt == null)
                    {
                        x.News_submission_dt = x.DissemDT;
                    }
                });

                var list1 = list.OrderBy(x => x.News_submission_dt).ToList();

                using (var connection = new SqlConnection(Connection.MyConnection()))
                {
                    connection.Open();

                    var dt = new DataTable();

                    dt.Columns.Add("NEWS_ID", typeof(string));
                    dt.Columns.Add("COMPANY_NAME", typeof(string));
                    dt.Columns.Add("COMPANY_ID", typeof(string));
                    dt.Columns.Add("NEWS_SUBJECT", typeof(string));
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
                        dt.Rows.Add(item.NEWSID, item.SLONGNAME, item.SCRIP_CD, item.NEWSSUB, item.HEADLINE, item.MORE, item.ANNOUNCEMENT_TYPE, item.QUARTER_ID, 
                                    item.ATTACHMENTNAME, item.CATEGORYNAME, item.NSURL, item.AGENDA_ID, item.News_submission_dt, item.DissemDT, item.TimeDiff);
                        
                    }

                    for (int i = 0; i < dt.Rows.Count; i = i + 1000)
                    {
                        var items = dt.Rows.Cast<System.Data.DataRow>().Skip(i).Take(1000);

                        DataTable dataTable = new DataTable();
                        dataTable = items.CopyToDataTable();

                        var xx = await connection.ExecuteAsync("sp_InsertAnnouncement", new { @announcementType = dataTable.AsTableValuedParameter("AnnouncementType") }, commandType: CommandType.StoredProcedure);
                    }

                    
                }
            }
            catch (Exception ee)
            {
                
            }
            
        }
    }
}