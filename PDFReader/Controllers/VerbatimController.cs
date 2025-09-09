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

        MemoryCache _cache = MemoryCache.Default;

        public VerbatimController() { }
        public async Task<ActionResult> Index(int reportId)
        {
            var result = await DB.GetFoundKeywordsByReportId(reportId);
             
            foreach (var item in result)
            {
                if (!string.IsNullOrEmpty(item.Url))
                {
                    _cache.Set($"URL", item.Url, DateTimeOffset.Now.AddMinutes(30));
                    break; // Use first URL found
                }
            }
            
            return View(result);
        }

        public async Task<ActionResult> SearchKeyword(string Keyword)
        { 
            var url = _cache.Get($"URL") as string;
            
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