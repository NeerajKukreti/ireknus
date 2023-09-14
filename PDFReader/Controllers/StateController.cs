using Dapper;
using Newtonsoft.Json;
using PDFReader.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.UI;

namespace PDFReader.Controllers
{
    [Authorize]
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

        [HttpPost]
        public async Task<ActionResult> LoadStateData(string query)
        {
            string spname = "SP_PARIVESH_STATE";
            if (Request.QueryString["type"] != null) spname = "SP_PARIVESH_STATE_ALL";
            using (IDbConnection db = new SqlConnection(Connection.MyConnection()))
            {
                try
                {
                    List<StateModel> data =new List<StateModel>(await db.QueryAsync<StateModel>(spname, new { @query = query }, commandType: CommandType.StoredProcedure));
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

        [AllowAnonymous]
        public void StateAlert()
        {
            var data = StateDB.CheckandLogNewChanges().Result;

            if (data != null && data.Any())
            {
                SendMail(data);
            }
        }

        private void SendMail(List<StateModel> list)
        {
            var to = ConfigurationManager.AppSettings["Mail-to"];
            var from = ConfigurationManager.AppSettings["Mail-from"];
            var cc = ConfigurationManager.AppSettings["Mail-cc"];
            var bcc = ConfigurationManager.AppSettings["Mail-bcc"];
            var passcode = ConfigurationManager.AppSettings["Mail-passcode"];

            MailModel objModelMail = new MailModel
            {
                To = to,
                Subject = $"State Proposal Alert | {DateTime.Now.AddDays(0).ToString("dd/MM/yyyy")}",
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
        private string SetBodyContent(List<StateModel> List)
        {
            StringBuilder sb = new StringBuilder();
            List.GroupBy(x => x.Proposal_No).ToList().ForEach(x =>
            {
                var proposals = x.OrderBy(y => y.Record_ID).ToList();
                var spanL = "<span style=\"font-weight: 500;font-size: larger; COLOR: blueviolet;\">{0}</span>";
                var spanR = "<span style=\"font-weight: bolder;text-decoration: underline; font-size: larger; COLOR: blueviolet;\">{0}</span><br />";

                sb.AppendLine($"{string.Format(spanL, "Proposal#: ")}{string.Format(spanR, x.Key)}");
                sb.AppendLine($"{string.Format(spanR, "Previous log: ")}");

                sb.AppendLine($"{string.Format(spanL, "Company_or_Proponent: ")}{string.Format(spanR, proposals.FirstOrDefault()?.Company_or_Proponent)}");
                sb.AppendLine($"{string.Format(spanL, "File_No: ")}{string.Format(spanR, proposals.FirstOrDefault()?.File_No)}");
                sb.AppendLine($"{string.Format(spanL, "Current_status: ")}{string.Format(spanR, proposals.FirstOrDefault()?.Current_Status)}");

                sb.AppendLine($"{string.Format(spanL, "Important_Dates: ")}<br />");

                var impDates = proposals.FirstOrDefault()?.Important_Dates;

                if (!string.IsNullOrEmpty(impDates))
                {
                    foreach (var date in impDates.Split(','))
                    {
                        var datePart = date.Split(':');
                        if (datePart.Length == 1)
                            sb.AppendLine($"Date: {string.Format(spanR, datePart[0].Trim())}");
                        else if (datePart.Length == 2)
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

                sb.AppendLine($"{string.Format(spanL, "Company_or_Proponent: ")}{string.Format(spanR, proposals.LastOrDefault()?.Company_or_Proponent)}");
                sb.AppendLine($"{string.Format(spanL, "File_No: ")}{string.Format(spanR, proposals.LastOrDefault()?.File_No)}");
                sb.AppendLine($"{string.Format(spanL, "Current_status: ")}{string.Format(spanR, proposals.LastOrDefault()?.Current_Status)}");

                sb.AppendLine($"{string.Format(spanL, "Important_Dates: ")}<br />");

                impDates = proposals.LastOrDefault()?.Important_Dates;

                if (!string.IsNullOrEmpty(impDates))
                {
                    foreach (var date in impDates.Split(','))
                    {
                        var datePart = date.Split(':');

                        if(datePart.Length == 1)
                            sb.AppendLine($"Date: {string.Format(spanR, datePart[0].Trim())}");
                        else if (datePart.Length == 2)
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