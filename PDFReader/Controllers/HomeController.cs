using Dapper;
using ExcelDataReader;
using PDFReader.Helpers;
using PDFReader.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace PDFReader.Controllers
{
    public class HomeController : Controller
    {

        public async Task<ActionResult> Index()
        {

            var list = await DB.GetFinancialYears();

            return View("index", list);
        }

        [HttpPost]
        public string UploadFile()
        {
            string filepath = FileHandler.SaveUploadedFile(Request);
            string finyear = Request.Params["FinantialYear"].ToString();
            return (InsertFileData(filepath, finyear));
        }

        private string InsertFileData(string filepath, string finyear)
        {
            #region report insertion

            var filename = filepath;
            List<Reports> list = new List<Reports>();

            #region EDR

            using (var stream = System.IO.File.Open(filename, FileMode.Open, FileAccess.Read))
            {
                using (var reader = ExcelReaderFactory.CreateReader(stream))
                {
                    while (reader.Read())
                    {
                        var doc = (string)reader.GetValue(0) ?? "";
                        var url = (string)reader.GetValue(1) ?? "";

                        if (!string.IsNullOrEmpty(doc.Trim()) || !string.IsNullOrEmpty(url.Trim()))
                        {
                            list.Add(new Reports { CompanyName = doc, URL = url, FinancialYear = finyear });
                        }
                    }
                }
            }

            #endregion EDR

            list.RemoveAt(0);
            //DB.insertCompaniesData(list);

            #endregion report insertion

            return "Data Added Successfully";
        }

        public async Task<ActionResult> LoadCompany(string financialyear)
        {
            using (IDbConnection db = new SqlConnection(Connection.MyConnection()))
            {
                try
                {
                    return Json(new { data = await db.QueryAsync<Reports>("SELECT CompanyName,Url FROM tbl_AnnualReports where FinancialYear='" + financialyear + "'", commandType: CommandType.Text) }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception ee)
                {
                    var list = await DB.GetFinancialYears();

                    return View("index", list);
                }
            }
        }
    }
}