using DocumentFormat.OpenXml.Bibliography;
using PDFReader.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Text;
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
            ClearCache();

            var cachedReports = new HashSet<string>();
            var reportIds = reportId.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

            var result = await DB.GetFoundKeywordsByReportId(reportIds);

            var uniqueReportIds = result.Select(x => new { x.ReportId, x.Url }).Distinct();

            foreach (var item in uniqueReportIds)
            {
                if (!string.IsNullOrEmpty(item.Url) && !cachedReports.Contains(item.ReportId.ToString()))
                {
                    _cache.Set($"URL_{item.ReportId.ToString()}", item.Url, DateTimeOffset.Now.AddMinutes(30));
                    cachedReports.Add(item.ReportId.ToString());
                }
            }

            // change: build unique keyword -> reportIds mapping
            var uniqueKeywords = result
                .Where(r => !string.IsNullOrWhiteSpace(r.FoundKeywords))            // ignore empty keywords
                .GroupBy(r => r.FoundKeywords.Trim(), StringComparer.OrdinalIgnoreCase) // group case-insensitively on trimmed text
                .Select(g => new Verbatim
                {
                    FoundKeywords = g.Key, // group's trimmed keyword (preserves casing from first occurrence)
                    ReportIds = string.Join(",", g
                        .Select(x => x.ReportId)         // get ints
                        .Distinct()                      // unique report ids
                        .OrderBy(id => id)               // order ascending (optional)
                        .Select(id => id.ToString()))    // convert to string for join
                }).OrderBy(x=> x.FoundKeywords)
                .ToList();

            return View(uniqueKeywords);
        }

        public async Task<ActionResult> SearchKeyword(string Keyword, string reportId)
        {
            ClearCache();
            var reportIds = reportId.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            var allPhrases = new List<FetchedPhrase>();
            foreach (var report in reportIds)
            {
                var url = _cache.Get($"URL_{report}") as string;
                var phrases = await PDFSearch.GetPhrases(report, url, Keyword);
                if (phrases != null)
                    allPhrases.AddRange(phrases);
            }

            return Json(allPhrases, JsonRequestBehavior.AllowGet);
        }
        public async Task<string> GetPDFText()
        {
            var allKeys = _cache
            .Select(kvp => kvp.Key)
            .Where(key => key.All(char.IsDigit))
            .OrderBy(key => int.Parse(key))   // sort numerically
            .ToList();

            var builder = new StringBuilder();

            foreach (var key in allKeys)
            {
                if (_cache[key] is string pdfText && !string.IsNullOrWhiteSpace(pdfText))
                {
                    builder.AppendLine(pdfText);
                }
            }

            return builder.ToString();
        }

        private void ClearCache() {
            var numericKeys = _cache
            .Select(kvp => kvp.Key)
            .Where(key => key.All(char.IsDigit))
            .ToList();  // materialize before removing

            foreach (var key in numericKeys)
            {
                _cache.Remove(key);
            }
        }

    }
}