using Dapper;
using iTextSharp.text;
using Newtonsoft.Json;
using PDFReader.Model;
using PDFReader.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.UI;
using System.Runtime.Caching;

namespace PDFReader.Controllers
{
    public class VerbatimController : Controller
    {
        public VerbatimController() { }
        public async Task<ActionResult> Index(int reportId = 84489)
        {
            var result = await DB.GetFoundKeywordsByReportId(reportId);
            return View(result);
        }

        public async Task<ActionResult> SearchKeyword(string URL, string Keyword)
        {
            var phrases = await PDFSearch.GetPhrases(URL, Keyword);
            return Json(phrases, JsonRequestBehavior.AllowGet);
        }
        public async Task<string> GetPDFText()
        {
            MemoryCache _cache = MemoryCache.Default;
            string cachedData = _cache["PDF_Text"] as string;
            return cachedData;
        }
    }
}