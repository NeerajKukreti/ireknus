using Dapper;
using PDFReader.Models;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace PDFReader.Controllers
{
    public class CategoryController : Controller
    {
        // GET: Category
        [Authorize]
        public ActionResult Index()
        {
            AnnouncementCategoryModel ob = new AnnouncementCategoryModel();

            var parentcategory = DB.GetCategories();

            ob.ACTION = "1";

            ob.ParentCategory = parentcategory.Select(x =>
            new SelectListItem
            {
                Text = x.CATEGORY,
                Value = x.CATEGORY_ID.ToString()
            }).ToList();

            return View("Category", ob);
        }

        public async Task<ActionResult> LoadCategory()
        {
            using (IDbConnection db = new SqlConnection(Connection.MyConnection()))
            {
                DynamicParameters dp = new DynamicParameters();
                dp.Add("CATEGORY_ID", 0);
                dp.Add("CATEGORY", "");
                dp.Add("PARENT_ID", 0);
                dp.Add("SEARCH_VALUES", "");
                dp.Add("ACTION", 4);

                try
                {
                    return Json(new { data = await db.QueryAsync<AnnouncementCategoryModel>("SP_CATEGORY", dp, commandType: CommandType.StoredProcedure) }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception ee)
                {
                    return View("Category", new AnnouncementCategoryModel());
                }
            }
        }


        public string setPriority(string OrderValue)
        {
            try
            {
                string categoryorders = OrderValue;
                string[] categorypriority = categoryorders.Split('|');

                using (var connection = new SqlConnection(Connection.MyConnection()))
                {
                    connection.Open();

                    var dt = new DataTable();
                    dt.Columns.Add("categoryid");
                    dt.Columns.Add("priority");

                    foreach (var item in categorypriority)
                    {
                        string[] idvalue = item.Split(',');
                        dt.Rows.Add(idvalue[0], idvalue[1]);
                    }

                    var xx = connection.ExecuteAsync("sp_UpdatePriority", new { @categorypriorityType = dt.AsTableValuedParameter("UpdatePriorityType") }, commandType: CommandType.StoredProcedure).Result;
                    return ("Priority Updated Successfully");
                }
            }
            catch (Exception ee)
            {
                return ("Error: " + ee.Message);
            }


        }

        [HttpPost]
        public async Task<int> addEditCategory(AnnouncementCategoryModel ob)
        {
            using (IDbConnection db = new SqlConnection(Connection.MyConnection()))
            {
                DynamicParameters dp = new DynamicParameters();
                dp.Add("CATEGORY_ID", ob.CATEGORY_ID);
                dp.Add("CATEGORY", ob.CATEGORY);
                dp.Add("PARENT_ID", ob.PARENT_ID);
                dp.Add("SEARCH_VALUES", ob.SEARCH_VALUES);
                dp.Add("ACTION", ob.ACTION);

                try
                {
                    return await db.ExecuteScalarAsync<int>("SP_CATEGORY", dp, commandType: CommandType.StoredProcedure);
                }
                catch (Exception ee)
                {
                    return 0;
                }
            }
        }
    }
}