using Dapper;
using Newtonsoft.Json;
using PDFReader.Model;
using PDFReader.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Text;
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

                    List<CentralModel> data = new List<CentralModel>(await db.QueryAsync<CentralModel>(str_query, commandType: CommandType.Text));
                    foreach (var central in data)
                    {
                        string str_file = "";
                        if (!string.IsNullOrWhiteSpace(central?.Attached_Files))
                        {
                            var attachedFiles = central?.Attached_Files?.Split(',');
                            foreach (var filedetails in attachedFiles)
                            {
                                if (!string.IsNullOrWhiteSpace(filedetails))
                                {
                                    string[] details = filedetails.Split('~');
                                    str_file += "<a href='" + (1 < details.Length ? details[1] : details[0]) + "' target='_blank'>" + details[0] + "</a>,<br />";
                                }
                            }
                        }

                        central.Files_Detail = str_file;

                        if (!string.IsNullOrWhiteSpace(central.Acknowlegment))
                            central.Acknowlegment = "<a href='" + central.Acknowlegment + "' target='_blank' style='color:#048d34;font-weight: bold;'>Ack</a>";
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

        public void CentralAlert()
        {
            var data = CentralDB.CheckandLogNewChanges().Result;

            if (data != null && data.Any())
            {
                SendMail(data);
            }
        }

        private void SendMail(List<CentralModel> list)
        {
            var to = ConfigurationManager.AppSettings["Mail-to"];
            var from = ConfigurationManager.AppSettings["Mail-from"];
            var cc = ConfigurationManager.AppSettings["Mail-cc"];
            var bcc = ConfigurationManager.AppSettings["Mail-bcc"];
            var passcode = ConfigurationManager.AppSettings["Mail-passcode"];

            MailModel objModelMail = new MailModel
            {
                To = to,
                Subject = $"Proposal Alert | {DateTime.Now.AddDays(0).ToString("dd/MM/yyyy")}",
                Body = SetBodyContent(list)
            };

            using (MailMessage mail = new MailMessage(from, objModelMail.To))
            {
                mail.Subject = objModelMail.Subject;
                mail.IsBodyHtml = true;
                mail.Body = objModelMail.Body;

                SmtpClient smtp = new SmtpClient
                {
                    Host = "smtp.gmail.com",
                    EnableSsl = true,
                    UseDefaultCredentials = false
                };

                smtp.Credentials = new NetworkCredential(from, passcode);
                smtp.Port = 587;
                smtp.Send(mail);
            }
        }
        private string SetBodyContent(List<CentralModel> List)
        {
            StringBuilder sb = new StringBuilder();
            List.GroupBy(x => x.Proposal_No).ToList().ForEach(x =>
            {
                var proposals = x.OrderBy(y => y.Record_ID).ToList();
                var spanL = "<span style=\"font-weight: 500;font-size: larger; COLOR: blueviolet;\">{0}</span>";
                var spanR = "<span style=\"font-weight: bolder;text-decoration: underline; font-size: larger; COLOR: blueviolet;\">{0}</span><br />";

                sb.AppendLine($"{string.Format(spanL, "Proposal#: ")}{string.Format(spanR, x.Key)}");
                sb.AppendLine($"{string.Format(spanR, "Previous log: ")}");

                sb.AppendLine($"{string.Format(spanL, "MOEFCC_File_No: ")}{string.Format(spanR, proposals.FirstOrDefault()?.MOEFCC_File_No)}");
                sb.AppendLine($"{string.Format(spanL, "Proposal_Status: ")}{string.Format(spanR, proposals.FirstOrDefault()?.Proposal_Status)}");

                sb.AppendLine($"{string.Format(spanL, "Important_Dates: ")}<br />");

                var impDates = proposals.FirstOrDefault()?.Important_Dates;

                if (!string.IsNullOrEmpty(impDates))
                {
                    foreach (var date in impDates.Split(','))
                    {
                        var datePart = date.Split(':');
                        sb.AppendLine($"{string.Format(spanL, datePart[0].Trim())}: {string.Format(spanR, datePart[1].Trim())}");
                    }
                }

                sb.AppendLine($"{string.Format(spanL, "Attached_Files: ")}");

                var files = proposals.FirstOrDefault()?.Attached_Files;

                if (!string.IsNullOrEmpty(files))
                {
                    foreach (var file in files.Split(','))
                    {
                        var filePart = file.Split('~');
                        //sb.AppendLine($"{string.Format(spanL, filePart[0].Trim())}: {string.Format(spanR, $"<a href='{(1 < filePart.Length ? filePart[1] : filePart[0].Trim())}'>{filePart[0].Trim()}</a>"});
                        sb.AppendLine($"<a href='{(1 < filePart.Length ? filePart[1] : filePart[0].Trim())}'>{filePart[0].Trim()}</a>");
                    }
                }
                sb.AppendLine($"<br />{string.Format(spanR, "New log: ")}");

                sb.AppendLine($"{string.Format(spanL, "MOEFCC_File_No: ")}{string.Format(spanR, proposals.LastOrDefault()?.MOEFCC_File_No)}");
                sb.AppendLine($"{string.Format(spanL, "Proposal_Status: ")}{string.Format(spanR, proposals.LastOrDefault()?.Proposal_Status)}");

                sb.AppendLine($"{string.Format(spanL, "Important_Dates: ")}<br />");

                impDates = proposals.LastOrDefault()?.Important_Dates;

                if (!string.IsNullOrEmpty(impDates))
                {
                    foreach (var date in impDates.Split(','))
                    {
                        var datePart = date.Split(':');
                        sb.AppendLine($"{string.Format(spanL, datePart[0].Trim())}: {string.Format(spanR, datePart[1].Trim())}");
                    }
                }

                sb.AppendLine($"{string.Format(spanL, "Attached_Files: ")}");

                files = proposals.LastOrDefault()?.Attached_Files;

                if (!string.IsNullOrEmpty(files))
                {
                    foreach (var file in files.Split(','))
                    {
                        var filePart = file.Split('~');
                        sb.AppendLine($"<a href='{(1 < filePart.Length ? filePart[1] : filePart[0].Trim())}'>{filePart[0].Trim()}</a>");
                    }
                }
                sb.AppendLine("<hr>");
                sb.AppendLine("<br />");
            });
           

            return string.Format("<html><body>{0}</body></html>", sb.ToString());
        }
    }
}