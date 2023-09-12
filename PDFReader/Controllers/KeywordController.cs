using Dapper;
using ExcelDataReader;
using PDFReader.Helpers;
using PDFReader.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace PDFReader.Controllers
{
    [Authorize]
    public class KeywordController : Controller
    {
        public ActionResult Index()
        {
            KeywordModel ob = new KeywordModel();

            ob.ACTION = "1";

            var keywordset = DB.GetKeywordSet();

            ob.KEYWORDSET = keywordset.Select(x =>
            new SelectListItem
            {
                Text = x.SM_NAME.ToString(),
                Value = x.SMID.ToString()
            }).ToList();

            return View("Keyword", ob);
        }

        [HttpPost]
        public async Task<int> AddKeyword(KeywordModel KeywordModel)
        {
            using (IDbConnection db = new SqlConnection(Connection.MyConnection()))
            {
                DynamicParameters dp = new DynamicParameters();
                dp.Add("KID", KeywordModel.KID);
                dp.Add("KEYWORD", KeywordModel.KEYWORD);
                dp.Add("SMID", KeywordModel.SMID);
                dp.Add("ACTION", KeywordModel.ACTION);

                try
                {
                    return await db.ExecuteScalarAsync<int>("SP_KEYWORD", dp, commandType: CommandType.StoredProcedure);
                    //return 0;
                }
                catch (Exception ee)
                {
                    return 0;
                }
            }
        }

        [HttpPost]
        public async Task<int> EditKeyword(KeywordModel KeywordModel)
        {
            using (IDbConnection db = new SqlConnection(Connection.MyConnection()))
            {
                DynamicParameters dp = new DynamicParameters();
                dp.Add("KID", KeywordModel.KID);
                dp.Add("KEYWORD", KeywordModel.KEYWORD);
                dp.Add("ACTION", 2);

                try
                {
                    return await db.ExecuteScalarAsync<int>("SP_KEYWORD", dp, commandType: CommandType.StoredProcedure);
                }
                catch (Exception ee)
                {
                    return 0;
                }
            }
        }

        [HttpPost]
        public async Task<int> DeleteKeyword(KeywordModel KeywordModel)
        {
            using (IDbConnection db = new SqlConnection(Connection.MyConnection()))
            {
                DynamicParameters dp = new DynamicParameters();
                dp.Add("KID", KeywordModel.KID);
                dp.Add("KEYWORD", KeywordModel.KEYWORD);
                dp.Add("ACTION", 3);

                try
                {
                    return await db.ExecuteScalarAsync<int>("SP_KEYWORD", dp, commandType: CommandType.StoredProcedure);
                }
                catch (Exception ee)
                {
                    return 0;
                }
            }
        }

        public async Task<ActionResult> LoadKeyword(string keywordSetId)
        {
            using (IDbConnection db = new SqlConnection(Connection.MyConnection()))
            {
                return Json(new { data = await db.QueryAsync<KeywordModel>(@"SELECT KID,KEYWORD,TBL_KEYWORDS.SMID,Tbl_KeywordSetMaster.SM_NAME FROM [dbo].[TBL_KEYWORDS] INNER JOIN Tbl_KeywordSetMaster 
on TBL_KEYWORDS.SMID=Tbl_KeywordSetMaster.SMID WHERE TBL_KEYWORDS.SMID=" + keywordSetId + " ORDER BY KEYWORD") }, JsonRequestBehavior.AllowGet);
            }

        }

        [HttpPost]
        public string UploadFile()
        {
            string filepath = FileHandler.SaveFile(Request, "Keywords");
            string smid = Request.Params["SMID"].ToString();
            return (InsertFileData(filepath, smid));
        }

        private string InsertFileData(string filepath, string smid)
        {
            #region kwyords insertion

            var filename = filepath;

            //return (filename);

            List<KeywordModel> list = new List<KeywordModel>();

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
                        var searchkeywords = (string)reader.GetValue(0) ?? "";

                        if (!string.IsNullOrEmpty(searchkeywords.Trim()))
                            list.Add(new KeywordModel { KEYWORD = searchkeywords, SMID = smid });
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
            DB.insertKeywordsData(list);

            #endregion kwyords insertion

            return "All Keywords Added Successfully";
        }
    }
}