using PDFReader.Model;
using PDFReader.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.WebPages;

namespace PDFReader.Controllers
{
    [Authorize]
    public class ReportController : Controller
    {
        // GET: Dashboard
        public async Task<ActionResult> Index()
        {

            var guid = Guid.NewGuid();

            DataTable dt = new DataTable();
            dt.Columns.Add("WebGUID", typeof(Guid));

            dt.Rows.Add(guid);


            DataRow[] drList = dt.Select("WebGUID = '" + guid.ToString() + "'");
            var xx = dt.Select("WebGUID = '" + guid.ToString() + "'");

            var list = await DB.GetFinancialYears();
            var keywordset = DB.GetKeywordSet();

            ReportViewModel reportViewModel = new ReportViewModel
            {
                KeywordSet = keywordset.Select(x =>
                   new SelectListItem
                   {
                       Text = x.SM_NAME.ToString(),
                       Value = x.SMID.ToString()
                   }).ToList(),
                ReportList = list.ToList()
            };

            return View("Report", reportViewModel);
        }

        public async Task<ActionResult> GetReportData(String FinancialYear)
        {
            ReportResult reportResult = DB.GetPrcoessedReport(FinancialYear);
            var xx = Json(data: reportResult, JsonRequestBehavior.AllowGet);
            xx.MaxJsonLength = int.MaxValue;

            return xx;
        }

        public async Task<ActionResult> GetSearchedReport(string Year, string query = "", bool queryEnabled = false)
        {
            query = HttpUtility.UrlDecode(query);
            IEnumerable<KeywordResult> reportResult = DB.GetSearchedReport(Year, query, queryEnabled);
            Session["searchresult"] = reportResult;
            var xx = Json(data: reportResult, JsonRequestBehavior.AllowGet);
            xx.MaxJsonLength = int.MaxValue;
            return xx;
        }

        public async Task PerformSearch(string Year, string KeywordSetId)
        {
            var reports = DB.GetReports(Year, DateTime.Now).ToList();
            var Keywords = DB.GetKeywords(KeywordSetId);

            reports = reports.Take(2000).ToList();
            //reports = reports.Where(x => x.CompanyName.ToLower().Equals("tcs financial year 2021")).ToList();
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            Parallel.ForEach(reports, report =>
            {
                PDFSearch.Search(report.ID, report.URL, Keywords.Select(x => x.KEYWORD).ToList());
            });

            //var report = reports.FirstOrDefault();
            //PDFSearch.Search(report.ID, report.URL, Keywords.Select(x => x.KEYWORD).ToList());

            stopWatch.Stop();
            // Get the elapsed time as a TimeSpan value.
            TimeSpan ts = stopWatch.Elapsed;

            // Format and display the TimeSpan value.

            string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                ts.Hours, ts.Minutes, ts.Seconds,
                ts.Milliseconds / 10);
        }

        public async Task DeleteReport(string Year)
        {
            await DB.DeleteReport(Year);
        }

        public async Task<ActionResult> GetSkippedCount(string Year)
        {
            return Json(data: DB.GetSkippedCount(Year), JsonRequestBehavior.AllowGet);
        }

        public async Task<ActionResult> GetUnproccesedRpt(string year, int type)
        {
            return Json(data: DB.GetUnproccesedRpt(year, type).ToList(), JsonRequestBehavior.AllowGet);
        }

        public async Task DeleteCompany(string year)
        {
            DB.DeleteCompany(year);
        }
        public ActionResult GetAllCompanies()
        {
            var list = DB.GetAllCompanies().Take(100);
            return Json(data:
                list.Select(x => new { value = x.COMPANY_ID, text = x.COMPANY_NAME }).Distinct(), JsonRequestBehavior.AllowGet);
        }

        #region deep search
        public async Task<ActionResult> AdvanceSearch(string reportid)
        {
            var reportResult = (IEnumerable<KeywordResult>)Session["searchresult"];
            reportResult = reportResult.Where(x => x.ReportId == int.Parse(reportid));
            if (reportResult == null)
                return Content("retry from execute search");

            return View("DeepSearch", reportResult);
        }
        #endregion

        public async Task<string> PerformParaSearch(string URL)
        {
            Uri url = new Uri(URL);
            int val = int.Parse(url.Fragment.Split('=')[1]);
            await PDFSearch.SearchPara(val,URL);
            return "";
        }
    }
}