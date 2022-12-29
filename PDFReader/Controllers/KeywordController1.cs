using Dapper;
using PDFReader.Models;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace PDFReader.Controllers
{
    public class KeywordController1 : Controller
    {
        // GET: Keyword
        [Authorize]
        public ActionResult Index()
        {
            return View("Keyword", new KeywordModel());
        }

        [HttpPost]
        public async Task<int> AddKeyword(KeywordModel KeywordModel)
        {
            using (IDbConnection db = new SqlConnection(Connection.MyConnection()))
            {
                DynamicParameters dp = new DynamicParameters();
                dp.Add("KID", KeywordModel.KID);
                dp.Add("KEYWORD", KeywordModel.KEYWORD);
                dp.Add("ACTION", 1);

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

        public async Task<ActionResult> LoadKeyword()
        {
            using (IDbConnection db = new SqlConnection(Connection.MyConnection()))
            {
                return Json(new { data = await db.QueryAsync<KeywordModel>("SELECT KID,KEYWORD FROM [dbo].[TBL_KEYWORDS] ORDER BY KEYWORD") }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}