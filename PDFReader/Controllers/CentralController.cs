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
    public class CentralController : Controller
    {
        // GET: Dashboard
        public async Task<ActionResult> Index()
        {

            var guid = Guid.NewGuid();

            DataTable dt = new DataTable();
            dt.Columns.Add("WebGUID", typeof(Guid));

            dt.Rows.Add(guid);


            DataRow[] drList = dt.Select("WebGUID = '" + guid.ToString() + "'");
            var xx = dt.Select("WebGUID = '" + guid.ToString() + "'");

            CentralModel ob = new CentralModel();

            return View("Central", ob);
        }

        [HttpGet]
        public async Task<ActionResult> LoadCentralData()
        {
            using (IDbConnection db = new SqlConnection(Connection.MyConnection()))
            {
                try
                {
                    string str_query = @"SELECT dbo.parivesh_central.Record_ID, dbo.VIEW_LATEST_CENTRAL.Proposal_No, dbo.parivesh_central.MOEFCC_File_No, 
                    dbo.parivesh_central.Project_Name, dbo.parivesh_central.Company, dbo.parivesh_central.Proposal_Status, 
                    REPLACE(REPLACE(REPLACE(dbo.parivesh_central.Location,'State : ',''),'District :',''),'Tehsil :','') Location, 
                    REPLACE(REPLACE(dbo.parivesh_central.Important_Dates,'Date of ',''),'Submission for ','') Important_Dates, 
                    dbo.parivesh_central.Category, dbo.parivesh_central.Company_or_Proponent, dbo.parivesh_central.Type_of_project, dbo.parivesh_central.Attached_Files, 
                    dbo.parivesh_central.Acknowlegment, dbo.parivesh_central.Pagination, dbo.parivesh_central.input_company_name, dbo.parivesh_central.subsidiary_name, dbo.parivesh_central.DateTimeStamp
                    FROM dbo.parivesh_central INNER JOIN
                    dbo.VIEW_LATEST_CENTRAL ON dbo.parivesh_central.Record_ID = dbo.VIEW_LATEST_CENTRAL.Record_ID 
                    order by dbo.parivesh_central.DateTimeStamp desc";
                    
                    List<CentralModel> data =new List<CentralModel>(await db.QueryAsync<CentralModel>(str_query, commandType: CommandType.Text));
                    foreach(var central in data)
                    {
                        string str_file = "";
                        if(!string.IsNullOrWhiteSpace(central?.Attached_Files))
                        {
                            var attachedFiles = central?.Attached_Files?.Split(',');
                            foreach (var filedetails in attachedFiles)
                            {
                                if (!string.IsNullOrWhiteSpace(filedetails))
                                {
                                    string[] details = filedetails.Split('~');
                                    str_file += "<a href='" + details[1] + "' target='_blank'>" + details[0] + "</a>,</br>";
                                }
                            }
                        }                   

                        central.Files_Detail= str_file;

                        if(!string.IsNullOrWhiteSpace(central.Acknowlegment))
                            central.Acknowlegment = "<a href='"+ central.Acknowlegment + "' target='_blank' style='color:#048d34;font-weight: bold;'>Ack</a>";
                    }
                    var test = Json(data, JsonRequestBehavior.AllowGet);
                    test.MaxJsonLength = int.MaxValue;
                    return test;
                }
                catch (Exception ee)
                {
                    CentralModel ob = new CentralModel();
                    return View("Central", ob);
                }
            }
        }
    }
}