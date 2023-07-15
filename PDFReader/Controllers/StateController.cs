using Dapper;
using Newtonsoft.Json;
using PDFReader.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.UI;

namespace PDFReader.Controllers
{
    
    public class StateController : Controller
    {
        // GET: Dashboard
        public async Task<ActionResult> Index()
        {

            var guid = Guid.NewGuid();

            DataTable dt = new DataTable();
            dt.Columns.Add("WebGUID", typeof(Guid));

            dt.Rows.Add(guid);


            StateModel ob = new StateModel();

            return View("State", ob);
        }

        [HttpGet]
        public async Task<ActionResult> LoadStateData()
        {
            string spname = "SP_PARIVESH_STATE";
            if (Request.QueryString["type"] != null) spname = "SP_PARIVESH_STATE_ALL";
            using (IDbConnection db = new SqlConnection(Connection.MyConnection()))
            {
                try
                {
                    List<StateModel> data =new List<StateModel>(await db.QueryAsync<StateModel>(spname, commandType: CommandType.StoredProcedure));
                    foreach(var state in data)
                    {
                        string str_file = "";
                        if(!string.IsNullOrWhiteSpace(state?.Attached_Files))
                        {
                            var attachedFiles = state?.Attached_Files?.Split(',');
                            foreach (var filedetails in attachedFiles)
                            {
                                if (!string.IsNullOrWhiteSpace(filedetails))
                                {
                                    string[] details = filedetails.Split('~');
                                    str_file += "<a href='" + (1 < details.Length ? details[1] : details[0]) + "' target='_blank'>" + details[0] + "</a>,</br>";
                                }
                            }
                        }

                        state.Files_Detail = str_file;
                    }
                    var test = Json(data, JsonRequestBehavior.AllowGet);
                    test.MaxJsonLength = int.MaxValue;
                    return test;
                }
                catch (Exception ee)
                {
                    StateModel ob = new StateModel();
                    return View("StateAll", ob);
                }
            }
        }        
    }
}