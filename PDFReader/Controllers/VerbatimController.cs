using System;
using System.Collections.Generic;
using System.Runtime.Caching;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace PDFReader.Controllers
{
    public class VerbatimController : Controller
    {

        MemoryCache _cache = MemoryCache.Default;

        public VerbatimController() { }
        public async Task<ActionResult> Index(string reportId)
        {
            var cachedReports = new HashSet<string>();
            var reportIds = reportId.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

            var result = await DB.GetFoundKeywordsByReportId(reportIds);
             
            foreach (var item in result)
            {
                if (!string.IsNullOrEmpty(item.Url) && !cachedReports.Contains(item.ReportId.ToString()))
                {
                    _cache.Set($"URL_{item.ReportId.ToString()}", item.Url, DateTimeOffset.Now.AddMinutes(30));
                    cachedReports.Add(item.ReportId.ToString());
                }
            }
            
            return View(result);
        }

        public async Task<ActionResult> SearchKeyword(string Keyword, string reportId)
        {
            var url = _cache.Get($"URL_{reportId}") as string;

            var phrases = await PDFSearch.GetPhrases(url, Keyword);
            return Json(phrases, JsonRequestBehavior.AllowGet);
        }
        public async Task<string> GetPDFText()
        {
            string cachedData = _cache["PDF_Text"] as string;
            return cachedData;
        }
    }
}