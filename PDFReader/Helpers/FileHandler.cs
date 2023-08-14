using System;
using System.Configuration;
using System.IO;
using System.Web;

namespace PDFReader.Helpers
{
    public class FileHandler
    {
        public static string SaveUploadedFile(HttpRequestBase Request)
        {
            string fname;

            string finantialyear = Request.Form["FinantialYear"].ToString();

            if (Request.Files.Count > 0)
            {
                try
                {
                    HttpFileCollectionBase files = Request.Files;

                    HttpPostedFileBase file = files[0];

                    if (Request.Browser.Browser.ToUpper() == "IE" || Request.Browser.Browser.ToUpper() == "INTERNETEXPLORER")
                    {
                        string[] testfiles = file.FileName.Split(new char[] { '\\' });
                        fname = testfiles[testfiles.Length - 1];
                    }
                    else
                    {
                        fname = file.FileName;
                    }

                    var uploadPath = ConfigurationManager.AppSettings["UploadPath"].ToString();

                    // Get the complete folder path and store the file inside it.
                    var guidId = Guid.NewGuid();
                    fname = Path.Combine(HttpContext.Current.Server.MapPath(uploadPath), finantialyear + Path.GetExtension(file.FileName));

                    file.SaveAs(fname);

                    //Returns message that successfully uploaded

                    return (fname);
                }
                catch (Exception ex)
                {
                    return ("Error occurred. Error details: " + ex.Message);
                }
            }

            return "";
        }

        public static string SaveFile(HttpRequestBase Request, string FileFrom)
        {
            string fname;

            if (Request.Files.Count > 0)
            {
                try
                {
                    //  Get all files from Request object
                    HttpFileCollectionBase files = Request.Files;

                    HttpPostedFileBase file = files[0];

                    // Checking for Internet Explorer
                    if (Request.Browser.Browser.ToUpper() == "IE" || Request.Browser.Browser.ToUpper() == "INTERNETEXPLORER")
                    {
                        string[] testfiles = file.FileName.Split(new char[] { '\\' });
                        fname = testfiles[testfiles.Length - 1];
                    }
                    else
                    {
                        fname = file.FileName;
                    }

                    var uploadPath = ConfigurationManager.AppSettings["UploadPath"].ToString();

                    // Get the complete folder path and store the file inside it.                 
                    fname = Path.Combine(HttpContext.Current.Server.MapPath(uploadPath), FileFrom + Path.GetExtension(file.FileName));

                    file.SaveAs(fname);

                    //Returns message that successfully uploaded

                    return (fname);
                }
                catch (Exception ex)
                {
                    return ("Error occurred. Error details: " + ex.Message);
                }
            }

            return "";
        }

        public static bool UploadHARFile(HttpRequestBase Request)
        {
            string fname;

            if (Request.Files.Count > 0)
            {
                try
                {
                    //  Get all files from Request object
                    HttpFileCollectionBase files = Request.Files;

                    HttpPostedFileBase file = files[0];

                    // Checking for Internet Explorer
                    if (Request.Browser.Browser.ToUpper() == "IE" || Request.Browser.Browser.ToUpper() == "INTERNETEXPLORER")
                    {
                        string[] testfiles = file.FileName.Split(new char[] { '\\' });
                        fname = testfiles[testfiles.Length - 1];
                    }
                    else
                    {
                        fname = file.FileName;
                    }

                    var uploadPath = ConfigurationManager.AppSettings["UploadPath"].ToString();

                    // Get the complete folder path and store the file inside it.                 
                    fname = Path
                        .Combine(HttpContext.Current.Server.MapPath(uploadPath), "annoucements" + Path.GetExtension(file.FileName));

                    file.SaveAs(fname);

                }
                catch (Exception ex)
                {
                    return false;
                }
            }
            return (true);
        }
    }
}