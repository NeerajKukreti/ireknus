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
                            !entry.Response.Content.Text.Contains("site_domain"))
                        {
                            strTemp = null;
                            strTemp = entry.Response.Content.Text;

                            strTemp = strTemp.Replace("{\"Table\":[", "");

                            string[] stringSeparators = new string[] { "}]," };

                            strTempArr = strTemp.Split(stringSeparators, StringSplitOptions.RemoveEmptyEntries);

                            strFinal = strFinal + strTempArr[0] + "},";
                        }

                    }

                    if (strFinal != null)
                    {
                        strFinal = strFinal.Remove(strFinal.Length - 1, 1);
                        strFinal = "{\"Table\":[" + strFinal + "]}";
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