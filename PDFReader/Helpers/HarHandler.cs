using HarSharp;
using System;
using System.IO;

namespace PDFReader.Helpers
{
    internal class HarHandler
    {
        public static string GetJsonFromHARFile(string filePath) {
            try
            {
                string fileName = $"{filePath}";

                using (StreamWriter writer = new StreamWriter(fileName.ToString().Replace(".har", ".txt")))
                {
                    var har = HarConvert.DeserializeFromFile(fileName);

                    String strFinal = null;
                    String strTemp = null;
                    String[] strTempArr;

                    foreach (var entry in har.Log.Entries)
                    {
                        if (entry.Response.Content.MimeType == "application/json" &&
                            !entry.Response.Content.Text.Contains("indxnm") &&
                            !entry.Response.Content.Text.Contains("h.key") &&
                            !entry.Response.Content.Text.Contains("site_domain")&&
                            !entry.Response.Content.Text.Contains("beacon_url")&&
                            entry.Response.Content.Text.Contains("Table"))
                        {
                            strTemp = null;
                            strTemp = entry.Response.Content.Text; 
                            strFinal = strFinal + strTemp +","; 
                        }

                    }

                    if (strFinal != null)
                    {
                       strFinal = $"[{strFinal.Trim(',')}]";
                    }
                    writer.Flush();
                    return strFinal;
                }
            }
            catch (Exception ex)
            {
                return "";
            }
        }
    }
}