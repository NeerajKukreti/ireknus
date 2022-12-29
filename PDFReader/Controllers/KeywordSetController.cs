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
    public class KeywordSetController : Controller
    {
        public ActionResult Index()
        {
            return View("KeywordSet", new KeywordSetModel());
        }

        [HttpPost]
        public async Task<int> AddKeyword(KeywordSetModel KeywordSetModel)
        {
            using (IDbConnection db = new SqlConnection(Connection.MyConnection()))
            {
                DynamicParameters dp = new DynamicParameters();
                dp.Add("SMID", KeywordSetModel.SMID);
                dp.Add("SM_NAME", KeywordSetModel.SM_NAME);
                dp.Add("ACTION", KeywordSetModel.ACTION);

                try
                {
                    return await db.ExecuteScalarAsync<int>("SP_KEYWORDSET", dp, commandType: CommandType.StoredProcedure);
                }
                catch (Exception ee)
                {
                    return 0;
                }
            }
        }

        

        public async Task<ActionResult> LoadKeyword()
        {
            using (IDbConnection db = new SqlConnection(Connection.MyConnection()))
            {
                return Json(new { data = await db.QueryAsync<KeywordSetModel>("SELECT SMID,SM_NAME FROM [dbo].[Tbl_KeywordSetMaster] ORDER BY SM_NAME") }, JsonRequestBehavior.AllowGet);
            }
        }

        
    }
}